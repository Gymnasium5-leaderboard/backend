using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Leaderboard.DAL.Configurations;

public class SchoolClassConfiguration : IEntityTypeConfiguration<SchoolClass>
{
    public void Configure(EntityTypeBuilder<SchoolClass> builder)
    {
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Grade).IsRequired();
        builder.Property(x => x.Letter).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Ignore(x => x.DisplayName);

        builder.HasIndex(x => new { x.Grade, x.Letter }).IsUnique().HasFilter("\"IsActive\"");

        builder.HasMany(x => x.Students)
            .WithOne(x => x.Class)
            .HasForeignKey(x => x.ClassId)
            .HasPrincipalKey(x => x.Id);

        builder.ToTable(t => t.HasCheckConstraint("CK_SchoolClasses_Grade",
            $"\"Grade\" BETWEEN {EntityConstraints.MinGrade} AND {EntityConstraints.MaxGrade}"));
    }
}