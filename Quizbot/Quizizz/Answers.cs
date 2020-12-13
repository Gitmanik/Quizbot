using System.Collections.Generic;

namespace Quizbot
{
    public class Query
    {
        public string text { get; set; }
        public List<Media> media;
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
    }

    public class Question
    {
        public string type { get; set; }
        public Structure structure { get; set; }
    }
}
