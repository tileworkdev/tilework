using MudBlazor;

namespace Tilework.Ui.Components.Layout;

internal static class TileworkTheme
{
    public static MudTheme Create() => new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#526a57",
            Secondary = "#6d746f",
            Black = "#171918",
            Surface = "#ffffff",
            Background = "#f7f8f7",
            BackgroundGray = "#f1f2f1",
            AppbarText = "#2e322f",
            AppbarBackground = "#ffffff",
            DrawerBackground = "#fafafa",
            ActionDefault = "#626864",
            TextPrimary = "#242825",
            TextSecondary = "#69706b",
            DrawerIcon = "#68706a",
            DrawerText = "#454b47",
            GrayLight = "#e1e4e2",
            GrayLighter = "#f4f5f4",
            Info = "#52758a",
            Success = "#4f7a57",
            Warning = "#b17a2d",
            Error = "#b4514e",
            LinesDefault = "#e0e3e1",
            TableLines = "#e4e6e5",
            Divider = "#e0e3e1",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#86b991",
            PrimaryContrastText = "#142017",
            Secondary = "#91aa9b",
            Surface = "#242825",
            Background = "#191c1a",
            BackgroundGray = "#151816",
            AppbarText = "#e0e5e1",
            AppbarBackground = "rgba(25,28,26,0.94)",
            DrawerBackground = "#1f2421",
            ActionDefault = "#b2bbb4",
            ActionDisabled = "#aeb2af4d",
            ActionDisabledBackground = "#666a674d",
            TextPrimary = "#e5e7e6",
            TextSecondary = "#afb4b1",
            TextDisabled = "#ffffff33",
            DrawerIcon = "#aab9ad",
            DrawerText = "#d0d7d2",
            GrayLight = "#39413b",
            GrayLighter = "#2b302c",
            Info = "#79afc8",
            Success = "#78b985",
            Warning = "#d9a24f",
            Error = "#df7772",
            LinesDefault = "#3a423c",
            TableLines = "#3a423c",
            Divider = "#343b36",
            OverlayLight = "#24282580",
        },
        LayoutProperties = new LayoutProperties(),
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = ["Inter", "Helvetica", "Arial", "sans-serif"]
            }
        }
    };
}
