using Auth.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Api.Data.EntityMapping
{
    public class ApplicationUserMapping : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(x => x.UserName).IsRequired();
            builder.Property(x => x.FullName).IsRequired();
            builder.Property(x => x.NationalCode).IsRequired(false).HasMaxLength(10);
            builder.Property(x => x.CreatedAt).IsRequired();
        }
    }
}
