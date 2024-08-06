namespace DotBoil.Dependency
{
    public abstract class Module
    {
        public abstract Task AddModule();
        public abstract Task UseModule();
    }
}
