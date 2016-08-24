using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Transactions;
using Repository.Dto;

namespace Repository
{
    internal class CreateTablesIfNotExists<TContext> : IDatabaseInitializer<TContext> where TContext : DatabaseContext
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
                var webSiteSettingSql = @"CREATE TABLE IF NOT EXISTS [WebSiteSetting] (
	                    Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	                    Name varchar(50) NOT NULL,
	                    Url varchar(100) NOT NULL, 
	                    CaptchaLength INTEGER,
	                    LoginTimeOut INTEGER,
	                    GrabDataInterval INTEGER
                    );";
                context.Database.ExecuteSqlCommand(webSiteSettingSql);

                var maxbetSetting = new WebSiteSettingDto
                {
                    Name = "沙巴",
                    Url = "http://www.maxbet.com/",
                    CaptchaLength = 4,
                    GrabDataInterval = 1,
                    LoginTimeOut = 10
                };
                var isn99Setting = new WebSiteSettingDto
                {
                    Name = "智博",
                    Url = "http://www.isn99.com/",
                    CaptchaLength = 4,
                    GrabDataInterval = 1,
                    LoginTimeOut = 10
                };
                var pinnacleSetting = new WebSiteSettingDto
                {
                    Name = "平博",
                    Url = "https://www.pinnacle.com/zh-cn/",
                    CaptchaLength = 4,
                    GrabDataInterval = 1,
                    LoginTimeOut = 10
                };
                var sbobetSetting = new WebSiteSettingDto
                {
                    Name = "利记",
                    Url = "https://www.sbobet.com/zh-cn/betting.aspx",
                    CaptchaLength = 4,
                    GrabDataInterval = 1,
                    LoginTimeOut = 10
                };
                var webSiteSettings = new List<WebSiteSettingDto>
                {
                    maxbetSetting,
                    isn99Setting,
                    pinnacleSetting,
                    sbobetSetting
                };
                webSiteSettings.ForEach(x => context.WebSiteSettings.Add(x));
                context.SaveChanges();

                var webSiteSql = @"CREATE TABLE IF NOT EXISTS [WebSite] (
	                    Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	                    LoginName varchar(50) NOT NULL,
	                    Password varchar(50) NOT NULL,
                        SettingId INTEGER NOT NULL
                    );";
                context.Database.ExecuteSqlCommand(webSiteSql);

                var webSites = new List<WebSiteDto>
                {
                    new WebSiteDto
                    {
                        LoginName = "sfb1337952",
                        Password = "Aaaa2235",
                        SettingId = maxbetSetting.Id
                    },
                    new WebSiteDto
                    {
                        LoginName = "zb999111",
                        Password = "sss123456",
                        SettingId = isn99Setting.Id
                    },
                    new WebSiteDto
                    {
                        LoginName = "hh7d1hi061",
                        Password = "ss123456@",
                        SettingId = pinnacleSetting.Id
                    },
                    new WebSiteDto
                    {
                        LoginName = "hbhcgc3061",
                        Password = "sss123456",
                        SettingId = sbobetSetting.Id
                    }
                };
                webSites.ForEach(x => context.WebSites.Add(x));
                context.SaveChanges();
            }
            else
            {
                throw new ApplicationException("No database instance");
            }
        }
    }
}