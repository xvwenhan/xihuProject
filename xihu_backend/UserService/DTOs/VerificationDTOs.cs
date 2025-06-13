using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs
{
    public class SendVerificationCodeRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

//        [Required(ErrorMessage = "Purpose is required")]
        //public string Purpose { get; set; }
    }

    public class VerifyCodeRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Verification code must be 6 characters")]
        public string Code { get; set; }

//        [Required(ErrorMessage = "Purpose is required")]
//        public string Purpose { get; set; }
    }

    public class VerificationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
