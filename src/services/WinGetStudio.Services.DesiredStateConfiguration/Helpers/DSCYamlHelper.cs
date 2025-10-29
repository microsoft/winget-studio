// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using YamlDotNet.Serialization;

namespace WinGetStudio.Services.DesiredStateConfiguration.Helpers;

public static class DSCYamlHelper
{
    // YAML serializer and deserializer for DSC objects.
    private static readonly ISerializer _yamlSerializer = new SerializerBuilder().DisableAliases().WithQuotingNecessaryStrings().Build();
    private static readonly IDeserializer _yamlDeserializer = new DeserializerBuilder().WithAttemptingUnquotedStringTypeDeserialization().Build();

    /// <summary>
    /// Converts an object to its YAML representation.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">The object to convert.</param>
    /// <returns>The YAML string representation of the object.</returns>
    public static string ToYaml<T>(T obj) => _yamlSerializer.Serialize(obj);

    /// <summary>
    /// Converts a YAML string to an object of type T.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="input">The YAML string.</param>
    /// <returns>>The deserialized object of type T.</returns>
    public static T FromYaml<T>(string input) => _yamlDeserializer.Deserialize<T>(input);

    /// <summary>
    /// Creates a deep copy of an object by serializing it to YAML and then deserializing it back.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">The object to copy.</param>
    /// <returns>>A deep copy of the object.</returns>
    public static T DeepCopy<T>(T obj) => FromYaml<T>(ToYaml(obj));

    /// <summary>
    /// Converts a JSON string to a YAML string.
    /// </summary>
    /// <remarks> Since JSON is a subset of YAML, we can parse the JSON string as YAML and then serialize it</remarks>
    /// <param name="json">The JSON string.</param>
    /// <returns>>The YAML string.</returns>
    public static string JsonToYaml(string json) => ToYaml(FromYaml<object>(json));
}
