using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace NightmareCoreWeb2 {

public class Account
{
    public UInt32 Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string LastIP { get; set; }
    public DateTime LastLogin { get; set; }
    public List<Character> Characters { get; set; }
    public List<AccountAccess> Access { get; set; }


    public Account AccountByID(int id, MySqlConnection conn)
    {
        conn.Open();
        string sql = "select username from account where id=@id";
        MySqlCommand cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);
        MySqlDataReader rdr = cmd.ExecuteReader();
        while (rdr.Read())
        {
            try
            {
                this.Username = rdr.GetString(0);
                return new Account(this.Username, conn);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        return null;
    }

    public Account(string username, MySqlConnection conn)
    {
        conn.Open();

        string sql = "select id,username,email,last_ip,last_login from account where username=@username";
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        rdr.Close();
        sql = "select username,name,level,race,class from characters.characters join auth.account on characters.characters.account = auth.account.id where characters.characters.account=@id";
        cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", this.Id);
        rdr = cmd.ExecuteReader();
        this.Characters = new List<Character>();
        while (rdr.Read())
        {
            try
            {
                Character c = new Character();
                c.Username = rdr.GetString(0);
                c.Name = rdr.GetString(1);
                c.Level = rdr.GetByte(2);
                c.Race = rdr.GetByte(3);
                c.Class = rdr.GetByte(4);
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

}
public class AccountAccess
{
    public int SecurityLevel { get; set; }
    public int RealmID { get; set; }
}
}