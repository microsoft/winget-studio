// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using WinGetStudio.ViewModels.Controls;

namespace WinGetStudio.Converters;

/// <summary>
/// Converter to highlight the matching part of a resource suggestion.
/// </summary>
public sealed partial class ResourceSuggestionMatchConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is ResourceSuggestionViewModel suggestion)
        {
            var textBlock = new TextBlock() { TextTrimming = TextTrimming.CharacterEllipsis };
            var fullText = suggestion.DisplayName ?? string.Empty;
            var query = suggestion.SearchText ?? string.Empty;
            var index = fullText.IndexOf(query, StringComparison.OrdinalIgnoreCase);

            // If there's a match, split the text and apply bold formatting to
            // the matching part.
            if (index >= 0)
            {
                var before = fullText[..index];
                var match = fullText.Substring(index, query.Length);
                var after = fullText[(index + query.Length)..];

                textBlock.Inlines.Add(new Run { Text = before });
                textBlock.Inlines.Add(new Run { Text = match, FontWeight = FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = after });
            }
            else
            {
                textBlock.Inlines.Add(new Run { Text = fullText });
            }

            return textBlock;
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
