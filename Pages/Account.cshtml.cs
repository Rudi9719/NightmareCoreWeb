using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace NightmareCoreWeb2.Pages
{
    public class AccountModel : PageModel
    {
        string connStr = $"SslMode=None;server={Program.MysqlServer};user={Program.MysqlUser};database={Program.MysqlDatabase};port={Program.MysqlPort};password={Program.MysqlPassword}";
        public string UserEmail { get; set; }
        public string UserPassword { get; set; }
        public string CharacterListType {get; set;}
        public string AuthToken { get; set; }
        public string Username {get; set;}
        
        public List<Character> OnlineCharacters = new List<Character>();

        private readonly ILogger<AccountModel> _logger;

        private MySqlConnection conn;
        public AccountModel(ILogger<AccountModel> logger)
        {
            
            conn = new MySqlConnection(connStr);
            _logger = logger;
        }
        public void OnGet()
        {

            ViewData["Title"] = "Login";
            AuthToken = Request.Cookies["AuthToken"];
            Username = Request.Cookies["Username"];
            if (!string.IsNullOrEmpty(Username)) {
                Account a = new Account(Username, conn);
                OnlineCharacters = a.characters;

                ViewData["Title"] = a.Username;
                CharacterListType = $"{a.Username}'s Characters";
            } 
        }
        public void OnPostLogin()
        {
            UserEmail = Request.Form["UserEmail"];
            UserPassword = Request.Form["UserPassword"];
            Username = UserEmail.Substring(0, UserEmail.IndexOf("@"));
            AuthToken = Hash($"{Username.ToUpper()}:{UserPassword.ToUpper()}");

            Response.Cookies.Append("Username", Username);
            Response.Cookies.Append("AuthToken", AuthToken);
        }

        static string Hash(string input)
        {
            var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }
    }
}
