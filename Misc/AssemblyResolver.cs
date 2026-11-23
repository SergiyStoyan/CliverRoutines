using System;
using System.IO;
using System.Linq;
using System.Reflection;

public static class AssemblyResolver
{
    public static void Initialize()
    {
        if (isRegistered)
            return;
        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        isRegistered = true;
    }
    static bool isRegistered = false;

    private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
    {
        var requestedName = new AssemblyName(args.Name);

        // 2. Look for an already loaded assembly with the exact same name, 
        // ignoring the fact that it has a newer version.
        var matchingAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => string.Equals(a.GetName().Name, requestedName.Name, StringComparison.OrdinalIgnoreCase));
        if (matchingAssembly != null)
            return matchingAssembly;

        // 3. Fallback: If it's not loaded in memory yet, look for the physical file 
        // in the application executing directory.
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string assemblyPath = Path.Combine(baseDirectory, requestedName.Name + ".dll");

        try
        {
            return Assembly.LoadFrom(assemblyPath);
        }
        catch (Exception e)
        {
            throw;
        }
    }
}
