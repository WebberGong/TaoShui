using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Entity;

namespace Repository
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext()
            : base("DatabaseConnection")
        {
            Database.SetInitializer(
                new CreateTablesIfNotExists<DatabaseContext>());
            Configuration.LazyLoadingEnabled = false;
        }

        public DbSet<WebSiteSetting> WebSiteSettings { get; set; }
    }
}
