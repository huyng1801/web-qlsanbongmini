using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace QLSanBongMini.Web.Data;

public static class DatabaseSetup
{
    public const string ProviderSqlServer = "SqlServer";
    public const string ProviderSqlite = "Sqlite";
    public const string ProviderAuto = "Auto";

    public static void ConfigureDbContext(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var provider = configuration["Database:Provider"] ?? ProviderAuto;
        var sqlServerConnection = configuration.GetConnectionString("SqlServer")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Thiếu connection string SqlServer hoặc DefaultConnection.");
        var sqliteConnection = configuration.GetConnectionString("Sqlite")
            ?? "Data Source=qlsanbongmini.db";

        var useSqlite = provider.Equals(ProviderSqlite, StringComparison.OrdinalIgnoreCase)
            || (provider.Equals(ProviderAuto, StringComparison.OrdinalIgnoreCase) && !CanConnectSqlServer(sqlServerConnection));

        if (useSqlite)
        {
            Console.WriteLine($"[DB] Dùng SQLite: {sqliteConnection}");
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(sqliteConnection));
        }
        else
        {
            Console.WriteLine("[DB] Dùng SQL Server.");
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(sqlServerConnection));
        }
    }

    private static bool CanConnectSqlServer(string connectionString)
    {
        try
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DB] Không kết nối được SQL Server, chuyển sang SQLite. ({ex.Message})");
            return false;
        }
    }
}
