namespace DotBoil.EFCore.Exceptions
{
    internal class EFCoreDbContextLoaderException : Exception
    {
        public EFCoreDbContextLoaderException() : base($"{nameof(EFCoreDbContextLoader)} is not implemented")
        {

        }
    }
}
