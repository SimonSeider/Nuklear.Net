using Nuklear.Net.Rendering;

namespace Nuklear.Net;

public static class ContextUiExtensions
{
    public static float ScaleUi(this Context context, float value) => value * context.UiScale;
}
