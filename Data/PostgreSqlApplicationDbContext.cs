
using Microsoft.EntityFrameworkCore;

namespace MiniERP.Data
{
    public class PostgreSqlApplicationDbContext : ApplicationDbContext
    {
        public PostgreSqlApplicationDbContext(
            DbContextOptions<PostgreSqlApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
