using Microsoft.EntityFrameworkCore;
using Zephyr.Entities;

namespace Zephyr.Db
{
    public class ZephyrDbContext : DbContext
    {
        public ZephyrDbContext(DbContextOptions<ZephyrDbContext> options) : base(options)
        {
        }
        public DbSet<Metar> Metars { get; set; }
    }
}
