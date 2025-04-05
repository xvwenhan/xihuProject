using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models // 确保命名空间与项目一致
{
    public class User
    {
        // 主键，自动递增Id
        [Key] // 指定为主键
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // 自动生成唯一值
        public int Id { get; set; }

        // 用户名，非空约束
        [Required(ErrorMessage = "Username is required")]
        [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters")] // 限制最大长度
        public string Username { get; set; }

        //邮箱
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; }


        // 密码哈希，非空约束
        [Required(ErrorMessage = "PasswordHash is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")] // 最小长度验证
        public string PasswordHash { get; set; }

        // 用户角色，默认值为 "User"
        [Required(ErrorMessage = "Role is required")]
        [MaxLength(20, ErrorMessage = "Role cannot exceed 20 characters")] // 限制最大长度
        public string Role { get; set; } = "User";

        // 添加时间戳，可选字段
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 默认设置为当前时间

        // 是否为有效用户，逻辑删除标记
        public bool IsActive { get; set; } = true;

        [MaxLength(20, ErrorMessage = "phone cannot exceed 50 characters")] // 限制最大长度
        public string? phone { get; set; }

        [MaxLength(100, ErrorMessage = "company cannot exceed 50 characters")] // 限制最大长度
        public string? company { get; set; }
        [MaxLength(100, ErrorMessage = "department cannot exceed 50 characters")] // 限制最大长度
        public string? department { get; set; }
        [MaxLength(50, ErrorMessage = "position cannot exceed 50 characters")] // 限制最大长度
        public string? position { get; set; }
        // 微信相关字段
        [MaxLength(50)]
        public string? WeChatOpenId { get; set; }

        [MaxLength(50)]
        public string? WeChatUnionId { get; set; }

        [Required]
        [MaxLength(20)]
        public string LoginType { get; set; } = "Normal"; // Normal:普通登录, WeChat:微信登录
    }
}
