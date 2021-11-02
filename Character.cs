using System;
using MySql.Data.MySqlClient;

namespace NightmareCoreWeb2
{

    public class Character
    {
        //select username,name,level,race,class from characters.characters join auth.account on characters.characters.account = auth.account.id where characters.characters.online = 1;
        public int guid { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int Race { get; set; }
        public int Class { get; set; }
        public AtLoginOptions AtLogin { get; set; }

        public string[] classes = {
            "Null",
    "Warrior",
    "Paladin",
    "Hunter",
    "Rogue",
    "Priest",
    "Death Knight",
    "Shaman",
    "Mage",
    "Warlock",
    "Monk",
    "Druid",
    "Demon Hunter"
    };
        public string[] races = {
           "Null",
    "Human",
    "Orc",
    "Dwarf",
    "Night Elf",
    "Undead",
    "Tauren",
    "Gnome",
    "Troll",
    "Goblin",
    "Blood Elf",
    "Draenei"
    };
        [Flags]
        public enum AtLoginOptions
        {
            AT_LOGIN_RENAME = 1,
            AT_LOGIN_RESET_SPELLS = 2,
            AT_LOGIN_RESET_TALENTS = 4,
            AT_LOGIN_CUSTOMIZE = 8,
            AT_LOGIN_RESET_PET_TALENTS = 16,
            AT_LOGIN_FIRST = 32,
            AT_LOGIN_CHANGE_FACTION = 64,
            AT_LOGIN_CHANGE_RACE = 128
        }
        public string GetClass()
        {
            return classes[this.Class];
        }
        public string GetRace()
        {
            return races[this.Race];
        }

        public Character() { }
        public Character(int guid)
        {

            MySqlConnection conn = new MySqlConnection(Program.connStr);
            conn.Open();

            string sql = "select username,name,level,race,class,at_login from characters.characters join auth.account on characters.characters.account = auth.account.id where characters.characters.guid=@id";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", guid);
            MySqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                try
                {
                    this.guid = guid;
                    this.Username = rdr.GetString(0);
                    this.Name = rdr.GetString(1);
                    this.Level = rdr.GetByte(2);
                    this.Race = rdr.GetByte(3);
                    this.Class = rdr.GetByte(4);
                    this.AtLogin = (AtLoginOptions)rdr.GetUInt16(5);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            rdr.Close();
            conn.Close();
        }

        public bool TransferToAccount(int newAccount)
        {
            MySqlConnection conn = new MySqlConnection(Program.connStr);
            conn.Open();
            string sql = "update characters.characters set account=@newAccount where guid=@guid";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("guid", this.guid);
            cmd.Parameters.AddWithValue("newAccount", newAccount);
            int status = cmd.ExecuteNonQuery();
            conn.Close();
            return status == 1;
        }

        public bool SetAtLogin()
        {
            MySqlConnection conn = new MySqlConnection(Program.connStr);
            conn.Open();
            string sql = "update characters.characters set at_login=@loginOpts where guid=@guid";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("guid", this.guid);
            cmd.Parameters.AddWithValue("loginOpts", (int)this.AtLogin);
            int status = cmd.ExecuteNonQuery();
            conn.Close();
            return status == 1;
        }
    }
}