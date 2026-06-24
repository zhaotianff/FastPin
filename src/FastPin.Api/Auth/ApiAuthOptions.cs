namespace FastPin.Api.Auth;

public class ApiAuthOptions
{
    public const string SectionName = "ApiAuth";

    public string Username { get; set; } = "admin";
    public string Password { get; set; } = "change-me";
    public int TokenLifetimeMinutes { get; set; } = 480;
}
