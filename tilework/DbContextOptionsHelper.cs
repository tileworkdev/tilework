using Microsoft.EntityFrameworkCore;

public static class DbContextOptionsHelper
{
    public static Action<DbContextOptionsBuilder> Configure(string connectionString)
    {
        return options => options.UseSqlite(connectionString);
    }
}
