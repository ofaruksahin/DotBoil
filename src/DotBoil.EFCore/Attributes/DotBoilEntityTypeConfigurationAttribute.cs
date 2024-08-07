namespace DotBoil.EFCore.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DotBoilEntityTypeConfigurationAttribute : Attribute
    {
        public Type DbContextType { get; private set; }

        public DotBoilEntityTypeConfigurationAttribute(Type dbContextType)
        {
            DbContextType = dbContextType;
        }
    }
}
