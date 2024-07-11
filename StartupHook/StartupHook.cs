using System.Reflection;
using TestEnvironment;

/// <summary>
/// https://github.com/dotnet/runtime/blob/main/docs/design/features/host-startup-hook.md
/// </summary>
internal class StartupHook
{
    public static void Initialize()
    {
        Console.WriteLine("Initializing test environment!");

        var loadContext = new HookAssemblyLoadContext();
        var assembly = loadContext.LoadFromAssemblyName(new AssemblyName("TestEnvironment"));
        if (assembly.CreateInstance("TestEnvironment.TestEnvironment") is not IAsyncDisposable testEnvironment)
        {
            Console.WriteLine("Failed to create test environment.");
            return;
        }

        // NOTE: This must be called synchronously to ensure the environment is ready before application runs.
        testEnvironment.Invoke("StartAsync");

        AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
        {
            Console.WriteLine("Stopping test environment!");

            // NOTE: This must be called synchronously to ensure the dispose executes.
            testEnvironment.Invoke("StopAsync");
            testEnvironment.DisposeAsync().AsTask().Wait();
        };
        
        Environment.SetEnvironmentVariable("HostOptions__ShutdownTimeout", "00:00:15");
    }
}
