namespace AppConfig
{
    public static class OptionsStore
    {
        public static void SetApplicationOptions(ApplicationOptions appOptions)
        {
            ApplicationOptions = appOptions;
        }

        public static void SetTokenProviderOptions(TokenProviderOptions tokenOptions)
        {
            TokenProviderOptions = tokenOptions;
        }

        public static ApplicationOptions ApplicationOptions { get; private set; }
        public static TokenProviderOptions TokenProviderOptions { get; private set; }
    }
}
