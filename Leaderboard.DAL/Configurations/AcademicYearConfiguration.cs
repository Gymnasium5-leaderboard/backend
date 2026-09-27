using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Leaderboard.DAL.Configurations;

public class AcademicYearConfiguration : IEntityTypeConfiguration<AcademicYear>
{
    public void Configure(EntityTypeBuilder<AcademicYear> builder)
    {
        builder.Property(x => x.Id).ValueGeneratedOnAdd().HasIdentityOptions(startValue: 2); // 1 is seeded
        builder.Property(x => x.Title).IsRequired().HasMaxLength(EntityConstraints.AcademicYearTitleMaxLength);
        builder.Property(x => x.StartedAt).IsRequired();
        builder.Property(x => x.FinishedAt);
        builder.Ignore(x => x.IsCurrent);

        builder.HasIndex(x => x.Title).IsUnique();

        // First academic year. The next ones are opened by an owner (StartNewAsync)
        builder.HasData(new AcademicYear
        {
            Id = 1,
            Title = "2026/2027",
            StartedAt = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc)
        });
        // Only one current year: unique partial index on a constant, created in the migration via SQL
    }
}
