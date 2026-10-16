using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Leaderboard.DAL.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(x => x.Id).ValueGeneratedOnAdd().HasIdentityOptions(startValue: 6); // 1-5 are seeded
        builder.Property(x => x.Name).IsRequired().HasMaxLength(EntityConstraints.CategoryNameMaxLength);
        builder.Property(x => x.IsArchived).IsRequired();

        // Names are unique ignoring case, archived ones too: unique index on lower("Name"), created in the
        // migration via SQL

        builder.HasData(
            new Category { Id = 1, Name = "Учёба" },
            new Category { Id = 2, Name = "Спорт" },
            new Category { Id = 3, Name = "Творчество" },
            new Category { Id = 4, Name = "Общественная деятельность" },
            new Category { Id = 5, Name = "Военно-патриотическая деятельность" }
        );
    }
}