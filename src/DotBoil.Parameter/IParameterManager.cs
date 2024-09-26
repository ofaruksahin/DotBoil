namespace DotBoil.Parameter
{
    public interface IParameterManager
    {
        Task<T> GetParameterValue<T>(string section, string name);
        Task<T> GetParameterValue<T>(string name);
    }
}
