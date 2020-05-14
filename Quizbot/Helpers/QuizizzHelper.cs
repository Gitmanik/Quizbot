using NLog;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quizbot.Helpers
{
    class QuizizzHelper
    {
        private static readonly Logger Logger = LogManager.GetLogger("Quizizz Helper");
        public const string SINGLE_QUESTION = "MCQ", MULTI_QUESTION = "MSQ", FILL_QUESTION = "BLANK", WO_ANSWER_QUESTION = "OPEN";


        public static async Task<string> RequestQuizizzAnswers(string quizID)
        {
            Uri x = new Uri($"https://quizizz.com/quiz/{quizID}/");
            Logger.Trace($"Getting Answers from {x}");
            string responseString = await (await Program.httpClient.GetAsync(x)).Content.ReadAsStringAsync();
            Logger.Trace(responseString);
            return responseString;
        }
        public static string GetQuizizzGameID()
        {
            foreach (var entry in Program.driver.Manage().Logs.GetLog("performance"))
            {
                string line = entry.ToString();
                if (line.Contains("quizId="))
                {
                    return line.Substring(line.IndexOf("quizId=") + 7, 24);
                }
            }
            throw new NotFoundException();
        }
    }
}
