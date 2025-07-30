// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;

namespace WinGetStudio.Services;

internal abstract class PageService
{
    private readonly Dictionary<string, Type> _pages = new();

    public PageService()
    {
        ConfigurePages();
    }

    /// <summary>
    /// Configures the pages for the application.
    /// </summary>
    protected abstract void ConfigurePages();

    /// <summary>
    /// Gets the page type associated with the specified typeparamref name="VM"/>.
    /// </summary>
    /// <typeparam name="VM">ViewModel type that is associated with the page.</typeparam>
    /// <returns>Type of the page associated with the specified ViewModel type.</returns>
    public Type GetPageType<VM>()
        where VM : ObservableObject
    {
        return GetPageType(typeof(VM));
    }

    /// <summary>
    /// Gets the page type associated with the specified ViewModel type.
    /// </summary>
    /// <param name="viewModelType">Type of the ViewModel that is associated with the page.</param>
    /// <returns>Type of the page associated with the specified ViewModel type.</returns>
    public Type GetPageType(Type viewModelType)
    {
        Type? pageType;
        var key = viewModelType.FullName!;
        lock (_pages)
        {
            if (!_pages.TryGetValue(key, out pageType))
            {
                throw new ArgumentException($"Page not found: {key}.");
            }
        }

        return pageType;
    }

    /// <summary>
    /// Configures a page with the specified ViewModel and Page types.
    /// </summary>
    /// <typeparam name="VM">ViewModel type that is associated with the page.</typeparam>
    /// <typeparam name="V">Page type that is associated with the ViewModel.</typeparam>
    protected void Configure<VM, V>()
        where VM : ObservableObject
        where V : Page
    {
        lock (_pages)
        {
            var key = typeof(VM).FullName!;
            if (_pages.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured");
            }

            var type = typeof(V);
            if (_pages.ContainsValue(type))
            {
                throw new ArgumentException($"This type is already configured with key {_pages.First(p => p.Value == type).Key}");
            }

            _pages.Add(key, type);
        }
    }
}
