namespace DotBoil.Localization
{
    public interface ILocalize
    {
        Task<string> LocalizeText(string name);
        Task<string> LocalizeText(string group, string name);
    }
}
