using Microsoft.EntityFrameworkCore;
using VideoService.Models;
using System.ComponentModel.DataAnnotations;

namespace VideoService.Data
{
    /// <summary>
    /// 视频服务数据库上下文
    /// </summary>
    public class VideoDbContext : DbContext
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options">数据库上下文选项</param>
        public VideoDbContext(DbContextOptions<VideoDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// 视频实体集
        /// </summary>
        public DbSet<Video> Videos { get; set; }
        public DbSet<Models.Stream> Streams { get; set; }

        public DbSet<VideoSummary> VideoSummaries { get; set; }

        /// <summary>
        /// 配置数据库模型
        /// </summary>
        /// <param name="modelBuilder">模型构建器</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置视频实体
            modelBuilder.Entity<Video>(entity =>
            {
                // 设置主键
                entity.HasKey(e => e.Id);

                // 设置标题索引
                entity.HasIndex(e => e.Title);

                // 设置用户ID索引
                entity.HasIndex(e => e.UserId);

                // 设置状态索引
                entity.HasIndex(e => e.Status);

                // 配置必填字段
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.FilePath)
                    .IsRequired();

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Format)
                    .HasMaxLength(10);

                // 配置处理进度范围
                entity.Property(e => e.ProcessingProgress)
                    .HasDefaultValue(0);

                entity.Property(e => e.UserId)
                    .IsRequired();
            });

            // 配置直播流实体
            modelBuilder.Entity<Models.Stream>(entity =>
            {
                // 设置主键
                entity.HasKey(e => e.ConferenceId);

                // 配置字段约束
                entity.Property(e => e.RoomId)
                    .HasMaxLength(20);

                entity.Property(e => e.ChannelId)
                    .HasMaxLength(20);

                entity.Property(e => e.LiveStatus)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .HasDefaultValue(LiveStatus.NOT_STARTED);

/*                // 外键约束
                entity.HasOne(e => e.Conference)
                    .WithOne()
                    .HasForeignKey<Stream>(e => e.ConferenceId)
                    .OnDelete(DeleteBehavior.Cascade);*/
            });


            modelBuilder.Entity<VideoSummary>(entity =>
            {
                // 设置主键
                entity.HasKey(e => e.Id);

                // 配置属性
                entity.Property(e => e.StartTime).IsRequired();
                entity.Property(e => e.EndTime).IsRequired();
                entity.Property(e => e.OriginalText).IsRequired();
                entity.Property(e => e.Summary).IsRequired();

                // 可选：你也可以为MeetingId添加索引，视情况而定
                // entity.HasIndex(e => e.MeetingId);
            });
        }
    }
} 