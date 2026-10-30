using Leaderboard.Domain.Entities;

namespace Leaderboard.Tests.TestData;

internal static class ClassMother
{
    public static IQueryable<SchoolClass> GetClasses()
    {
        return new SchoolClass[]
        {
            new() { Id = 1, Grade = 11, Letter = 'А', ClassTeacherName = "Иванова Анна" },
            new() { Id = 2, Grade = 10, Letter = 'А', ClassTeacherName = "Петров Сергей" },
            new() { Id = 3, Grade = 7, Letter = 'А', ClassTeacherName = "Смирнова Ольга" },
            new() { Id = 4, Grade = 7, Letter = 'Б', ClassTeacherName = "Кузнецов Дмитрий" },
            new() { Id = 5, Grade = 11, Letter = 'Б', ClassTeacherName = "Попова Елена", IsActive = false }, // graduated
            new() { Id = 6, Grade = 5, Letter = 'В', ClassTeacherName = "Соколов Андрей" } // no students
        }.AsQueryable();
    }
}