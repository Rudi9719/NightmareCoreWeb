using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

public class Account
{
    public UInt32 Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string LastIP { get; set; }
    public DateTime LastLogin { get; set; }
    public List<Character> characters { get; set; }

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
        this.characters = new List<Character>();
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
                this.characters.Add(c);
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