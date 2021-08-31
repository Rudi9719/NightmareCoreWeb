using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;



namespace NightmareCoreWeb2
{

    public class GMTicket
    {
        public int Id { get; set; }
        public Account OpenedBy { get; set; }
        public string CharacterName { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime LastModifiedTime { get; set; }
        public Account ClosedBy { get; set; }
        public Account AssignedTo { get; set; }
        public string Description { get; set; }


        public static List<GMTicket> GetAllTickets()
        {
            List<GMTicket> ret = new List<GMTicket>();

            MySqlConnection conn = new MySqlConnection(Program.connStr);
            conn.Open();
            string sql = "select id from characters.gm_ticket";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                try
                {
                    GMTicket ticket = new GMTicket(rdr.GetInt32(0));
                    ret.Add(ticket);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return ret;

        }
        public GMTicket(int id)
        {
            this.Id = id;

            MySqlConnection conn = new MySqlConnection(Program.connStr);
            conn.Open();

            string sql = "select playerGuid,name,createTime,lastModifiedTime,closedBy,assignedTo,description from characters.gm_ticket where id=@id";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);
            MySqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                try
                {
                    this.OpenedBy = new Account(new Character(rdr.GetInt32(0)).Username);
                    this.CharacterName = rdr.GetString(1);
                    this.CreateTime = DateTimeOffset.FromUnixTimeSeconds(rdr.GetInt32(2)).UtcDateTime;
                    this.LastModifiedTime = DateTimeOffset.FromUnixTimeSeconds(rdr.GetInt32(3)).UtcDateTime;
                    if (rdr.GetInt32(4) != 0)
                    {
                        this.ClosedBy = Account.AccountByID(rdr.GetInt32(4));
                    }
                    if (rdr.GetInt32(5) != 0)
                    {
                        this.AssignedTo = Account.AccountByID(rdr.GetInt32(5));
                    }
                    this.Description = rdr.GetString(6);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            rdr.Close();

        }
    }




}