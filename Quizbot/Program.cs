using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Quizbot.Helpers;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Quizbot
{

    class Program
    {
        private static readonly Logger Logger = LogManager.GetLogger("Quizbot");
        private static Random random = new Random();

        private const string Path = "QuizbotConfig.json";
        public static BotConfig config = new BotConfig();
        public static IWebDriver driver;
        public static WebDriverWait waiter;

        public static HttpClient httpClient;

        public static async Task Main()
        {
            NLogManager.ConfigureNLog();

            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.ConnectionClose = false;
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "pl,en-US;q=0.7,en;q=0.3");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Host", "quizizz.com");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:75.0) Gecko/20100101 Firefox/75.0");

            if (File.Exists(Path))
                config = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText(Path));

            Logger.Info("Uruchamianie Quizbot");
            Logger.Debug("Nick: " + config.nickname);

            string code = "";

            do
            {
                Console.Write("Wpisz Gamecode >> "); code = Console.ReadLine();
            } while (code == "" || !int.TryParse(code, out _));

            string needs_login;
            do
            {
                Console.Write($"Quiz wymaga logowania? (Tt/Nn, {(config.credentials.needs_login ? "T" : "N")}),  >>");
                needs_login = Console.ReadLine().ToUpper();
            } while (needs_login != "" && needs_login != "T" && needs_login != "N");

            if (needs_login == "T")
                config.credentials.needs_login = true;
            else
                config.credentials.needs_login = false;

            Logger.Debug("Quiz wymaga logowania: " + (config.credentials.needs_login ? "TAK" : "NIE"));

            File.WriteAllText(Path, JsonConvert.SerializeObject(config, Formatting.Indented));

            Logger.Debug("Uruchamianie Chrome");
            ChromeOptions options = new ChromeOptions();
            options.SetLoggingPreference("performance", OpenQA.Selenium.LogLevel.All);
            options.AddArgument("mute-audio");

            driver = new ChromeDriver(options);
            waiter = new WebDriverWait(driver, TimeSpan.FromSeconds(999999));

            Logger.Debug("Przechodzenie na Quizizz");
            driver.Navigate().GoToUrl("https://quizizz.com/join/");
            Wait(".check-room-input");

            //Wpisywanie Gamecode
            Logger.Debug("Wpisywanie Gamecode");
            Write(".check-room-input", code);
            Click(".check-room-button");

            //Logowanie jeżeli wymagane
            if (config.credentials.needs_login)
            {
                Logger.Debug("Logowanie");
                Wait(".footer-login-btn");
                Click(".footer-login-btn");
                driver.FindElement(By.XPath("//input[@placeholder='johndoe@company.com']")).SendKeys(config.credentials.username);
                Write(".auth-input[type=\'password\']", config.credentials.password);
                Click(".login-submit-btn");
            }

            Wait(".enter-name-field");
            if (!config.credentials.needs_login || config.force_name)
            {
                Write(".enter-name-field", config.nickname);
            }

            Logger.Debug("Wchodzenie do lobby/gry");
            Click(".start-game");

            await Task.Delay(2000);

            Logger.Debug("Uzyskiwanie odpowiedzi");
            string gameid = QuizizzHelper.GetQuizizzGameID();
            Logger.Trace($"GameID: {gameid}");

            AnswersRoot answers = JsonConvert.DeserializeObject<AnswersRoot>(await QuizizzHelper.RequestQuizizzAnswers(gameid));
            Logger.Info($"Nazwa Quizu: {answers.data.quiz.info.name}, ilość pytań: {answers.data.quiz.info.questions.Count}");

            PrintAnswers(answers, true);

            Logger.Info("Oczekiwanie na rozpoczęcie gry");

            Wait(".question-text-color");

            Logger.Info("Gra rozpoczęta");

            for (int idx = 0; idx < answers.data.quiz.info.questions.Count; idx++)
            {
                Wait(".question-text-color");
                Wait(".options-container");
                waiter.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(".dummy-content")));
                try
                {
                    driver.FindElement(By.CssSelector(".powerup-onboarding-button"));
                    Click(".powerup-onboarding-button");
                }

                catch (NoSuchElementException) { }
                string quizizz_question = driver.FindElement(By.CssSelector(".question-text-color")).GetAttribute("innerHTML").Replace("&nbsp;", " ");

                Question found_question = answers.data.quiz.info.questions.Find(x => x.structure.query.text.Equals(quizizz_question));

                Logger.Trace($"Wykryte pytanie: {quizizz_question}");
                Logger.Trace($"Odnalezione pytanie? {(found_question != null ? "TAK" : "NIE")}");

                if (found_question == null)
                {
                    Logger.Error("NIE DOPASOWANO PYTANIA! ODPOWIEDZ I WCIŚNIJ enter ABY KONTYNUOWAĆ..");
                    PrintAnswers(answers);
                    Console.ReadLine();
                    Logger.Info("Wznawianie pracy");
                }
                else
                {
                    switch (found_question.type)
                    {
                        case "MCQ":
                        case "MSQ":
                            HandleClickable(found_question);
                            break;

                        case "BLANK":
                            HandleInsert(found_question);
                            break;

                        case "OPEN":
                            Logger.Warn("OTWARTE PYTANIE. Wciśnij enter NA NASTĘPNYM PYTANIU");
                            Console.ReadLine();
                            Logger.Info("Wznawianie pracy");
                            break;
                    }

                }
                if (config.rush_quiz)
                    WaitUntilInsisible(".dummy-content");
            }

            Logger.Warn("Koniec :)");
            new ManualResetEvent(false).WaitOne();
        }

        private static List<string> GetAnswers(Question question)
        {
            List<string> answers = new List<string>();
            switch (question.type)
            {
                case QuizizzHelper.FILL_QUESTION:
                    question.structure.options.ForEach(x => answers.Add(x.text));
                    break;

                case QuizizzHelper.MULTI_QUESTION:
                case QuizizzHelper.SINGLE_QUESTION:
                    List<long> adxx = new List<long>();
                    if (question.structure.answer?.GetType() == typeof(JArray))
                        adxx = ((JArray)question.structure.answer).ToObject<List<long>>();
                    else
                        adxx.Add((long)question.structure.answer);

                    foreach (int adxxx in adxx)
                    {
                        Option x = question.structure.options[adxxx];

                        if (x.type == "text")
                            answers.Add(x.text);
                        else if (x.type == "image")
                            answers.Add(x.media[0].url);
                    }
                    break;

                case QuizizzHelper.WO_ANSWER_QUESTION:
                    answers.Add("--Otwarte pytanie. Brak Odpowiedzi--");
                    break;

                default:
                    answers.Add("--co tu sie wydarzylo--");
                    break;
            }
            return answers;

        }
        private static void HandleInsert(Question found_question)
        {
            List<string> answers = GetAnswers(found_question);

            Logger.Info($"Pytanie: {found_question.structure.query.text} - {string.Join(", ", answers)}");
            Write(".typed-option-input", answers[0]);
            if (!config.rush_quiz)
            {
                Logger.Warn("Wciśnij ENTER na nastepnym pytaniu");
                Console.ReadLine();
            }
            else
            {
                Wait(".submit-button");
                Click(".submit-button");
            }
        }

        private static void HandleClickable(Question found_question)
        {
            List<string> answers = new List<string>(GetAnswers(found_question));

            Logger.Info($"Pytanie: {found_question.structure.query.text} - {string.Join(", ", answers)}");

            foreach (IWebElement e in driver.FindElement(By.CssSelector(".options-container")).FindElements(By.CssSelector(".option")))
            {
                try
                {
                    if (answers.Contains(e.FindElement(By.CssSelector(".resizeable")).GetAttribute("innerHTML")))
                    {
                        Logger.Info($"Zaznaczono tekst: {e.FindElement(By.CssSelector(".resizeable")).GetAttribute("innerHTML")}");
                        e.Click();
                    }
                }
                catch (NoSuchElementException) //Obraz
                {
                    foreach (string x in answers)
                    {
                        if (e.FindElement(By.CssSelector(".option-image")).GetAttribute("style").ToLower().Contains(x))
                        {
                            Logger.Info($"Zaznaczono obrazek: {x}");
                            e.Click();
                        }
                    }
                }
            }

            if (found_question.type == QuizizzHelper.MULTI_QUESTION)
            {
                Wait(".submit-button");
                Click(".submit-button");
            }


            if (!config.rush_quiz)
            {
                int x = random.Next(10, 15);
                Logger.Debug($"Oczekiwanie {x} sekund");
                Thread.Sleep(TimeSpan.FromSeconds(x));
            }
        }

        private static void PrintAnswers(AnswersRoot a, bool tofile = false)
        {
            string filename = $"odpowiedzi/QUIZIZZ_ODPOWIEDZI_{new Regex("[^a-zA-Z0-9\\.\\-]").Replace(a.data.quiz.info.name, "_")}.html";
            string p = "" +
                "<!DOCTYPE html>" +
                "<html>" +
                "<style> p { display: inline } </style>" +
                "<head>" +
                "<meta charset=\"UTF-8\">" +
                "</head>" +
                "<body>";

            foreach (Question q in a.data.quiz.info.questions)
            {
                Logger.Info(q.structure.query.text);
                p += $"<b>{q.structure.query.text}</b>: {string.Join(", ", GetAnswers(q))}\n<hr>\n";
            }
            p += "</body></html>";
            if (tofile)
            {
                File.WriteAllText(filename, p);
                Process.Start(Directory.GetCurrentDirectory() + "/" + filename);
            }

            Console.WriteLine(p);
        }
        private static void Write(string selector, string text) => driver.FindElement(By.CssSelector(selector)).SendKeys(text);
        private static void Click(string selector) => driver.FindElement(By.CssSelector(selector)).Click();
        private static void Wait(string selector) => waiter.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.CssSelector(selector)));
        private static void WaitUntilInsisible(string selector) => waiter.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(selector)));
    }


    public class BotConfig
    {
        public bool force_name;
        public bool rush_quiz = false;
        public string nickname = "hello";
        public Credentials credentials = new Credentials();

        public class Credentials
        {
            public bool needs_login = false;
            public string username;
            public string password;
        }
    }
}
