using Newtonsoft.Json;
using NLog;
using System;
using System.IO;

namespace Quizbot.Helpers
{
    class QuizizzHelper
    {
        private static readonly Logger Logger = LogManager.GetLogger("Quizizz Helper");
        public const string SINGLE_QUESTION = "MCQ", MULTI_QUESTION = "MSQ", FILL_QUESTION = "BLANK", WO_ANSWER_QUESTION = "OPEN";


        //public static async Task<string> RequestQuizizzAnswers(string quizID)
        //{
        //    Uri x = new Uri($"https://quizizz.com/quiz/{quizID}/");
        //    Logger.Trace($"Getting Answers from {x}");
        //    string responseString = await (await Program.httpClient.GetAsync(x)).Content.ReadAsStringAsync();
        //    Logger.Trace(responseString);

        //    //return File.ReadAllText("force.txt");
        //    return responseString;
        //}

        public static Room RequestQuizizzAnswers()
        {
            //File.ReadLines("force.txt")
            //
            File.WriteAllText("x.txt", string.Join("\n", Program.driver.Manage().Logs.GetLog("performance")));
            foreach (var entry in Program.driver.Manage().Logs.GetLog("performance"))
            {
                string line = entry.ToString();

                if (line.Contains("5e89fe9b241632001b16bddd"))
                    Console.WriteLine(line);

                if (line.Contains("\\\"room\\\""))
                {
                    //Console.WriteLine(line);
                    string s = line.Substring(line.IndexOf("{\"message\""));
                    //Console.WriteLine(s);
                    PayloadRoot root = JsonConvert.DeserializeObject<PayloadRoot>(s);
                    //Console.WriteLine();
                    //Console.WriteLine();
                    s = root.message.@params.response.payloadData;
                    s = s.Substring(s.IndexOf("{"));
                    s = s.Remove(s.Length - 1);
                    File.WriteAllText("ff.txt", s);
                    QuizRoot qroot = JsonConvert.DeserializeObject<QuizRoot>(s);
                    return qroot.room;
                }
            }


            return null;
        }

        //public static string GetQuizizzGameID()
        //{
        //    File.WriteAllText("test.txt", string.Join("\n", Program.driver.Manage().Logs.GetLog("performance")));
        //    foreach (var entry in Program.driver.Manage().Logs.GetLog("performance"))
        //    {
        //        string line = entry.ToString();

        //        if (line.Contains("\\\"code\\\":\\\""))
        //        {
        //            return line.Substring(line.IndexOf("quizId=") + 7, 24);
        //        }
        //    }

        //    throw new NotFoundException();
        //}
    }
}
