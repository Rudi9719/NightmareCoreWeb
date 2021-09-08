using System;
using System.Numerics;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Globalization;

namespace NightmareCoreWeb2
{

    public class Account
    {
        public UInt32 Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string LastIP { get; set; }
        public byte[] Verifier { get; set; }
        public DateTime LastLogin { get; set; }
        public List<Character> Characters { get; set; }
        public List<AccountAccess> Access { get; set; }


        public static Account AccountByID(int id)
        {

            MySqlConnection conn = new MySqlConnection(Program.connStr);
            conn.Open();
            string sql = "select username from account where id=@id";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);
            MySqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                try
                {
                    return new Account(rdr.GetString(0));

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return null;
        }

        public Account(string username)
        {
            MySqlConnection conn = new MySqlConnection(Program.connStr);
            conn.Open();

            string sql = "select id,username,email,last_ip,last_login,verifier from account where username=@username";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("username", username);
            MySqlDataReader rdr = cmd.ExecuteReader();
            this.Verifier = new byte[32];
            while (rdr.Read())
            {
                try
                {
                    this.Id = rdr.GetUInt32(0);
                    this.Username = rdr.GetString(1);
                    this.Email = rdr.GetString(2);
                    this.LastIP = rdr.GetString(3);
                    this.LastLogin = rdr.GetDateTime(4);
                    rdr.GetBytes(5, 0, this.Verifier, 0, 32);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            rdr.Close();
            sql = "select guid,username,name,level,race,class from characters.characters join auth.account on characters.characters.account = auth.account.id where characters.characters.account=@id";
            cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", this.Id);
            rdr = cmd.ExecuteReader();
            this.Characters = new List<Character>();

            while (rdr.Read())
            {
                try
                {
                    Character c = new Character();
                    c.guid = (int)rdr.GetUInt32(0);
                    c.Username = rdr.GetString(1);
                    c.Name = rdr.GetString(2);
                    c.Level = rdr.GetByte(3);
                    c.Race = rdr.GetByte(4);
                    c.Class = rdr.GetByte(5);
                    this.Characters.Add(c);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            rdr.Close();
            sql = "select SecurityLevel,RealmID from account_access where AccountID=@id";
            cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", this.Id);
            rdr = cmd.ExecuteReader();
            this.Access = new List<AccountAccess>();
            while (rdr.Read())
            {
                try
                {
                    AccountAccess acctA = new AccountAccess();
                    acctA.SecurityLevel = rdr.GetByte(0);
                    acctA.RealmID = rdr.GetInt32(1);
                    this.Access.Add(acctA);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            rdr.Close();

            conn.Close();
        }
        public bool AuthenticateWithToken(string token)
        {
            MySqlConnection conn = new MySqlConnection(Program.connStr);
            conn.Open();
            string sql = "select token from tokens.active_tokens where email=@email";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("email", this.Email);
            MySqlDataReader rdr = cmd.ExecuteReader();
            string dbToken = "";
            while (rdr.Read())
            {
                try
                {
                    dbToken = rdr.GetString(0);
                }
                catch (Exception) { }
            }
            return token.Equals(dbToken);
        }
        public bool AuthenticateAccount(string password)
        {
            MySqlConnection conn = new MySqlConnection(Program.connStr);
            conn.Open();
            string sql = "select salt,verifier from account where username=@username";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("username", this.Username);
            MySqlDataReader rdr = cmd.ExecuteReader();
            byte[] salt = new byte[32];
            byte[] verifier = new byte[32];
            while (rdr.Read())
            {
                try
                {
                    rdr.GetBytes(0, 0, salt, 0, 32);
                    rdr.GetBytes(1, 0, verifier, 0, 32);
                }
                catch (Exception) { }
            }
            byte[] calculatedVerifier = Framework.Cryptography.SRP6.CalculateVerifier(this.Username, password, salt);
            return calculatedVerifier.Compare(verifier);
        }
        public bool AuthenticateAccount(byte[] verifier)
        {
            return verifier.Compare(this.Verifier);
        }

    }

    public class AccountAccess
    {
        public int SecurityLevel { get; set; }
        public int RealmID { get; set; }
    }

}