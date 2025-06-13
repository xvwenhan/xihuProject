namespace VideoService.Models
{
    /// <summary>
    /// 视频摘要实体
    /// </summary>
    public class VideoSummary
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 会议ID
        /// </summary>
        public int MeetingId { get; set; }

        /// <summary>
        /// 视频开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 视频结束时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 原始文字内容
        /// </summary>
        public string OriginalText { get; set; }

        /// <summary>
        /// 摘要文字内容
        /// </summary>
        public string Summary { get; set; }
    }
}
