namespace DotBoil.Parameter
{
    public interface IParameterManager
    {
        Task<T> GetParameterValue<T>(string section, string name, bool isPublic = false);
        Task<T> GetParameterValue<T>(string name, bool isPublic = false);
        Task<T> GetParameterValue<T>(int tenantId, string name, bool isPublic = false);
        Task<T> GetParameterValue<T>(int tenantId, string section, string name, bool isPublic = false);
    }
}
