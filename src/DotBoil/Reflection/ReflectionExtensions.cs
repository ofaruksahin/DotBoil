using System.Security.AccessControl;

namespace DotBoil.Reflection
{
    public static class ReflectionExtensions
    {
        public static Type FindType(this AppDomain appDomain, string typeName)
        {
            foreach (var assembly in appDomain.GetAssemblies())
            {
                var type = assembly.GetType(typeName, false);

                if (type is not null)
                    return type;
            }

            return null;
        }

        public static Type FindTypeWithBaseType(this AppDomain appDomain, Type baseType)
        {
            foreach (var assembly in appDomain.GetAssemblies())
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
            foreach (var assembly in appDomain.GetAssemblies())
            {
                var types = assembly
                    .GetTypes()
                    .Where(type => type.BaseType != null && type.BaseType == baseType)
                    .ToList();

                if (types is not null && types.Any())
                    return types;
            }

            return Enumerable.Empty<Type>();
        }

        public static IEnumerable<Type> FindTypesWithBaseType(this AppDomain appDomain, Func<Type, bool> predicate)
        {
            foreach (var assembly in appDomain.GetAssemblies())
            {
                var types = assembly
                .GetTypes()
                    .Where(predicate)
                    .ToList();

                if (types is not null && types.Any())
                    return types;
            }

            return Enumerable.Empty<Type>();
        }

        public static Type FindTypeWithInterface(this AppDomain appDomain, Type interfaceType)
        {
            foreach (var assembly in appDomain.GetAssemblies())
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
            foreach (var assembly in appDomain.GetAssemblies())
            {
                var types = assembly
                    .GetTypes()
                    .Where(type => type.GetInterface(interfaceType.Name) is not null)
                    .ToList();

                if (types is not null && types.Any())
                    return types;
            }

            return null;
        }
    }
}
