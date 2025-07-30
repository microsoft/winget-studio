using WinGetStudio.Contracts.Services;
using Windows.ApplicationModel.Activation;



namespace WinGetStudio.Activation;

public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly IAppNavigationService _navigationService;

    public DefaultActivationHandler(IAppNavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        // None of the ActivationHandlers has handled the activation.
        return _navigationService.Frame?.Content == null;
    }

    protected async override Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        _navigationService.NavigateToDefaultPage();
        await Task.CompletedTask;
    }
}
