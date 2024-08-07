using DotBoil.EFCore.Interceptors;

namespace DotBoil.EFCore.Configurations
{
    internal class EFCoreContextConfiguration
    {
        public string TypeName { get; set; }
        public List<string> Interceptors { get; set; }

        public EFCoreContextConfiguration()
        {
            Interceptors = new List<string>();
            Interceptors.Add(typeof(AuditInterceptor).FullName);
        }
    }
}
