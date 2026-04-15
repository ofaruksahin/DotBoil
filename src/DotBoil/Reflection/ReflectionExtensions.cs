using System.Diagnostics;
using System.Reflection;

namespace DotBoil.Reflection
{
    public static class ReflectionExtensions
    {
        private static IEnumerable<Assembly> GetFilteredAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.GetName().Name!.StartsWith("Microsoft.Build"))
                .Where(a => !a.GetName().Name!.StartsWith("Microsoft.CodeAnalysis"))
                .ToList();
        }

        public static Type FindType(this AppDomain appDomain, string typeName)
        {
            foreach (var assembly in GetFilteredAssemblies())
            {
                var type = assembly.GetType(typeName, false);

                if (type is not null)
                    return type;
            }

            return null;
        }

        public static Type FindTypeWithBaseType(this AppDomain appDomain, Type baseType)
        {
            foreach (var assembly in GetFilteredAssemblies())
            {
                var type = assembly
                    .GetTypes()
                    .FirstOrDefault(type => type.BaseType != null && type.BaseType == baseType);

                if (type is not null)
                    return type;
            }

            return null;
        }

        public static IEnumerable<Type> FindTypesWithBaseType(this AppDomain appDomain, Type baseType)
        {
            var response = new List<Type>();
            foreach (var assembly in GetFilteredAssemblies())
            {
                var types = assembly
                    .GetTypes()
                    .Where(type => type.BaseType != null && type.BaseType == baseType)
                    .ToList();

                if (types is not null && types.Any())
                    response.AddRange(types);
            }

            return response;
        }

        public static IEnumerable<Type> FindTypesWithBaseType(this AppDomain appDomain, Func<Type, bool> predicate)
        {
            var response = new List<Type>();
            foreach (var assembly in GetFilteredAssemblies())
            {
                var types = assembly
                    .GetTypes()
                    .Where(predicate)
                    .ToList();

                if (types is not null && types.Any())
                    response.AddRange(types);
            }

            return response;
        }

        public static Type FindTypeWithInterface(this AppDomain appDomain, Type interfaceType)
        {
            foreach (var assembly in GetFilteredAssemblies())
            {
                var type = assembly
                    .GetTypes()
                    .FirstOrDefault(type => type.GetInterface(interfaceType.Name) is not null);

                if (type is not null)
                    return type;
            }

            return null;
        }

        public static IEnumerable<Type> FindTypesWithInterface(this AppDomain appDomain, Type interfaceType)
        {
            var responseTypes = new List<Type>();
            foreach (var assembly in GetFilteredAssemblies())
            {
                try
                {
                    var types = assembly
                        .GetTypes()
                        .Where(type => !type.IsAbstract && type.GetInterface(interfaceType.Name) is not null)
                        .ToList();

                    if (types is not null && types.Any())
                        responseTypes.AddRange(types);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }

            return responseTypes;
        }
    }
}
