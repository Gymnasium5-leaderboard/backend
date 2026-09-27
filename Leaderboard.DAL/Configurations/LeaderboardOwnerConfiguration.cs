using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Leaderboard.DAL.Configurations;

public class LeaderboardOwnerConfiguration : IEntityTypeConfiguration<LeaderboardOwner>
{
    public void Configure(EntityTypeBuilder<LeaderboardOwner> builder)
    {
        builder.Property(x => x.Id).ValueGeneratedOnAdd().HasIdentityOptions(startValue: 2); // 1 is seeded
        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(EntityConstraints.NameMaxLength);
        builder.Property(x => x.LastName).IsRequired().HasMaxLength(EntityConstraints.NameMaxLength);
        builder.Property(x => x.Login).IsRequired().HasMaxLength(EntityConstraints.LoginMaxLength);
        builder.Property(x => x.PasswordHash).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.LastModifiedAt);

        builder.HasIndex(x => x.Login).IsUnique();

        // First owner: admin / admin. The password must be changed after the first login.
        // The hash is a constant: PasswordHasher uses a random salt and would change the model on every build.
        builder.HasData(new LeaderboardOwner
        {
            Id = 1,
            Login = "admin",
            PasswordHash = "AQAAAAIAAYagAAAAEAS66X8L+GKN2U8Z/GrfEi57uN2ZGbLjTP3uwqL/UwAGiZD8q5g26dXt4dkokFQc5A==",
            FirstName = "Admin",
            LastName = "Admin",
            CreatedAt = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
