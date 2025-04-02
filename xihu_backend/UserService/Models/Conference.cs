using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Conference
{
    [Key]  // 主键
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  // 自增主键（如果数据库支持）
    public int ConferenceId { get; set; }

    [MaxLength(255)]
    public string? ConferenceName { get; set; }

    public DateTime? Date { get; set; }  // Date 类型，C# 中 `DateTime` 也能存储日期

    public TimeSpan? StartTime { get; set; }  // 对应 SQL 的 `time` 类型

    public TimeSpan? EndTime { get; set; }  // 对应 SQL 的 `time` 类型

    [MaxLength(255)]
    public string? Type { get; set; }

    [MaxLength(10)]
    public string? IsOnlyOffline { get; set; }  // 推荐改成 `bool` 类型，如果值是 "true"/"false"

    [MaxLength(255)]
    public string? Location { get; set; }

    public int OfflineNum { get; set; }
    public int SubscribeNum { get; set; }

    [MaxLength(255)]
    public string? Url { get; set; }

}
