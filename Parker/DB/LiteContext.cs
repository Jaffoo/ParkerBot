using Microsoft.EntityFrameworkCore;

namespace ParkerBot
{
    public class LiteContext : DbContext
    {
        private string _conStr ="DataSource="+ Environment.CurrentDirectory + @"/data/config.db";
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlite(_conStr);
            
        } 
        public DbSet<Message> Messages=>Set<Message>();
        public DbSet<Config> Config => Set<Config>();
        public DbSet<Idol> Idol => Set<Idol>();
        public DbSet<Caches> Caches => Set<Caches>();
        public DbSet<Logs> Logs => Set<Logs>();
    }
}
