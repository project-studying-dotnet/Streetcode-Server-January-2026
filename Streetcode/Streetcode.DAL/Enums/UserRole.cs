namespace Streetcode.DAL.Enums
{
    [Flags]
    public enum UserRole
    {
        User = 1,
        Moderator = 2,
        Administrator = 4,
        MainAdministrator = 8
    }
}
