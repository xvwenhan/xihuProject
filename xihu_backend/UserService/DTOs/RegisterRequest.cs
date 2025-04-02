namespace UserService.DTOs
{
    public class RegisterRequest
    {
        //public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Code { get; set; }  // 验证码
    }

    public class InternalRegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginRequest
    {
        public string Account { get; set; }  // 用户名或邮箱
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
        public UserInfo User { get; set; }
    }

    public class UserInfo
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }
}
