using System;
using System.IO;


namespace Helper
{
    public static class DbConfig
    {
        public static string ConnectionString { get; private set; }

        static DbConfig()
        {
            var lines = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "dbconfig.ini"));
            string host = null, port = null, user = null, password = null, database = null;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("host=", StringComparison.OrdinalIgnoreCase))
                    host = trimmed.Split('=')[1];
                else if (trimmed.StartsWith("port=", StringComparison.OrdinalIgnoreCase))
                    port = trimmed.Split('=')[1];
                else if (trimmed.StartsWith("user=", StringComparison.OrdinalIgnoreCase))
                    user = trimmed.Split('=')[1];
                else if (trimmed.StartsWith("password=", StringComparison.OrdinalIgnoreCase))
                    password = trimmed.Split('=')[1];
                else if (trimmed.StartsWith("database=", StringComparison.OrdinalIgnoreCase))
                    database = trimmed.Split('=')[1];
            }

            ConnectionString = $"Host={host};Port={port};Database={database};Username={user};Password={password}";
        }
    }
}