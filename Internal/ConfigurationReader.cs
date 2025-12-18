namespace SimpleDiscordNet_DemoApp.Internal;

/// <summary>
/// Reads bot configuration from a key=value configuration file.
/// </summary>
public sealed class ConfigurationReader
{
    private readonly Dictionary<string, string> _values = new();

    /// <summary>
    /// Loads configuration from the specified file.
    /// </summary>
    /// <param name="filePath">Path to the configuration file.</param>
    /// <returns>True if the file was loaded successfully, false if the file does not exist.</returns>
    /// <exception cref="IOException">Thrown if the file exists but cannot be read.</exception>
    public bool Load(string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        _values.Clear();

        foreach (string line in File.ReadAllLines(filePath))
        {
            string trimmed = line.Trim();

            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
                continue;

            int separatorIndex = trimmed.IndexOf('=');
            if (separatorIndex <= 0)
                continue;

            string key = trimmed.Substring(0, separatorIndex).Trim();
            string value = trimmed.Substring(separatorIndex + 1).Trim();

            _values[key] = value;
        }

        return true;
    }

    /// <summary>
    /// Gets the value for the specified key, or null if not found.
    /// </summary>
    public string? GetValue(string key)
    {
        return _values.TryGetValue(key, out string? value) ? value : null;
    }

    /// <summary>
    /// Gets the value for the specified key, or the default value if not found or empty.
    /// </summary>
    public string GetValueOrDefault(string key, string defaultValue)
    {
        string? value = GetValue(key);
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }

    /// <summary>
    /// Checks if the configuration contains a non-empty value for the specified key.
    /// </summary>
    public bool HasValue(string key)
    {
        return _values.TryGetValue(key, out string? value) && !string.IsNullOrWhiteSpace(value);
    }
}
