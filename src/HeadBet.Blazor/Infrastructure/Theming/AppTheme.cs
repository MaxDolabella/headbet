using MudBlazor;

namespace HeadBet.Blazor.Infrastructure.Theming;

/// <summary>
/// Tema do HeadBet — paleta da marca: verde #006933, vermelho #f94134, azul #005ea4.
/// </summary>
public static class AppTheme
{
    public static readonly MudTheme Default = new()
    {
        PaletteLight = BuildLight(),
        PaletteDark = BuildDark(),
        Typography = BuildTypography(),
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "10px",
            DrawerWidthLeft = "260px",
            AppbarHeight = "64px",
        },
    };

    private static PaletteDark BuildDark() => new()
    {
        Primary = "#006933",           // verde HeadBet
        Secondary = "#f59e0b",         // âmbar (acento)
        Tertiary = "#005ea4",          // azul HeadBet
        Info = "#005ea4",
        Success = "#006933",
        Warning = "#f59e0b",
        Error = "#f94134",             // vermelho HeadBet
        AppbarBackground = "#08081f",  // mais escuro que o BG, pra destacar
        DrawerBackground = "#13133a",
        Background = "#0e0e2c",        // BG HeadBet
        Surface = "#1a1a47",           // cards/papers (levemente acima do BG)
        DrawerText = "rgba(255,255,255, 0.85)",
        DrawerIcon = "rgba(255,255,255, 0.85)",
        TextPrimary = "rgba(255,255,255, 0.92)",
        TextSecondary = "rgba(255,255,255, 0.65)",
        ActionDefault = "rgba(255,255,255, 0.65)",
        ActionDisabled = "rgba(255,255,255, 0.30)",
        Divider = "rgba(255,255,255, 0.08)",
        DividerLight = "rgba(255,255,255, 0.04)",
        TableLines = "rgba(255,255,255, 0.06)",
        LinesDefault = "rgba(255,255,255, 0.10)",
        OverlayDark = "rgba(0,0,0,0.6)",
    };

    private static PaletteLight BuildLight() => new()
    {
        Primary = "#006933",           // verde HeadBet
        Secondary = "#d97706",
        Tertiary = "#005ea4",          // azul HeadBet
        Info = "#005ea4",
        Success = "#006933",
        Warning = "#d97706",
        Error = "#f94134",             // vermelho HeadBet
        AppbarBackground = "#006933",
        AppbarText = "#ffffff",
    };

    private static Typography BuildTypography() => new()
    {
        Default = new DefaultTypography
        {
            FontFamily = ["Roboto", "Helvetica", "Arial", "sans-serif"],
            FontSize = ".925rem",
        },
        H1 = new H1Typography { FontSize = "2.25rem", FontWeight = "600" },
        H2 = new H2Typography { FontSize = "1.85rem", FontWeight = "600" },
        H3 = new H3Typography { FontSize = "1.55rem", FontWeight = "600" },
        H4 = new H4Typography { FontSize = "1.35rem", FontWeight = "600" },
        H5 = new H5Typography { FontSize = "1.2rem", FontWeight = "600" },
        H6 = new H6Typography { FontSize = "1.05rem", FontWeight = "600" },
    };
}
