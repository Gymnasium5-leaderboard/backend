using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Leaderboard.DAL.Configurations;

public class ScoreTransactionConfiguration : IEntityTypeConfiguration<ScoreTransaction>
{
    public void Configure(EntityTypeBuilder<ScoreTransaction> builder)
    {
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Delta).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(EntityConstraints.ScoreDescriptionMaxLength);
        builder.Property(x => x.CreatedAt).IsRequired();

        // NULLs are distinct in PostgreSQL, so requests without a key never conflict
        builder.HasIndex(x => x.IdempotencyKey).IsUnique();
        builder.HasIndex(x => new { x.AcademicYearId, x.StudentId })
            .IncludeProperties(x => new { x.Delta, x.CreatedAt });

        builder.HasOne(x => x.AcademicYear)
            .WithMany()
            .HasForeignKey(x => x.AcademicYearId)
            .HasPrincipalKey(x => x.Id);
        builder.HasOne(x=>x.Student)
            .WithMany()
            .HasForeignKey(x=>x.StudentId)
            .HasPrincipalKey(x=>x.Id);
        builder.HasOne(x=> x.Owner)
            .WithMany()
            .HasForeignKey(x=>x.OwnerId)
            .HasPrincipalKey(x=>x.Id);
        
        builder.ToTable(t => t.HasCheckConstraint("CK_ScoreTransactions_Delta", "\"Delta\" <> 0"));
    }
}
