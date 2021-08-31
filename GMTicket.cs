using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;



namespace NightmareCoreWeb2
{

    public class GMTicket
    {
        public int Id { get; set; }
        public Account Account { get; set; }
        public string CharacterName { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime LastModifiedTime { get; set; }
        public Account ClosedBy { get; set; }
        public Account AssignedTo { get; set; }
        public string Description { get; set; }


        public static List<GMTicket> GetAllTickets(MySqlConnection conn)
        {
            List<GMTicket> ret = new List<GMTicket>();

            conn.Open();
            string sql = "select id from gm_ticket";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                try
                {
                    GMTicket ticket = new GMTicket(rdr.GetInt32(0), conn);
                    ret.Add(ticket);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return ret;

        }
        public GMTicket(int id, MySqlConnection conn)
        {
            this.Id = id;
            conn.Open();

            string sql = "select id,playerGuid,name,description,createTime,lastModifiedTime,closedBy,assignedTo from gm_ticket where id=@id";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);
            MySqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                try
                {
                    this.Account = Account.AccountByID(rdr.GetInt32(0), conn);
                    this.CharacterName = rdr.GetString(1);
                    this.CreateTime = rdr.GetDateTime(2);
                    this.LastModifiedTime = rdr.GetDateTime(3);
                    if (rdr.GetInt32(4) != 0)
                    {
                        this.ClosedBy = Account.AccountByID(rdr.GetInt32(4), conn);
                    }
                    if (rdr.GetInt32(5) != 0)
                    {
                        this.AssignedTo = Account.AccountByID(rdr.GetInt32(5), conn);
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