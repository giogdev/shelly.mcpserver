namespace Shelly.Test.Helpers;

public static class TestFixtureLoader
{
    public static string LoadEmbeddedJson(string resourceName)
    {
        var assembly = typeof(TestFixtureLoader).Assembly;
        var fullName = assembly.GetManifestResourceNames()
            .First(n => n.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase));
        using var stream = assembly.GetManifestResourceStream(fullName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
