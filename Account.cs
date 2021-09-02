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
            string salt = "", verifier = "";
            while (rdr.Read())
            {
                try
                {
                    salt = rdr.GetString(0);
                    verifier = rdr.GetString(1);
                }
                catch (Exception) { }
            }

            return VerifySRP6Login(this.Username, password, Encoding.ASCII.GetBytes(salt), Encoding.ASCII.GetBytes(verifier)) || AuthenticateWithToken(password);
        }
        // https://gist.github.com/Rochet2/3bb0adaf6f3e9a9fbc78ba5ce9a43e09
        public static byte[] CalculateSRP6Verifier(string username, string password, byte[] salt_bytes)
        {
            // algorithm constants
            BigInteger g = 7;
            BigInteger N = BigInteger.Parse("894B645E89E1535BBDAD5B8B290650530801B18EBFBF5E8FAB3C82872A3E9BB7", NumberStyles.HexNumber);

            SHA1 sha1 = SHA1.Create();

            // calculate first hash
            byte[] login_bytes = Encoding.ASCII.GetBytes((username + ':' + password).ToUpper());
            byte[] h1_bytes = sha1.ComputeHash(login_bytes);

            // calculate second hash
            byte[] h2_bytes = sha1.ComputeHash(salt_bytes.Concat(h1_bytes).ToArray());

            // convert to integer (little-endian)
            BigInteger h2 = new BigInteger(h2_bytes.Reverse().ToArray());
            Console.WriteLine(h2);

            // g^h2 mod N
            BigInteger verifier = BigInteger.ModPow(g, h2, N);

            // convert back to a byte array (little-endian)
            byte[] verifier_bytes = verifier.ToByteArray().Reverse().ToArray();

            // pad to 32 bytes, remember that zeros go on the end in little-endian!
            byte[] verifier_bytes_padded = new byte[Math.Max(32, verifier_bytes.Length)];
            Buffer.BlockCopy(verifier_bytes, 0, verifier_bytes_padded, 0, verifier_bytes.Length);

            // done!
            return verifier_bytes_padded;
        }
        public static bool VerifySRP6Login(string username, string password, byte[] salt, byte[] verifier)
        {
            // re-calculate the verifier using the provided username + password and the stored salt
            byte[] checkVerifier = CalculateSRP6Verifier(username, password, salt);
            Console.WriteLine($"{Encoding.ASCII.GetString(verifier)}\n{Encoding.ASCII.GetString(checkVerifier)}");
            // compare it against the stored verifier
            return verifier.SequenceEqual(checkVerifier);
        }

    }

    public class AccountAccess
    {
        public int SecurityLevel { get; set; }
        public int RealmID { get; set; }
    }

}