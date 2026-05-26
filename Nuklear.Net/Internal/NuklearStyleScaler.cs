using Nuklear.Net.Native;

namespace Nuklear.Net.Internal;

internal static class NuklearStyleScaler
{
    public static void Apply(NkContext context, float scale)
    {
        if (MathF.Abs(scale - 1f) < 0.001f)
            return;

        nuklear.NkStyleDefault(context);
        var style = context.Style;
        ScaleButton(style.Button, scale);
        ScaleButton(style.ContextualButton, scale);
        ScaleButton(style.MenuButton, scale);
        ScaleToggle(style.Option, scale);
        ScaleToggle(style.Checkbox, scale);
        ScaleSelectable(style.Selectable, scale);
        ScaleSlider(style.Slider, scale);
        ScaleKnob(style.Knob, scale);
        ScaleProgress(style.Progress, scale);
        ScaleProperty(style.Property, scale);
        ScaleEdit(style.Edit, scale);
        ScaleChart(style.Chart, scale);
        ScaleScrollbar(style.Scrollh, scale);
        ScaleScrollbar(style.Scrollv, scale);
        ScaleTab(style.Tab, scale);
        ScaleCombo(style.Combo, scale);
        ScaleWindow(style.Window, scale);
        ScaleText(style.Text, scale);
    }

    static void ScaleText(NkStyleText text, float scale) => ScaleVec2(text.Padding, scale);

    static void ScaleButton(NkStyleButton button, float scale)
    {
        button.Border *= scale;
        button.Rounding *= scale;
        ScaleVec2(button.Padding, scale);
        ScaleVec2(button.ImagePadding, scale);
        ScaleVec2(button.TouchPadding, scale);
    }

    static void ScaleToggle(NkStyleToggle toggle, float scale)
    {
        toggle.Border *= scale;
        toggle.Spacing *= scale;
        ScaleVec2(toggle.Padding, scale);
        ScaleVec2(toggle.TouchPadding, scale);
    }

    static void ScaleSelectable(NkStyleSelectable selectable, float scale)
    {
        selectable.Rounding *= scale;
        ScaleVec2(selectable.Padding, scale);
        ScaleVec2(selectable.TouchPadding, scale);
        ScaleVec2(selectable.ImagePadding, scale);
    }

    static void ScaleSlider(NkStyleSlider slider, float scale)
    {
        slider.BarHeight *= scale;
        slider.Rounding *= scale;
        slider.Border *= scale;
        ScaleVec2(slider.Padding, scale);
        ScaleVec2(slider.Spacing, scale);
        ScaleVec2(slider.CursorSize, scale);
    }

    static void ScaleKnob(NkStyleKnob knob, float scale)
    {
        knob.Border *= scale;
        knob.KnobBorder *= scale;
        knob.CursorWidth *= scale;
        ScaleVec2(knob.Padding, scale);
        ScaleVec2(knob.Spacing, scale);
    }

    static void ScaleProgress(NkStyleProgress progress, float scale)
    {
        progress.Rounding *= scale;
        progress.Border *= scale;
        ScaleVec2(progress.Padding, scale);
    }

    static void ScaleProperty(NkStyleProperty property, float scale)
    {
        property.Rounding *= scale;
        property.Border *= scale;
        ScaleVec2(property.Padding, scale);
    }

    static void ScaleEdit(NkStyleEdit edit, float scale)
    {
        edit.Rounding *= scale;
        edit.Border *= scale;
        edit.CursorSize *= scale;
        edit.RowPadding *= scale;
        ScaleVec2(edit.Padding, scale);
        ScaleVec2(edit.ScrollbarSize, scale);
    }

    static void ScaleChart(NkStyleChart chart, float scale)
    {
        chart.Rounding *= scale;
        chart.Border *= scale;
        ScaleVec2(chart.Padding, scale);
    }

    static void ScaleScrollbar(NkStyleScrollbar scrollbar, float scale)
    {
        scrollbar.Rounding *= scale;
        scrollbar.Border *= scale;
        ScaleVec2(scrollbar.Padding, scale);
    }

    static void ScaleTab(NkStyleTab tab, float scale)
    {
        tab.Rounding *= scale;
        tab.Border *= scale;
        ScaleVec2(tab.Padding, scale);
        ScaleVec2(tab.Spacing, scale);
    }

    static void ScaleCombo(NkStyleCombo combo, float scale)
    {
        combo.Rounding *= scale;
        combo.Border *= scale;
        ScaleVec2(combo.ContentPadding, scale);
        ScaleVec2(combo.ButtonPadding, scale);
        ScaleVec2(combo.Spacing, scale);
    }

    static void ScaleWindow(NkStyleWindow window, float scale)
    {
        window.Rounding *= scale;
        window.Border *= scale;
        window.PopupBorder *= scale;
        ScaleVec2(window.Padding, scale);
        ScaleVec2(window.Spacing, scale);
        ScaleWindowHeader(window.Header, scale);
    }

    static void ScaleWindowHeader(NkStyleWindowHeader header, float scale)
    {
        ScaleVec2(header.Padding, scale);
        ScaleVec2(header.LabelPadding, scale);
        ScaleVec2(header.Spacing, scale);
    }

    static void ScaleVec2(NkVec2 vec, float scale)
    {
        vec.X *= scale;
        vec.Y *= scale;
    }
}
