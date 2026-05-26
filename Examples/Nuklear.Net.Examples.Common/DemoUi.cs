using Nuklear.Net.Native;

namespace Nuklear.Net.Examples.Common;

public static class DemoUi
{
    public static void Build(Context context, ref float sliderValue, ref int clickCount)
    {
        var nk = context.Native;
        using var bounds = new NkRect
        {
            X = context.ScaleUi(50),
            Y = context.ScaleUi(50),
            W = context.ScaleUi(280),
            H = context.ScaleUi(220),
        };

        var flags = NkPanelFlags.NK_WINDOW_BORDER
            | NkPanelFlags.NK_WINDOW_MOVABLE
            | NkPanelFlags.NK_WINDOW_CLOSABLE
            | NkPanelFlags.NK_WINDOW_TITLE;

        if (nuklear.NkBegin(nk, "Nuklear.Net", bounds, (uint)flags))
        {
            nuklear.NkLayoutRowDynamic(nk, context.ScaleUi(30), 1);
            if (nuklear.NkButtonLabel(nk, "Click me"))
                clickCount++;

            nuklear.NkLayoutRowDynamic(nk, context.ScaleUi(30), 1);
            nuklear.NkLabel(nk, $"Clicks: {clickCount}", (uint)NkTextAlignment.NK_TEXT_LEFT);

            nuklear.NkLayoutRowDynamic(nk, context.ScaleUi(30), 1);
            nuklear.NkSliderFloat(nk, 0, ref sliderValue, 1, 0.05f);
        }

        nuklear.NkEnd(nk);
    }
}
