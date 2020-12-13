using System.Collections.Generic;

namespace Quizbot
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Room
    {
        public string name { get; set; }
        public Dictionary<string, Question> questions { get; set; }
    }

    public class Ua
    {
        public string family { get; set; }
        public string version { get; set; }
    }

    public class Os
    {
        public string family { get; set; }
        public string version { get; set; }
    }

    public class Metadata
    {
        public string type { get; set; }
        public string model { get; set; }
        public Ua ua { get; set; }
        public Os os { get; set; }
    }

    public class Player
    {
        public string id { get; set; }
        public Metadata metadata { get; set; }
    }

    public class QuizRoot
    {
        public string __cid__ { get; set; }
        public Room room { get; set; }
        public Player player { get; set; }
    }
}