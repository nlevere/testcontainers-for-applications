public static class Extensions
{
    public static void Invoke(this object instance, string methodName)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

        var method = instance.GetType().GetMethod(methodName);
        if (method == null)
        {
            Console.WriteLine($"Failed to find method {methodName}.");
            return;
        }

        var task = (Task?)method.Invoke(instance, [cts.Token]);
        if (task == null)
        {
            Console.WriteLine($"Method {methodName} does not return a Task.");
            return;
        }
        // NOTE: This must be called synchronously to ensure the application is blocked from running.
        task.Wait(cts.Token);
    }
}
