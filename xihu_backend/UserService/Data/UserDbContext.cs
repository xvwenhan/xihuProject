using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<VerificationCode> VerificationCodes { get; set; }
        public DbSet<VerificationCodeSend> VerificationCodeSends { get; set; }
        public DbSet<Conference> Conferences { get; set; }
        public DbSet<Subscribe> Subscribes { get; set; }
        public DbSet<PushSubscriptions> PushSubscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.phone).HasMaxLength(20);
                entity.Property(e => e.company).HasMaxLength(100);
                entity.Property(e => e.department).HasMaxLength(100);
                entity.Property(e => e.position).HasMaxLength(50);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.LoginType).IsRequired().HasMaxLength(20).HasDefaultValue("Normal");
                entity.Property(e => e.WeChatOpenId).HasMaxLength(50).IsRequired(false);
                entity.Property(e => e.WeChatUnionId).HasMaxLength(50).IsRequired(false);

                // 添加唯一索引
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<VerificationCode>(entity =>
            {
                entity.ToTable("verification_codes");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(6);

                entity.Property(e => e.Purpose)
                    .IsRequired();

                entity.Property(e => e.IsVerified)
                    .HasDefaultValue(false);

                entity.Property(e => e.VerifyAttempts)
                    .HasDefaultValue(0);

                entity.Property(e => e.ExpirationTime)
                    .IsRequired();

                entity.Property(e => e.CreateTime)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                entity.Property(e => e.UpdateTime);

                // 添加索引以优化查询
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.ExpirationTime);
                entity.HasIndex(e => new { e.Email, e.Purpose });
            });

            modelBuilder.Entity<VerificationCodeSend>(entity =>
            {
                entity.ToTable("verification_code_sends");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Purpose)
                    .IsRequired();

                entity.Property(e => e.SendTime)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                entity.Property(e => e.IpAddress)
                    .HasMaxLength(45);

                // 添加索引
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.SendTime);
                entity.HasIndex(e => new { e.Email, e.Purpose });
            });

            modelBuilder.Entity<Conference>(entity =>
            {
                entity.ToTable("conference");  // 映射数据库表名

                entity.HasKey(e => e.ConferenceId);

                entity.Property(e => e.ConferenceId)
                    .HasColumnName("conference_id");

                entity.Property(e => e.ConferenceName)
                    .HasColumnName("conference_name")
                    .HasMaxLength(255);

                entity.Property(e => e.Date)
                    .HasColumnName("date")
                    .HasColumnType("date");

                entity.Property(e => e.StartTime)
                    .HasColumnName("start_time")
                    .HasColumnType("time");

                entity.Property(e => e.EndTime)
                    .HasColumnName("end_time")
                    .HasColumnType("time");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasMaxLength(255);

                entity.Property(e => e.IsOnlyOffline)
                    .HasColumnName("is_only_offline")
                    .HasMaxLength(10);

                entity.Property(e => e.Location)
                    .HasColumnName("location")
                    .HasMaxLength(255);

                entity.Property(e => e.OfflineNum)  // 映射 offline_num 字段
                    .HasColumnName("offline_num")
                    .HasDefaultValue(0)  // 默认值 0
                    .IsRequired();  // 不能为空

                entity.Property(e => e.SubscribeNum)  // 映射 offline_num 字段
                    .HasColumnName("subscribe_num")
                    .HasDefaultValue(0)  // 默认值 0
                    .IsRequired();  // 不能为空

                entity.Property(e => e.Url)
                    .HasColumnName("url")
                    .HasMaxLength(255);
            });
            modelBuilder.Entity<Subscribe>(entity =>
            {
                entity.ToTable("subscribe");  // 映射数据库表名

                // 设置联合主键
                entity.HasKey(uc => new { uc.UserId, uc.ConferenceId });

                // 配置 UserId 的数据库字段映射
                entity.Property(e => e.UserId)
                      .HasColumnName("user_id");

                // 配置 ConferenceId 的数据库字段映射
                entity.Property(e => e.ConferenceId)
                      .HasColumnName("conference_id");

                // 配置外键约束
                entity.HasOne(uc => uc.User)
                    .WithMany()  // 一个用户可以有多个会议订阅
                    .HasForeignKey(uc => uc.UserId)
                    .OnDelete(DeleteBehavior.Cascade);  // 级联删除

                entity.HasOne(uc => uc.Conference)
                    .WithMany()  // 一个会议可以有多个订阅
                    .HasForeignKey(uc => uc.ConferenceId)
                    .OnDelete(DeleteBehavior.Cascade);  // 级联删除
            });
            modelBuilder.Entity<PushSubscriptions>(entity =>
            {
                entity.ToTable("PushSubscriptions");
                entity.HasKey(e => e.SubscriptionId);
                entity.Property(e => e.Endpoint).HasMaxLength(500);
                entity.Property(e => e.P256dhKey).HasMaxLength(500);
                entity.Property(e => e.AuthKey).HasMaxLength(500);
                entity.Property(e => e.UserId);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                // 配置外键约束
                entity.HasOne(uc => uc.User)
                    .WithMany()  // 一个用户可以有多个浏览器
                    .HasForeignKey(uc => uc.UserId)
                    .OnDelete(DeleteBehavior.Cascade);  // 级联删除

            });
        }
    }
}
