namespace DotBoil.Configuration;

public class ApplicationConfiguration : IOptions
{
    public string Key => "DotBoil:Application:Info";

    public string MainApplicationName { get; set; }
    public string ApplicationName { get; set; }
    public string Description { get; set; }
    public List<string> PreloadPrefix { get; set; }

    public ApplicationConfiguration()
    {
        PreloadPrefix = new List<string>();
    }
}