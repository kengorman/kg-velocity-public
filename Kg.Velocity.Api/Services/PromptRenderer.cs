using System.Text;

namespace Kg.Velocity.Api.Services;

public static class PromptRenderer
{
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





