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
    }
}
