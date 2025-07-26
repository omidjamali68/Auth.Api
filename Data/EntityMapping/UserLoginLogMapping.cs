using Auth.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Api.Data.EntityMapping
{
    public class UserLoginLogMapping : IEntityTypeConfiguration<UserLoginLog>
    {
        public void Configure(EntityTypeBuilder<UserLoginLog> builder)
        {
            builder.ToTable("UserLoginLogs");

            builder.HasKey(x => x.LogId);
            builder.Property(x => x.LogId).ValueGeneratedOnAdd();

            builder.Property(x => x.UserId).IsRequired();

            builder.Property(x => x.LoginTime)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.LogoutTime)
                   .IsRequired(false);

            builder.Property(x => x.IpAddress)
                   .HasMaxLength(45)
                   .IsRequired(false);

            builder.Property(x => x.UserAgent)
                   .HasMaxLength(512)
                   .IsRequired(false);

            builder.Property(x => x.DeviceInfo)
                   .HasMaxLength(255)
                   .IsRequired(false);

            builder.Property(x => x.LoginStatus)
                   .IsRequired();

            builder.Property(x => x.LoginSource)
                   .IsRequired(false);

            builder.Property(x => x.Note)
                   .HasMaxLength(255)
                   .IsRequired(false);
        }
    }
}
