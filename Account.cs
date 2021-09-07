using System;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
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
        public string Verifier { get; set; }
        public DateTime LastLogin { get; set; }
        public List<Character> Characters { get; set; }
        public List<AccountAccess> Access { get; set; }
        private readonly BigInteger g = 7;
        private readonly BigInteger N = BigInteger.Parse("894B645E89E1535BBDAD5B8B290650530801B18EBFBF5E8FAB3C82872A3E9BB7", NumberStyles.HexNumber);


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

            while (rdr.Read())
            {
                try
                {
                    this.Id = rdr.GetUInt32(0);
                    this.Username = rdr.GetString(1);
                    this.Email = rdr.GetString(2);
                    this.LastIP = rdr.GetString(3);
                    this.LastLogin = rdr.GetDateTime(4);
                    this.Verifier = rdr.GetString(5);
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

            return AuthenticateWithToken(password) || VerifySRP6Login(this.Username, password, salt, verifier);
        }
        public bool VerifySRP6Login(string username, string password, byte[] salt, byte[] verifier)
        {
            // re-calculate the verifier using the provided username + password and the stored salt
            byte[] checkVerifier = CalculateVerifier(username, password, salt);
            Console.WriteLine($"{Encoding.ASCII.GetString(verifier)} {verifier.Length} bytes\n{Encoding.ASCII.GetString(checkVerifier)} {checkVerifier.Length} bytes");
            Console.WriteLine($"DB {new BigInteger(verifier)}\nTC {new BigInteger(CalculateVerifier(username, password, salt))}");
            // compare it against the stored verifier
            return verifier.SequenceEqual(checkVerifier.Reverse().ToArray());
        }
        public byte[] Hash(byte[] componentOne, byte[] componentTwo)
        {
            if (componentOne == null) throw new ArgumentNullException(nameof(componentOne));
            if (componentTwo == null) throw new ArgumentNullException(nameof(componentTwo));
            return Hash(componentOne.Concat(componentTwo).ToArray());
        }
        public byte[] Hash(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            //WoW expects non-secure SHA1 hashing. SRP6 is deprecated too. We need to do it anyway
            using (SHA1 shaProvider = SHA1.Create())
            {
                return shaProvider.ComputeHash(bytes);
            }
        }
        public byte[] CalculateVerifier(string username, string password, byte[] salt)
        {
            using (SHA1 shaProvider = SHA1.Create())
            {
                if (BitConverter.IsLittleEndian)
                {
                    return BigInteger.ModPow(
                                   g,
                                   new BigInteger(Hash(salt, Hash(Encoding.UTF8.GetBytes($"{username.ToUpper()}:{password.ToUpper()}")))),
                                   N
                               ).ToByteArray();
                }
                else
                {
                    return BigInteger.ModPow(
                                   g,
                                   new BigInteger(Hash(salt, Hash(Encoding.UTF8.GetBytes($"{username.ToUpper()}:{password.ToUpper()}")).Reverse().ToArray())),
                                   N
                               ).ToByteArray();
                }
            }

        }

    }

    public class AccountAccess
    {
        public int SecurityLevel { get; set; }
        public int RealmID { get; set; }
    }

}