namespace DotBoil;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class CheckRoleAttribute : Attribute
{
    public string Role { get; set; }

    public CheckRoleAttribute(string role)
    {
        Role = role;
    }
}