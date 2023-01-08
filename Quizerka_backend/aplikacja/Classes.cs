using Newtonsoft.Json;

namespace aplikacja
{
    public class User
    {
        public string? Login { get; set; }
        public string? Password { get; set; }
    }

    public class Quiz
    { 
        public string? pytanie { get; set; }
        public string? odpowiedz { get; set; }
    }

    public class Score
    {
        public int? user_id { get; set; }
        public int? score{ get; set; } 
    }

    public class UserInfo
    {
        public int? user_id { get; set; } 
    }
}
