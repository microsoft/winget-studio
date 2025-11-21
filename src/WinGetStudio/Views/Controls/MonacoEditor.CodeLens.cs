// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace WinGetStudio.Views.Controls;

public partial class MonacoEditor
{
    public partial class MonacoCodeLens
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("range")]
        public MonacoRange Range { get; set; }

        [JsonPropertyName("command")]
        public MonacoCodeLensCommand? Command { get; set; }

        public MonacoCodeLens(MonacoRange range)
        {
            Range = range;
        }
    }

    public sealed partial class MonacoRange
    {
        [JsonPropertyName("startLineNumber")]
        public long StartLineNumber { get; set; }

        [JsonPropertyName("startColumn")]
        public long StartColumn { get; set; }

        [JsonPropertyName("endLineNumber")]
        public long EndLineNumber { get; set; }

        [JsonPropertyName("endColumn")]
        public long EndColumn { get; set; }

        public MonacoRange(long startLineNumber, long startColumn, long endLineNumber, long endColumn)
        {
            StartLineNumber = startLineNumber;
            StartColumn = startColumn;
            EndLineNumber = endLineNumber;
            EndColumn = endColumn;
        }
    }

    public sealed partial class MonacoCodeLensCommand : MonacoCommand
    {
        public MonacoCodeLensCommand(string title)
            : base("codeLensCommand", title)
        {
        }
    }

    public partial class MonacoCommand
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("tooltip")]
        public string? Tooltip { get; set; }

        [JsonPropertyName("arguments")]
        public List<MonacoCommandArgument>? Arguments { get; set; }

        public MonacoCommand(string id, string title)
        {
            Id = id;
            Title = title;
        }
    }

    public partial class MonacoCommandArgument
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("value")]
        public object? Value { get; set; }
    }
}
