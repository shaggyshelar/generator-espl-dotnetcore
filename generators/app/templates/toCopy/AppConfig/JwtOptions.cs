namespace AppConfig
{
    public class JwtOptions
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string SecretPassword { get; set; }
    }
}
