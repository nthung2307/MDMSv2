namespace Nobisoft.Core.Configuration;

public class AppDbConfiguration
{
    public string Server { get; set; } = "localhost";
    public string UserName { get; set; } = "root";
    public string Password { get; set; } = "root";
    public string Database { get; set; } = "db";
    public string Scheme { get; set; } = "scheme";
    public int Port { get; set; } = 5555;
}