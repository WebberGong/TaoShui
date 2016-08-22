using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Repository.Entity;

namespace Repository
{
    public class CreateTablesIfNotExists<TContext> : IDatabaseInitializer<TContext> where TContext : DatabaseContext
    {
        public void InitializeDatabase(TContext context)
        {
            bool isDbExists;
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                isDbExists = context.Database.Exists();
            }
            if (isDbExists)
            {
                int numberOfTable = context.Database.SqlQuery<int>(
                    "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='WebSiteSetting'").Sum();
                var isCreateTable = numberOfTable == 0;
                if (isCreateTable)
                {
                    string sql = @"CREATE TABLE WebSiteSetting (
                        Id varchar (50),
	                    Name varchar (50),
	                    Url varchar(50), 
	                    CaptchaLength INTEGER,
	                    LoginTimeOut INTEGER,
	                    GrabDataInterval INTEGER
                    );";
                    var result = context.Database.ExecuteSqlCommand(sql);
                    if (result > -1)
                    {
                        var e1 = new WebSiteSetting
                        {
                            Name = "沙巴",
                            Url = "http://www.maxbet.com/",
                            CaptchaLength = 4,
                            GrabDataInterval = 1,
                            LoginTimeOut = 10
                        };
                        var e2 = new WebSiteSetting
                        {
                            Name = "智博",
                            Url = "http://www.isn99.com/",
                            CaptchaLength = 4,
                            GrabDataInterval = 1,
                            LoginTimeOut = 10
                        };
                        context.WebSiteSettings.Add(e1);
                        context.WebSiteSettings.Add(e2);
                        context.SaveChanges();
                    }
                }
            }
            else
            {
                throw new ApplicationException("No database instance");
            }
        }
    }
}
