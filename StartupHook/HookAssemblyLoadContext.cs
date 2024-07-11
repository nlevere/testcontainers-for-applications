using System.Reflection;
using System.Runtime.Loader;

namespace TestEnvironment;

public class HookAssemblyLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public HookAssemblyLoadContext()
    {
        _resolver = new AssemblyDependencyResolver(Assembly.GetExecutingAssembly().Location);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }
}
