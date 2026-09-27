using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Leaderboard.DAL.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(EntityConstraints.NameMaxLength);
        builder.Property(x => x.LastName).IsRequired().HasMaxLength(EntityConstraints.NameMaxLength);
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.LastModifiedAt);
    }
}
