using Tilework.Core;

namespace Tilework.Ui.Utilities;

public static class PageTitleHelper
{
    public static string Format(string pageTitle)
    {
        return $"{pageTitle} | {AppMetadata.Name}";
    }
}
