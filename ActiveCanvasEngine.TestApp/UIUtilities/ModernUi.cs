using System.ComponentModel;

namespace ActiveCanvasEngine.TestApp
{
    /// <summary>Colours of one look of the test app (light or dark).</summary>
    internal sealed class UiPalette
    {
        public required Color Window { get; init; }      // panels, tool strips, status bar
        public required Color Content { get; init; }     // lists, property grid
        public required Color Text { get; init; }
        public required Color SubtleText { get; init; }  // section headers, hints
        public required Color Icon { get; init; }
        public required Color Line { get; init; }        // thin lines below bars, separators
        public required Color Hover { get; init; }
        public required Color Pressed { get; init; }     // pressed and checked buttons
        public required Color Border { get; init; }      // border of hovered/pressed buttons
        public required Color Accent { get; init; }

        public static readonly UiPalette Light = new()
        {
            Window = Color.FromArgb(245, 247, 250),
            Content = Color.White,
            Text = Color.FromArgb(31, 41, 55),
            SubtleText = Color.FromArgb(100, 116, 139),
            Icon = Color.FromArgb(51, 65, 85),
            Line = Color.FromArgb(222, 226, 230),
            Hover = Color.FromArgb(229, 236, 245),
            Pressed = Color.FromArgb(212, 224, 240),
            Border = Color.FromArgb(196, 212, 232),
            Accent = Color.FromArgb(0, 120, 215),
        };

        public static readonly UiPalette Dark = new()
        {
            Window = Color.FromArgb(37, 37, 38),
            Content = Color.FromArgb(30, 30, 30),
            Text = Color.FromArgb(230, 230, 230),
            SubtleText = Color.FromArgb(150, 150, 150),
            Icon = Color.FromArgb(210, 214, 220),
            Line = Color.FromArgb(60, 60, 62),
            Hover = Color.FromArgb(62, 62, 64),
            Pressed = Color.FromArgb(0, 84, 153),
            Border = Color.FromArgb(80, 80, 84),
            Accent = Color.FromArgb(55, 148, 255),
        };
    }

    /// <summary>
    /// Flat, current look of the standard WinForms controls (tool strips, status strip, lists, property grid):
    /// plain background, light hover and pressed state, thin lines and separators, no gradients or rounded edges.
    /// </summary>
    internal static class ModernUi
    {
        /// <summary>Applies the palette to a control and all its children (the canvas PictureBox is left to the engine).</summary>
        public static void Apply(Control root, UiPalette palette)
        {
            var renderer = new FlatToolStripRenderer(palette);
            ToolStripManager.Renderer = renderer;   // drop-down menus and context menus
            ApplyTo(root, palette, renderer);
        }

        private static void ApplyTo(Control control, UiPalette palette, ToolStripRenderer renderer)
        {
            switch (control)
            {
                case PictureBox:
                    return;   // the ActiveCanvas engine paints it (ApplyDarkTheme/ApplyLightTheme)

                case ToolStrip strip:
                    strip.Renderer = renderer;
                    strip.BackColor = palette.Window;
                    strip.ForeColor = palette.Text;
                    if (strip is not StatusStrip)
                        strip.GripStyle = ToolStripGripStyle.Hidden;
                    foreach (ToolStripItem item in strip.Items)
                        item.ForeColor = palette.Text;
                    break;

                case PropertyGrid grid:
                    grid.BackColor = palette.Window;
                    grid.ViewBackColor = palette.Content;
                    grid.ViewForeColor = palette.Text;
                    grid.ViewBorderColor = palette.Line;
                    grid.LineColor = palette.Window;
                    grid.CategoryForeColor = palette.Text;
                    grid.CategorySplitterColor = palette.Window;
                    grid.HelpBackColor = palette.Window;
                    grid.HelpForeColor = palette.Text;
                    grid.HelpBorderColor = palette.Line;
                    grid.SelectedItemWithFocusBackColor = palette.Accent;
                    grid.SelectedItemWithFocusForeColor = Color.White;
                    grid.DisabledItemForeColor = palette.SubtleText;
                    return;   // its inner controls are themed through the properties above

                case ListBox or ListView or TextBoxBase:
                    control.BackColor = palette.Content;
                    control.ForeColor = palette.Text;
                    break;

                case SectionHeader header:
                    header.BackColor = palette.Window;
                    header.ForeColor = palette.SubtleText;
                    header.LineColor = palette.Line;
                    break;

                case Button button:
                    button.FlatStyle = FlatStyle.Flat;
                    button.BackColor = palette.Content;
                    button.ForeColor = palette.Text;
                    button.FlatAppearance.BorderColor = palette.Border;
                    break;

                default:
                    control.BackColor = control is NumericUpDown ? palette.Content : palette.Window;
                    control.ForeColor = palette.Text;
                    break;
            }

            foreach (Control child in control.Controls)
                ApplyTo(child, palette, renderer);
        }

