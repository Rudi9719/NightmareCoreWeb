using System;
using System.Net;
using System.Net.Mail;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
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
                AppendReaderToOnlineChars(rdr);
                CharacterListType = "Online Players";

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

        public void OnGetSearch(string query)
        {
            if (query.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                OnGetAccount("all");
                return;
            }
            ViewData["Title"] = "Search";
            string base_sql = "select username,name,level,race,class from characters.characters join auth.account on characters.characters.account = auth.account.id";
            string search_account_name_sql = $"{base_sql} where LOWER(auth.account.username) like @name";
            string search_characters_name_sql = $"{base_sql} where LOWER(characters.characters.name) like @name";
            query = $"%{query}%".ToLower();
            try
            {
                conn.Open();

                MySqlCommand cmd = new MySqlCommand(search_account_name_sql, conn);
                cmd.Parameters.AddWithValue("name", query);
                MySqlDataReader rdr = cmd.ExecuteReader();
                AppendReaderToOnlineChars(rdr);

                cmd = new MySqlCommand(search_characters_name_sql, conn);
                cmd.Parameters.AddWithValue("name", query);
                rdr = cmd.ExecuteReader();
                AppendReaderToOnlineChars(rdr);

                CharacterListType = "Search Results";


                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        private void AppendReaderToOnlineChars(MySqlDataReader rdr)
        {
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
        }
        public void OnGetAccount(string name)
        {
            if (name.Equals("all", StringComparison.OrdinalIgnoreCase))
            {

                ViewData["Title"] = "All Characters";
                string sql = "select username,name,level,race,class from characters.characters join auth.account on characters.characters.account = auth.account.id";
                QuerySQL(sql);
                this.CharacterListType = "All Characters";
                return;
            }
            Account a = new Account(name);
            ViewData["Title"] = name;
            CharacterListType = $"{name}'s Characters";
            OnlineCharacters = a.Characters;
        }

        public void OnPostActivateAccount()
        {

            conn.Open();
            bool valid = false;
            ActivateEmail = Request.Form["ActivateEmail"];
            string Username = ActivateEmail.Substring(0, ActivateEmail.IndexOf("@"));
            ActivatePassword = Request.Form["ActivatePassword"];
            ActivateToken = Request.Form["ActivateToken"];
            string sql = "SELECT token from tokens.active_tokens where email=@email";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("email", ActivateEmail);
            MySqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                if (ActivateToken.Equals(rdr.GetString(0)))
                {
                    valid = true;
                }
            }
            conn.Close();

            byte[] salt = new byte[32];
            byte[] verifier = new byte[32];
            (salt, verifier) = Framework.Cryptography.SRP6.MakeRegistrationData(Username, ActivatePassword);
            if (valid)
            {
                conn.Open();
                sql = "INSERT INTO auth.account (username,salt,verifier,email) VALUES (@username,@salt,@verifier,@email)";
                cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("email", ActivateEmail);
                cmd.Parameters.AddWithValue("username", Username);
                cmd.Parameters.AddWithValue("salt", salt);
                cmd.Parameters.AddWithValue("verifier", verifier);
                int status = cmd.ExecuteNonQuery();
                conn.Close();
                if (status == 1)
                {
                    Response.Redirect("/Account");
                }
                else
                {
                    conn.Open();
                    sql = "UPDATE auth.account SET salt=@salt, verifier=@verifier WHERE username=@username";
                    cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("email", ActivateEmail);
                    cmd.Parameters.AddWithValue("username", Username);
                    cmd.Parameters.AddWithValue("salt", salt);
                    cmd.Parameters.AddWithValue("verifier", verifier);
                    status = cmd.ExecuteNonQuery();
                    conn.Close();
                    if (status == 1)
                {
                    Response.Redirect("/Account");
                }
                }
            }

        }
        public void OnPostRequestToken()
        {
            string sql = "INSERT INTO tokens.active_tokens (email,token) VALUES (@email,@token)";
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[13];
            var random = new Random();
            RequestTokenEmail = Request.Form["RequestTokenEmail"];
            string Username = RequestTokenEmail.Substring(0, RequestTokenEmail.IndexOf("@"));
            string UserDomain = RequestTokenEmail.Substring(RequestTokenEmail.IndexOf("@"));
            bool valid = false;
            foreach (string s in Program.AllowedDomains)
            {
                if (UserDomain.Contains(s))
                {
                    valid = true;
                }
            }
            if (!valid)
            {
                Console.WriteLine($"Invalid Email {RequestTokenEmail}");
                return;
            }
            try
            {
                Account a = new Account(Username);
                Console.WriteLine($"Account already exists {Username}");
                if (a.Email.Equals(RequestTokenEmail))
                {
                    sql = "UPDATE tokens.active_tokens set token=@token where email=@email";
                    throw new Exception("Resetting password.");
                }
            }
            catch (Exception)
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("email", RequestTokenEmail);

                for (int i = 0; i < stringChars.Length; i++)
                {
                    stringChars[i] = chars[random.Next(chars.Length)];
                }

                var finalString = new String(stringChars);
                cmd.Parameters.AddWithValue("token", $"token_{finalString}");
                cmd.ExecuteNonQuery();
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    var basicCredential = new NetworkCredential($"{Program.EmailAddress}{Program.EmailDomain}", Program.EmailPass);
                    using (MailMessage message = new MailMessage())
                    {
                        MailAddress fromAddress = new MailAddress($"{Program.EmailAddress}{Program.EmailDomain}");

                        smtpClient.Host = Program.EmailHost;
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.Credentials = basicCredential;
                        smtpClient.Port = 587;
                        smtpClient.EnableSsl = true;
                        message.From = fromAddress;
                        message.Subject = "WoTDN Auth Token";
                        message.IsBodyHtml = false;
                        message.Body = $"WoTDN Auth Token for Account {Username}: token_{finalString}";
                        message.To.Add(RequestTokenEmail);

                        try
                        {
                            smtpClient.Send(message);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Unable to send message.");
                            //Error, could not send the message
                            Console.WriteLine(ex.Message);
                        }
                    }
                }

                conn.Close();
            }


        }

        public ActionResult OnGetAlert()
        {
            string ret = "SERVERALERT:\n\n<html><body>\n";
            if (this.OnlineCharacters.Count > 0)
            {
                ret += "<br/><h1 align=\"center\">Online Players</h1>\n";
                foreach (Character c in OnlineCharacters)
                {
                    Account a = new Account(c.Username);
                    if (a.IsGM)
                    {
                        ret += $"<p>[GM] <a href=\"https://wotdn.nightmare.haus/?handler=Account&amp;name={c.Username}\">{c.Username}</a>: {c.GetRace()} {c.GetClass()}, {c.Name}</p>";
                    }
                    else
                    {
                        ret += $"<p> <a href=\"https://wotdn.nightmare.haus/?handler=Account&amp;name={c.Username}\">{c.Username}</a>: Level {c.Level} {c.GetRace()} {c.GetClass()}, {c.Name}</p>";
                    }
                }
            }

            if (System.IO.File.Exists("announce.html"))
            {
                ret += "<br/>";
                ret += System.IO.File.ReadAllText("announce.html");
            }

            ret += "</body></html>\n\n\r";

            return Content(ret);

        }

    }
}
