namespace UserService.DTOs
{
    public class UserProfile
    {
        public string name  { get; set; }
        public string phone { get; set; }
        public string company { get; set; }
        public string department { get; set; }
        public string position { get; set; }
        public string email { get; set; }
        public string role { get; set; }
    }

    public class SetUserProfile
    {
        public string? name { get; set; }
        public string? phone { get; set; }
        public string? company { get; set; }
        public string? department { get; set; }
        public string? position { get; set; }
        public string? email { get; set; }
    }
}
