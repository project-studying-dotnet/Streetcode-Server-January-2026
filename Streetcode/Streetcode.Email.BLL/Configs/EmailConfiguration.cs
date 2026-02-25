namespace Streetcode.Email.BLL.Configs
{
    public class EmailConfiguration
    {
        public const string SectionName = "EmailConfiguration";

        public string FromAddress { get; set; }
        public string AdminAddress { get; set; }
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPassword { get; set; }
    }
}
