using Auth.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Api.Data.EntityMapping
{
    public class IdentityVerificationCodeMapping : IEntityTypeConfiguration<IdentityVerificationCode>
    {
        public void Configure(EntityTypeBuilder<IdentityVerificationCode> builder)
        {
            builder.ToTable("VerificationCodes");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(_ => _.PhoneNumber).IsRequired();
            builder.Property(_ => _.VerificationCode).IsRequired();
            builder.Property(_ => _.Email).IsRequired(false);
        }
    }
}
