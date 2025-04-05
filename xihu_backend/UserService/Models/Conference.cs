using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Conference
{
    [Key]  // ����
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  // ����������������ݿ�֧�֣�
    public int ConferenceId { get; set; }

    [MaxLength(255)]
    public string? ConferenceName { get; set; }

    public DateTime? Date { get; set; }  // Date ���ͣ�C# �� `DateTime` Ҳ�ܴ洢����

    public TimeSpan? StartTime { get; set; }  // ��Ӧ SQL �� `time` ����

    public TimeSpan? EndTime { get; set; }  // ��Ӧ SQL �� `time` ����

    [MaxLength(255)]
    public string? Type { get; set; }

    [MaxLength(10)]
    public string? IsOnlyOffline { get; set; }  // �Ƽ��ĳ� `bool` ���ͣ����ֵ�� "true"/"false"

    [MaxLength(255)]
    public string? Location { get; set; }

    public int OfflineNum { get; set; }
    public int SubscribeNum { get; set; }

    [MaxLength(255)]
    public string? Url { get; set; }

}
