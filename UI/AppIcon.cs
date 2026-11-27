namespace Cliperino.UI;

static class AppIcon
{
    const string Resource = "Cliperino.ico";

    public static Icon Value { get; } = Load();

    static Icon Load()
    {
        using var stream = typeof(AppIcon).Assembly.GetManifestResourceStream(Resource)!;
        return new Icon(stream);
    }
}
