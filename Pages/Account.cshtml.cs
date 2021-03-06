using System;
using System.Linq;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
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
        public string NewPassword {get; set;}
        public string NewPassword2 {get; set;}
        public bool IsAuthenticated = false;
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

            OnGet();
            Character c = new Character(guid);
            if (!IsAuthenticated || c.Username != this.UserAccount.Username)
            {
                return;
            }
            if ((c.AtLogin & Character.AtLoginOptions.AT_LOGIN_FIRST) == 0)
            {
                c.AtLogin |= (Character.AtLoginOptions)action;
            }
            c.SetAtLogin();
            Response.Redirect("/Account");

        }
        public void OnGet()
        {

            ViewData["Title"] = "Login";
            if (Request.Cookies.Count() > 1)
            {
                try
                {
                    this.UserAccount = new Account(Request.Cookies["Username"]);
                    byte[] auth = Convert.FromBase64String(Request.Cookies["AuthToken"]);
                    this.Username = this.UserAccount.Username;
                    if (!this.UserAccount.AuthenticateAccount(auth))
                    {
                        Console.WriteLine($"Failed to authenticate {this.UserAccount.Username}");
                        Response.Cookies.Delete("Username");
                        Response.Cookies.Delete("AuthToken");
                    }
                    else
                    {
                        this.IsAuthenticated = true;
                    }
                    SetupAccount(this.UserAccount.Username);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }

        }
        public void SetupAccount(string Username)
        {
            Account a = new Account(Username);
            UserAccount = a;
            OnlineCharacters = a.Characters;
            if (a.IsGM) {
                this.Tickets = GMTicket.GetAllTickets();
            }
            ViewData["Title"] = a.Username;
            CharacterListType = $"{a.Username}'s Characters";
            this.UserAccount = a;
        }

        public void OnPostLogin()
        {
            UserEmail = Request.Form["UserEmail"];
            UserPassword = Request.Form["UserPassword"];
            try
            {
                Username = UserEmail.Substring(0, UserEmail.IndexOf("@"));
            }
            catch (Exception)
            {
                Username = UserEmail;
            }
            Account a = new Account(Username);
            if (a.AuthenticateAccount(UserPassword))
            {
                Response.Cookies.Append("Username", Username);
                Response.Cookies.Append("AuthToken", Convert.ToBase64String(a.Verifier));
                Response.Redirect("/Account");
            }

        }
        public void OnPostChangePassword() {
            OnGet();
            NewPassword = Request.Form["NewPassword"];
            NewPassword2 = Request.Form["NewPassword2"];
            if (NewPassword.Equals(NewPassword2)) {
                this.UserAccount.ChangePassword(NewPassword);
            }
        }

    }
}
