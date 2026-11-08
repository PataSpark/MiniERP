
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MiniERP.Data
{
    public class PostgreSqlDbContextFactory
        : IDesignTimeDbContextFactory<PostgreSqlApplicationDbContext>
    {
        public PostgreSqlApplicationDbContext CreateDbContext(
            string[] args)
        {
            var connectionString =
                Environment.GetEnvironmentVariable(
                    "MINIERP_POSTGRES_CONNECTION");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "找不到 MINIERP_POSTGRES_CONNECTION 環境變數。");
            }

            var options = new DbContextOptionsBuilder<
                PostgreSqlApplicationDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            return new PostgreSqlApplicationDbContext(options);
        }
    }
}
