// MysqlConfig myDeserializedClass = JsonConvert.DeserializeObject<MysqlConfig>(myJsonResponse); 
   using System.Collections.Generic;
    public class MysqlConfig
    {
        public string MysqlUsername { get; set; }
        public string MysqlPassword { get; set; }
        public string MysqlPort { get; set; }
        public string MysqlServer { get; set; }
        public string MysqlDatabase { get; set; }
        
        public string EmailAddress { get; set; }
        public string EmailDomain { get; set; }
        public string EmailHost { get; set; }
        public string EmailPass { get; set; }
        public List<string> AllowedDomains { get; set; }
    }

