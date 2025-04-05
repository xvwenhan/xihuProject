using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models
{
    [Table("subscribe")] // 对应数据库表名
    public class Subscribe
    {
        [Key]
        [Column("user_id")]  // 映射数据库字段
        public int UserId { get; set; }

        [Key]
        [Column("conference_id")]  // 映射数据库字段
        public int ConferenceId { get; set; }

        // 导航属性
        public User User { get; set; }
        public Conference Conference { get; set; }
    }
}
