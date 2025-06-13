using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models
{
    [Table("PushSubscriptions")]
    public class PushSubscriptions
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // 自动生成唯一值
        [Column("SubscriptionId")]
        public int SubscriptionId { get; set; }

        [Required(ErrorMessage = "userid is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "endpoint is required")]
        [MaxLength(500, ErrorMessage = "endpoint cannot exceed 50 characters")] // 限制最大长度
        public string Endpoint { get; set; }

        [Required(ErrorMessage = "P256dhKey is required")]
        [MaxLength(500, ErrorMessage = "P256dhKey cannot exceed 50 characters")] // 限制最大长度
        public string P256dhKey { get; set; }

        [Required(ErrorMessage = "AuthKey is required")]
        [MaxLength(500, ErrorMessage = "AuthKey cannot exceed 50 characters")] // 限制最大长度
        public string AuthKey { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 默认设置为当前时间

        // 导航属性
        public User User { get; set; }
    }
}
