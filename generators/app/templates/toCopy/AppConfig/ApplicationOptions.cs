namespace AppConfig
{
    public class ApplicationOptions
    {
        public string Environment { get; set; } // Mostly for test/debug purposes :)
        public string DbConnection { get; set; }
        public EmailerOptions EmailerOptions { get; set; }
        public JwtOptions JwtOptions { get; set; }
    }
}
