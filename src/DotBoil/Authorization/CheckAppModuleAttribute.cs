namespace DotBoil;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class CheckAppModuleAttribute : Attribute
{
    public string AppModule { get; set; }

    public CheckAppModuleAttribute(string appModule)
    {
        AppModule = appModule;
    }
}