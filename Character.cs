public class Character
{
    //select username,name,level,race,class from characters.characters join auth.account on characters.characters.account = auth.account.id where characters.characters.online = 1;
    public string Username { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public int Race { get; set; }
    public int Class { get; set; }

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
    public string GetClass()
    {
        return classes[this.Class];
    }
    public string GetRace()
    {
        return races[this.Race];
    }
}