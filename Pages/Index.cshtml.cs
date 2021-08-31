using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace NightmareCoreWeb2.Pages
{
    public class IndexModel : PageModel
    {
        public List<Character> OnlineCharacters = new List<Character>();
        public Dictionary<string, string> Realms = new Dictionary<string, string>();

        public string ActivateEmail { get; set; }
        public string ActivatePassword { get; set; }
        public string ActivateToken { get; set; }
        public string RequestTokenEmail { get; set; }
        public string CharacterListType { get; set; }
        private MySqlConnection conn;

        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;

            conn = new MySqlConnection(Program.connStr);

            string sql = "select username,name,level,race,class from characters.characters join auth.account on characters.characters.account = auth.account.id where characters.characters.online = 1";
            QuerySQL(sql);

        }
        public void QuerySQL(string sql)
        {
            try
            {
                conn.Open();

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                CharacterListType = "Online Players";
                while (rdr.Read())
                {
                    Character c = new Character();
                    c.Username = rdr.GetString(0);
                    c.Name = rdr.GetString(1);
                    c.Level = rdr.GetByte(2);
                    c.Race = rdr.GetByte(3);
                    c.Class = rdr.GetByte(4);
                    OnlineCharacters.Add(c);
                }
                rdr.Close();
                sql = "SELECT name,flag FROM realmlist";
                cmd = new MySqlCommand(sql, conn);
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    Realms.Add(rdr.GetString(0), rdr.GetString(1).Equals("2") ? "❌" : "✔️");
                }
                rdr.Close();
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void OnGet()
        {
            ViewData["Title"] = "WotDN";
        }
        public void OnGetAccount(string name)
        {
            if (name.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                
                ViewData["Title"] = "All Characters";
                string sql = "select username,name,level,race,class from characters.characters join auth.account on characters.characters.account = auth.account.id";
                QuerySQL(sql);
                return;
            }
            Account a = new Account(name);
            ViewData["Title"] = name;
            CharacterListType = $"{name}'s Characters";
            OnlineCharacters = a.Characters;
        }

        public void OnPostActivateAccount()
        {
            ActivateEmail = Request.Form["ActivateEmail"];
            ActivatePassword = Request.Form["ActivatePassword"];
            ActivateToken = Request.Form["ActivateToken"];
        }
        public void OnPostRequestToken()
        {
            RequestTokenEmail = Request.Form["RequestTokenEmail"];
        }

        public bool RequestToken()
        {
            return false;
        }
        public bool CreateAccount()
        {
            return false;
        }
        public bool IsTokenValid(string username, string token)
        {
            return false;
        }
    }
}
