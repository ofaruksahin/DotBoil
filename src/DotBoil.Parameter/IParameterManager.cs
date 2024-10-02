namespace DotBoil.Parameter
{
    public interface IParameterManager
    {
        Task<T> GetParameterValue<T>(string section, string name);
        Task<T> GetParameterValue<T>(string name);
        Task<T> GetParameterValue<T>(int tenantId, string name);
        Task<T> GetParameterValue<T>(int tenantId, string section, string name);
    }
}
