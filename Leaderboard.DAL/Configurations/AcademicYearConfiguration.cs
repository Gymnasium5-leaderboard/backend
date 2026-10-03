using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Leaderboard.DAL.Configurations;

public class AcademicYearConfiguration : IEntityTypeConfiguration<AcademicYear>
{
    public void Configure(EntityTypeBuilder<AcademicYear> builder)
    {
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Title).IsRequired().HasMaxLength(EntityConstraints.AcademicYearTitleMaxLength);
        builder.Property(x => x.StartedAt).IsRequired();
        builder.Property(x => x.FinishedAt);
        builder.Ignore(x => x.IsCurrent);

        builder.HasIndex(x => x.Title).IsUnique();

        // The first year is created at startup by the current date (AcademicYearService.EnsureCurrentAsync),
        // the next ones are opened by an owner (StartNewAsync)

        // Only one current year: unique partial index on a constant, created in the migration via SQL
    }
}