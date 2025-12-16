using Metapsi.Html;
using Metapsi.Syntax;

namespace MetapsiCRM;

public static class CssExtensions
{
    public static void FlexRow<T>(this PropsBuilder<T> b)
    {
        b.AddStyle("display", "flex");
        b.AddStyle("flex-direction", "row");
    }

    public static void FlexColumn<T>(this PropsBuilder<T> b)
    {
        b.AddStyle("display", "flex");
        b.AddStyle("flex-direction", "column");
    }

    public static void AlignItemsCenter<T>(this PropsBuilder<T> b)
    {
        b.AddStyle("align-items", "center");
    }

    public static void JustifyContentCenter<T>(this PropsBuilder<T> b)
    {
        b.AddStyle("justify-content", "center");
    }

    public static void HeightFull<T>(this PropsBuilder<T> b)
    {
        b.AddStyle("height", "100%");
    }
}
