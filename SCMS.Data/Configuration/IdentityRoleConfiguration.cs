using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SCMS.Data.Configuration
{
    public class IdentityRoleConfiguration : IEntityTypeConfiguration<IdentityRole>
    {
        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
            builder.HasData(
                new IdentityRole { Id = "a1b2c3d4-0001-0000-0000-000000000001", Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = "00000000-0000-0000-0000-000000000001" },
                new IdentityRole { Id = "a1b2c3d4-0002-0000-0000-000000000002", Name = "Editor", NormalizedName = "EDITOR", ConcurrencyStamp = "00000000-0000-0000-0000-000000000002" },
                new IdentityRole { Id = "a1b2c3d4-0003-0000-0000-000000000003", Name = "User", NormalizedName = "USER", ConcurrencyStamp = "00000000-0000-0000-0000-000000000003" }
            );
        }
    }
}
