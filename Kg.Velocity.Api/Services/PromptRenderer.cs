using System.Text;

namespace Kg.Velocity.Api.Services;

/// <summary>
/// Replaces {{Name}} placeholders in a prompt with real values. Shared by all the prompt builders.
/// </summary>
public static class PromptRenderer
{
    /// <summary>Replaces {{token}} placeholders in prompt templates with trip-specific values.</summary>
    public static string Render(string template, IReadOnlyDictionary<string, string> tokens)
    {
        if (string.IsNullOrEmpty(template) || tokens.Count == 0)
            return template;

        var sb = new StringBuilder(template);
        foreach (var (key, value) in tokens)
        {
            sb.Replace("{{" + key + "}}", value);
        }

        return sb.ToString();
    }
}
















