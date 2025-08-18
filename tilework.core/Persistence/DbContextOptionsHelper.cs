using Microsoft.EntityFrameworkCore;
using EntityFramework.Exceptions.Sqlite;

namespace Tilework.Core;

public static class DbContextOptionsHelper
{
    public static Action<DbContextOptionsBuilder> Configure(string connectionString)
    {
        return options => options
            .UseSqlite(connectionString)
            .UseExceptionProcessor();
    }
}
