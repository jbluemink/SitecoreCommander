namespace SitecoreCommander.Examples
{
    public enum ExampleAuthMode
    {
        Auto,
        Jwt,
        UserJson
    }

    internal static class ExampleAuthContext
    {
        internal static ExampleAuthMode SelectedMode { get; set; } = ExampleAuthMode.Auto;
    }
}
