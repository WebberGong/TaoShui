using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using Repository.Dto;

namespace Repository
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext()
            : base("DatabaseConnection")
        {
            Database.SetInitializer(
                new CreateTablesIfNotExists<DatabaseContext>());
            Configuration.LazyLoadingEnabled = true;
        }

        public DbSet<WebSiteSettingDto> WebSiteSettings { get; set; }

        public DbSet<WebSiteDto> WebSites { get; set; }
    }
}