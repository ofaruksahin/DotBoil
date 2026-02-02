namespace DotBoil.Dependency
{
    public abstract class Module
    {
        public abstract string Name { get; }
        public abstract IEnumerable<string> DependsOn { get; }
        public abstract int Order { get; }
        public abstract Task AddModule();
        public abstract Task UseModule();
    }
}
