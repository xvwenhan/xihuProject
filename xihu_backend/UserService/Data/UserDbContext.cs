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
        }
    }
}
