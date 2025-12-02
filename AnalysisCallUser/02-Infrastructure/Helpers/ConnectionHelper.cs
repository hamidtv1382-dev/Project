namespace AnalysisCallUser._02_Infrastructure.Helpers
{
    public static class ConnectionHelper
    {
        public static string GetConnectionString(IConfiguration configuration, string name = "DefaultConnection")
        {
            var connectionString = configuration.GetConnectionString(name);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException($"Connection string '{name}' not found.");
            }
            return connectionString;
        }
    }
}
