using DotBoil.Parameter.Persistence;
using Microsoft.Extensions.Configuration;

namespace DotBoil.Parameter
{
    internal class ParameterConfigurationProvider : ConfigurationProvider
    {
        public override void Load()
        {
            using (var context = new ParameterDbContext())
            {
                var parameters = context.Parameters.Where(p => p.TenantId == 0).ToList();

                foreach (var parameter in parameters)
                {
                    var key = string.Empty;
                    if (!string.IsNullOrEmpty(parameter.Section))
                        key = string.Join(':', parameter.Section, parameter.Key);
                    else
                        key = string.Join(':', parameter.Key);

                    Data.Add(key, parameter.Value);
                }
            }
        }
    }
}
