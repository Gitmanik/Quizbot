namespace Quizbot
{
    public class Response
    {
        public bool mask { get; set; }
        public int opcode { get; set; }
        public string payloadData;
    }

    public class Params
    {
        public string requestId { get; set; }
        public Response response { get; set; }
        public double timestamp { get; set; }
    }

    public class Message
    {
        public string method { get; set; }
        public Params @params { get; set; }
    }

    public class PayloadRoot
    {
        public Message message { get; set; }
        public string webview { get; set; }
    }


}
