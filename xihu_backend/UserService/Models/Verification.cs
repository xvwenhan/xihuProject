using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models
{
    /// <summary>
    /// 验证码记录
    /// </summary>
    [Table("verification_codes")]
    public class VerificationCode
    {
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// 邮箱地址
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        /// <summary>
        /// 验证码
        /// </summary>
        [Required]
        [MaxLength(6)]
        public string Code { get; set; }

        /// <summary>
        /// 验证码用途
        /// </summary>
        [Required]
        public VerificationCodePurpose Purpose { get; set; }

        /// <summary>
        /// 是否已验证
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// 验证尝试次数
        /// </summary>
        public int VerifyAttempts { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime ExpirationTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
    }

    /// <summary>
    /// 验证码用途枚举
    /// </summary>
    public enum VerificationCodePurpose
    {
        /// <summary>
        /// 注册
        /// </summary>
        Registration = 1,

        /// <summary>
        /// 重置密码
        /// </summary>
        ResetPassword = 2,

        /// <summary>
        /// 修改邮箱
        /// </summary>
        ChangeEmail = 3
    }

    /// <summary>
    /// 验证码发送记录
    /// </summary>
    [Table("verification_code_sends")]
    public class VerificationCodeSend
    {
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// 邮箱地址
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        /// <summary>
        /// 验证码用途
        /// </summary>
        [Required]
        public VerificationCodePurpose Purpose { get; set; }

        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime SendTime { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        [MaxLength(45)]
        public string IpAddress { get; set; }
    }
}
