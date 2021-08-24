using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;

namespace NightmareCoreWeb2
{
    public class Program
    {
        // "server=localhost;user=trinity;database=auth;port=3306;password=Baevannas335a";
        public static string MysqlServer;
        public static string MysqlUser;
        public static string MysqlDatabase;
        public static string MysqlPort;
        public static string MysqlPassword;
        public static void Main(string[] args)
        {
        using (StreamReader r = new StreamReader("config.json"))
        {
            string json = r.ReadToEnd();
            MysqlConfig config = JsonConvert.DeserializeObject<MysqlConfig>(json); 
            Program.MysqlServer = config.MysqlServer;
            Program.MysqlUser = config.MysqlUsername;
            Program.MysqlDatabase = config.MysqlDatabase;
            Program.MysqlPassword = config.MysqlPassword;
            Program.MysqlPort = config.MysqlPort;

        }
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
