namespace DotBoil.Localization.Exceptions
{
    public class LocalizeException : Exception
    {
        public LocalizeException(string language, string group, string key) 
            : base($"{language} {group} {key} localization not found")
        {
            
        }
    }
}
