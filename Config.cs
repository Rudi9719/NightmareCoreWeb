// MysqlConfig myDeserializedClass = JsonConvert.DeserializeObject<MysqlConfig>(myJsonResponse); 
    public class MysqlConfig
    {
        public string MysqlUsername { get; set; }
        public string MysqlPassword { get; set; }
        public string MysqlPort { get; set; }
        public string MysqlServer { get; set; }
        public string MysqlDatabase { get; set; }
    }

