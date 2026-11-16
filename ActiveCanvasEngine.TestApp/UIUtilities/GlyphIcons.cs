using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace ActiveCanvasEngine.TestApp
{
    /// <summary>
    /// Code points of the Google Material Symbols (Outlined) glyphs used by the test app. Any other glyph
    /// can be added - look up its code point at https://fonts.google.com/icons (the embedded font contains
    /// the whole set as a static instance: FILL 0, weight 400, grade 0, optical size 24).
    /// </summary>
    internal static class Glyph
    {
        public const string Select = "";        // arrow_selector_tool
        public const string Point = "";         // location_on
        public const string Polyline = "";      // polyline
        public const string Polygon = "";       // pentagon
        public const string Icon = "";          // add_photo_alternate
        public const string Measure = "";       // straighten
        public const string Delete = "";        // delete
        public const string ClearAll = "";      // delete_sweep
        public const string Backgrounds = "";   // layers
        public const string Image = "";         // image
        public const string Map = "";           // map
        public const string Orientation = "";   // explore
        public const string View3D = "";        // view_in_ar
        public const string Views = "";         // visibility
        public const string ZoomToFit = "";     // fit_screen
        public const string DarkMode = "";      // dark_mode
        public const string LightMode = "";     // light_mode
        public const string ClearLog = "";      // clear_all
    }

    /// <summary>
    /// Toolbar and menu icons drawn from the Google Material Symbols font (Apache License 2.0, see
    /// Fonts/MaterialSymbols-LICENSE.txt), embedded in the assembly - nothing has to be installed. Bitmaps
    /// are made for the DPI of the window (sharp at 125 % or 150 %), in any colour (dark mode), and cached.
    /// </summary>
    internal static class GlyphIcons
    {
        private const string FontResource = "ActiveCanvasEngine.TestApp.Fonts.MaterialSymbolsOutlined.ttf";

        private static readonly Dictionary<(string Glyph, int Pixels, int Argb), Bitmap> Cache = new();
        private static PrivateFontCollection? _fonts;
        private static bool _loadFailed;

        /// <summary>The embedded Material Symbols font family, or null if it can't be loaded.</summary>
        private static FontFamily? Family
        {
            get
            {
                if (_fonts == null && !_loadFailed)
                {
                    try
                    {
                        using Stream? stream = typeof(GlyphIcons).Assembly.GetManifestResourceStream(FontResource);
                        if (stream == null) { _loadFailed = true; return null; }

                        byte[] data = new byte[stream.Length];
                        stream.ReadExactly(data);

                        // AddMemoryFont needs the memory for as long as the font is used - i.e. the whole
                        // lifetime of the app, so it's deliberately never freed.
                        IntPtr memory = Marshal.AllocCoTaskMem(data.Length);
                        Marshal.Copy(data, 0, memory, data.Length);

                        var fonts = new PrivateFontCollection();
                        fonts.AddMemoryFont(memory, data.Length);
                        _fonts = fonts;
                    }
                    catch (Exception)
                    {
                        _loadFailed = true;
                    }
                }
                return _fonts?.Families.FirstOrDefault();
            }
        }

        /// <summary>Icon of the given logical size (pixels at 96 DPI) for the DPI of the control; null if the font is missing.</summary>
        public static Bitmap? Get(string glyph, Color color, Control dpiSource, int logicalSize = 20)
        {
            int px = Math.Max(8, (int)Math.Round(logicalSize * dpiSource.DeviceDpi / 96f));
            var key = (glyph, px, color.ToArgb());
            if (Cache.TryGetValue(key, out Bitmap? cached))
                return cached;

            FontFamily? family = Family;
            if (family == null)
                return null;

            var bmp = new Bitmap(px, px, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            using (var path = new GraphicsPath())
            using (var brush = new SolidBrush(color))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                // The glyphs are designed on a 24 px grid with a 2 px margin, so an em size equal to the
                // bitmap size gives the intended proportions; the path is centred on its own bounds.
                path.AddString(glyph, family, (int)FontStyle.Regular, px, PointF.Empty, StringFormat.GenericTypographic);
                RectangleF bounds = path.GetBounds();
                if (bounds.Width > 0 && bounds.Height > 0)
                {
                    using var move = new Matrix();
                    move.Translate(px / 2f - (bounds.X + bounds.Width / 2), px / 2f - (bounds.Y + bounds.Height / 2));
                    path.Transform(move);
                }
                g.FillPath(brush, path);
            }

            Cache[key] = bmp;
            return bmp;
        }

        /// <summary>Sets the icon of a tool strip button or menu item (keeps its current image if the font is missing).</summary>
        public static void Apply(ToolStripItem item, string glyph, Color color, Control dpiSource, int logicalSize = 20)
        {
            Bitmap? bmp = Get(glyph, color, dpiSource, logicalSize);
            if (bmp == null)
                return;
            item.Image = bmp;
            item.ImageScaling = ToolStripItemImageScaling.None;
            item.ImageTransparentColor = Color.Empty;
        }
    }
}
