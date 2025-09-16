// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Dispatching;
using WinGetStudio.CLI;

namespace WinGetStudio;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // If arguments are provided, run in CLI mode
        if (args.Length > 0)
        {
            var cli = new WinGetStudioCLI();
            var result = cli.Invoke(args);
            Environment.Exit(result);
        }

        // Only initialize WinRT if we're starting the GUI
        WinRT.ComWrappersSupport.InitializeComWrappers();

        var isRedirect = DecideRedirection().GetAwaiter().GetResult();

        if (!isRedirect)
        {
            Microsoft.UI.Xaml.Application.Start((p) =>
            {
                var dispatcherQueue = DispatcherQueue.GetForCurrentThread();
                var context = new DispatcherQueueSynchronizationContext(dispatcherQueue);
                SynchronizationContext.SetSynchronizationContext(context);
                var app = new App();
            });
        }
    }

    private static async Task<bool> DecideRedirection()
    {
        var mainInstance = Microsoft.Windows.AppLifecycle.AppInstance.FindOrRegisterForKey("main");
        var activatedEventArgs = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();

        if (!mainInstance.IsCurrent)
        {
            // Redirect the activation (and args) to the "main" instance, and exit.
            await mainInstance.RedirectActivationToAsync(activatedEventArgs);
            return true;
        }

        return false;
    }
}
