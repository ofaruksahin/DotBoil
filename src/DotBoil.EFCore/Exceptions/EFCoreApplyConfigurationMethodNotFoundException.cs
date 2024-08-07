namespace DotBoil.EFCore.Exceptions
{
    internal class EFCoreApplyConfigurationMethodNotFoundException : Exception
    {
        public EFCoreApplyConfigurationMethodNotFoundException()
            : base ($"{nameof(EFCoreDbContext)} ApplyConfiguration method is not found")
        {
            
        }
    }
}
