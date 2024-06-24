namespace DotBoil.Exceptions
{
    public class ValidationException : Exception
    {
        public IEnumerable<string> Messages { get; private set; }

        public ValidationException(IEnumerable<string> messages)
        {
            Messages = messages;
        }
    }
}
