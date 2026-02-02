namespace DbUpdate
{
    using DbUp;
    using Microsoft.Extensions.Configuration;

    public class Program
    {
        static int Main(string[] args)
        {
            var streetcodeBasePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            var migrationPath = Path.Combine(streetcodeBasePath, "Streetcode.DAL", "Persistence", "ScriptsMigration");

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Local";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(streetcodeBasePath, "Streetcode.WebApi"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables("STREETCODE_")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var upgrader =
                DeployChanges.To
                    .SqlDatabase(connectionString)
                    .WithScriptsFromFileSystem(migrationPath)
                    .LogToConsole()
                    .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Error);
                Console.ResetColor();
#if DEBUG
                Console.ReadLine();
#endif
                return -1;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Success!");
            Console.ResetColor();
            return 0;
        }
    }
}