        /// <summary>Plain background, light hover and pressed state, thin line below each bar.</summary>
        private sealed class FlatToolStripRenderer : ToolStripProfessionalRenderer
        {
            private readonly UiPalette _palette;

            public FlatToolStripRenderer(UiPalette palette) : base(new FlatColorTable(palette))
            {
                _palette = palette;
                RoundedEdges = false;
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                if (e.ToolStrip is ToolStripDropDown)
                    base.OnRenderToolStripBorder(e);   // menus keep their frame
                else
                {
                    using var line = new Pen(_palette.Line);
                    int y = e.ToolStrip is StatusStrip ? 0 : e.ToolStrip.Height - 1;   // status bar: line on top
                    e.Graphics.DrawLine(line, 0, y, e.ToolStrip.Width, y);
                }
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = e.Item.Enabled ? _palette.Text : _palette.SubtleText;
                base.OnRenderItemText(e);
            }

            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
            {
                e.ArrowColor = _palette.Text;
                base.OnRenderArrow(e);
            }
        }

        private sealed class FlatColorTable : ProfessionalColorTable
        {
            private readonly UiPalette _p;
            public FlatColorTable(UiPalette palette) { _p = palette; UseSystemColors = false; }

            public override Color ToolStripGradientBegin => _p.Window;
            public override Color ToolStripGradientMiddle => _p.Window;
            public override Color ToolStripGradientEnd => _p.Window;
            public override Color ToolStripBorder => _p.Window;
            public override Color ToolStripDropDownBackground => _p.Content;
            public override Color ToolStripContentPanelGradientBegin => _p.Window;
            public override Color ToolStripContentPanelGradientEnd => _p.Window;
            public override Color StatusStripGradientBegin => _p.Window;
            public override Color StatusStripGradientEnd => _p.Window;
            public override Color OverflowButtonGradientBegin => _p.Window;
            public override Color OverflowButtonGradientMiddle => _p.Window;
            public override Color OverflowButtonGradientEnd => _p.Window;
            public override Color GripDark => _p.Window;
            public override Color GripLight => _p.Window;
            public override Color SeparatorDark => _p.Line;
            public override Color SeparatorLight => _p.Window;

            public override Color ButtonSelectedHighlight => _p.Hover;
            public override Color ButtonSelectedGradientBegin => _p.Hover;
            public override Color ButtonSelectedGradientMiddle => _p.Hover;
            public override Color ButtonSelectedGradientEnd => _p.Hover;
            public override Color ButtonSelectedBorder => _p.Border;
            public override Color ButtonPressedHighlight => _p.Pressed;
            public override Color ButtonPressedGradientBegin => _p.Pressed;
            public override Color ButtonPressedGradientMiddle => _p.Pressed;
            public override Color ButtonPressedGradientEnd => _p.Pressed;
            public override Color ButtonPressedBorder => _p.Border;
            public override Color ButtonCheckedHighlight => _p.Pressed;
            public override Color ButtonCheckedHighlightBorder => _p.Border;
            public override Color ButtonCheckedGradientBegin => _p.Pressed;
            public override Color ButtonCheckedGradientMiddle => _p.Pressed;
            public override Color ButtonCheckedGradientEnd => _p.Pressed;
            public override Color CheckBackground => _p.Pressed;
            public override Color CheckSelectedBackground => _p.Pressed;
            public override Color CheckPressedBackground => _p.Pressed;

            public override Color MenuItemSelected => _p.Hover;
            public override Color MenuItemBorder => _p.Border;
            public override Color MenuItemSelectedGradientBegin => _p.Hover;
            public override Color MenuItemSelectedGradientEnd => _p.Hover;
            public override Color MenuItemPressedGradientBegin => _p.Pressed;
            public override Color MenuItemPressedGradientMiddle => _p.Pressed;
            public override Color MenuItemPressedGradientEnd => _p.Pressed;
            public override Color MenuBorder => _p.Border;
            public override Color ImageMarginGradientBegin => _p.Content;
            public override Color ImageMarginGradientMiddle => _p.Content;
            public override Color ImageMarginGradientEnd => _p.Content;
        }
    }

    /// <summary>Section title of a side panel in small capitals (e.g. "PROPERTIES"), with a thin line below.</summary>
    internal sealed class SectionHeader : Label
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color LineColor { get; set; } = Color.FromArgb(222, 226, 230);

        public SectionHeader()
        {
            Dock = DockStyle.Top;
            AutoSize = false;
            Height = 28;
            Padding = new Padding(8, 0, 0, 0);
            TextAlign = ContentAlignment.MiddleLeft;
            Font = new Font(SystemFonts.MessageBoxFont ?? Control.DefaultFont, FontStyle.Regular);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (Text != Text.ToUpperInvariant())
                Text = Text.ToUpperInvariant();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using var line = new Pen(LineColor);
            e.Graphics.DrawLine(line, 0, Height - 1, Width, Height - 1);
        }
    }
}
