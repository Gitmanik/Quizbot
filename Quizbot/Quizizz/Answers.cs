using System.Collections.Generic;

namespace Quizbot
{
    public class Query
    {
        public string text { get; set; }
    }

    public class Media
    {
        public string url;
    }
    public class Option
    {
        public List<Media> media { get; set; }
        public string type { get; set; }
        public string text { get; set; }
    }

    public class Structure
    {
        public Query query { get; set; }
        public List<Option> options { get; set; }
        public object answer { get; set; }
    }

    public class Question
    {
        public string type { get; set; }
        public Structure structure { get; set; }
    }

    public class Info
    {
        public List<Question> questions { get; set; }
        public string name { get; set; }
    }

    public class Quiz
    {
        public Info info { get; set; }
    }

    public class Data
    {
        public Quiz quiz { get; set; }
    }

    public class AnswersRoot
    {
        public bool success { get; set; }
        public Data data { get; set; }
    }
}
