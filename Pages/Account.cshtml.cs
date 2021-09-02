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
        public string UserEmail { get; set; }
        public string UserPassword { get; set; }
        public string CharacterListType { get; set; }
        public string AuthToken { get; set; }
        public string Username { get; set; }
        public bool IsGM { get; set; }
        public Account UserAccount { get; set; }

        public List<Character> OnlineCharacters = new List<Character>();
        public List<GMTicket> Tickets = new List<GMTicket>();

        private readonly ILogger<AccountModel> _logger;

        private MySqlConnection conn;
        public AccountModel(ILogger<AccountModel> logger)
        {

            conn = new MySqlConnection(Program.connStr);
            _logger = logger;
        }
        public void OnGetCharacterAction(int guid, int action)
        {
            Character c = new Character(guid);
            if ((c.AtLogin & Character.AtLoginOptions.AT_LOGIN_FIRST) == 0)
            {
                c.AtLogin |= (Character.AtLoginOptions)action;
            }
            c.SetAtLogin();

        }
        public void OnGet()
        {

            ViewData["Title"] = "Login";
            AuthToken = Request.Cookies["AuthToken"];
            Username = Request.Cookies["Username"];
            if (!string.IsNullOrEmpty(Username))
            {
                SetupAccount(Username);
            }
        }
        public void SetupAccount(string Username)
        {
            Account a = new Account(Username);
            UserAccount = a;
            OnlineCharacters = a.Characters;
            foreach (var access in a.Access)
            {
                if (access.RealmID == -1 && access.RealmID >= 1)
                {
                    this.IsGM = true;
                    this.Tickets = GMTicket.GetAllTickets();
                }
            }
            ViewData["Title"] = a.Username;
            CharacterListType = $"{a.Username}'s Characters";
        }



        public void OnPostLogin()
        {
            Console.WriteLine("Logging in!");
            UserEmail = Request.Form["UserEmail"];
            UserPassword = Request.Form["UserPassword"];
            Username = UserEmail.Substring(0, UserEmail.IndexOf("@"));
            Account a = new Account(Username);
            if (a.AuthenticateAccount(UserPassword))
            {
                Response.Cookies.Append("Username", Username);
                Response.Cookies.Append("AuthToken", a.Verifier);
                Response.Redirect("/Account");
            }
            

        }

        static string Hash(string input)
        {
            var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }

    }
}
