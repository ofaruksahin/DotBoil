namespace DotBoil.EFCore
{
    public interface IAuditUser
    {
        Task<string> GetModifierName();
    }
}
