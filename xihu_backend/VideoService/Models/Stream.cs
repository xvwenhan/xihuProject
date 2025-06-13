using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace VideoService.Models
{
    public enum LiveStatus
    {
        NOT_STARTED,
        LIVE,
        ENDED
    }

    [Table("Stream")]
    public class Stream
    {
        [Key]
        [Column("conference_id")]
        [ForeignKey("Conference")]
        public int ConferenceId { get; set; }

        [Column("room_id")]
        [MaxLength(20)]
        public string? RoomId { get; set; }

        [Column("channel_id")]
        [MaxLength(20)]
        public string? ChannelId { get; set; }

        [Column("live_status")]
        [Required]
        public LiveStatus LiveStatus { get; set; } = LiveStatus.NOT_STARTED;
    }
}
