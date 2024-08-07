using DotBoil.Configuration;

namespace DotBoil.EFCore.Configurations
{
    internal class EFCoreConfiguration : IOptions
    {
        public string Key => "DotBoil:EFCore";

        public List<EFCoreContextConfiguration> Contexts { get; set; }

        public EFCoreConfiguration()
        {
            Contexts = new List<EFCoreContextConfiguration>();
        }
    }
}
