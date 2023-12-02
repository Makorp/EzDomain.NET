using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EzDomain.EventSourcing.EventStores.Sql;

public interface ISqlScriptsLoader
{
    string GetScript(string scriptName);
}

[ExcludeFromCodeCoverage]
public sealed class SqlScriptsLoader
    : ISqlScriptsLoader
{
    private static readonly IDictionary<string, string> Scripts = new Dictionary<string, string>();

    public SqlScriptsLoader(params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var sqlScriptsNames = assembly.GetManifestResourceNames();

            foreach (var sqlScriptsName in sqlScriptsNames)
            {
                using var stream = assembly.GetManifestResourceStream(sqlScriptsName);
                using var reader = new StreamReader(stream!);

                var scriptContent = reader.ReadToEnd();

                var key = Path.GetFileNameWithoutExtension(sqlScriptsName);
                Scripts.Add(key, scriptContent);
            }
        }
    }

    public string GetScript(string scriptName)
    {
        Scripts.TryGetValue(scriptName, out var scriptContent);

        if (string.IsNullOrWhiteSpace(scriptContent))
            throw new InvalidOperationException($"Script \"{scriptName}\" not found.");

        return scriptContent;
    }
}