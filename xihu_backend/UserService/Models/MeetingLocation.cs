// File: Models/MeetingLocation.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models
{
    /// <summary>
    /// 会议地点指南信息
    /// </summary>
    [Table("meeting_locations")] // 映射数据库表名
    public class MeetingLocation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// 地点名称
        /// </summary>
        [Required]
        [MaxLength(255)]
        [Column("name")]
        public string Name { get; set; }

        /// <summary>
        /// 纬度（精确到小数点后8位）
        /// </summary>
        [Column("latitude", TypeName = "decimal(10,8)")]
        public double Latitude { get; set; }

        /// <summary>
        /// 经度（精确到小数点后8位）
        /// </summary>
        [Column("longitude", TypeName = "decimal(11,8)")]
        public double Longitude { get; set; }

        /// <summary>
        /// 从主要机场1的路线指南
        /// 示例："从浦东机场打车约30分钟"
        /// </summary>

        [MaxLength(500)]
        [Column("route_from_airport_1")]
        public string RouteFromAirport1 { get; set; }

        /// <summary>
        /// 从主要机场2的路线指南
        /// 示例："从虹桥机场乘地铁约1小时"
        /// </summary>

        [MaxLength(500)]
        [Column("route_from_airport_2")]
        public string RouteFromAirport2 { get; set; }

        /// <summary>
        /// 记录创建时间（自动设置）
        /// </summary>
        [Column("created_at")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 记录最后更新时间
        /// </summary>
        [Column("updated_at")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? UpdatedAt { get; set; }
    }
}