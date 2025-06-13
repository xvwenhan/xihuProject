using System;
using System.ComponentModel.DataAnnotations;

namespace VideoService.Models
{
    /// <summary>
    /// 视频实体模型
    /// </summary>
    public class Video
    {
        /// <summary>
        /// 视频ID
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 视频标题
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 视频描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 上传时间
        /// </summary>
        [Required]
        public DateTime UploadTime { get; set; }

        /// <summary>
        /// 上传用户ID
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// 视频处理状态
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// 视频时长
        /// </summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// 文件格式
        /// </summary>
        [MaxLength(10)]
        [Required]
        public string Format { get; set; } = string.Empty;

        /// <summary>
        /// 处理进度（0-100）
        /// </summary>
        [Range(0, 100)]
        public int ProcessingProgress { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 最后处理时间
        /// </summary>
        public DateTime? LastProcessedTime { get; set; }

        /// <summary>
        /// 视频转写文本
        /// </summary>
        public string? Transcription { get; set; }

        /// <summary>
        /// 视频文件路径
        /// </summary>
        [Required]
        public string FilePath { get; set; } = string.Empty;
    }
} 