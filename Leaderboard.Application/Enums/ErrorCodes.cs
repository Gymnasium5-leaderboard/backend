namespace Leaderboard.Application.Enums;

public enum ErrorCodes
{
    //Data: 1-10
    //Auth: 11-20
    //Owner: 21-30
    //AcademicYear: 31-40
    //Class: 41-50
    //Student: 51-60
    //Score: 61-70

    InvalidProperty = 1,

    InvalidCredentials = 11,
    InvalidRefreshToken = 12,
    WrongCurrentPassword = 13,
    InvalidClaims = 14,

    OwnerNotFound = 21,
    OwnerAlreadyExists = 22,

    ClassNotFound = 41,
    ClassAlreadyExists = 42
}
