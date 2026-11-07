using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace IconPicker
{
    /// <summary>
    /// Helper that loads a downscaled (square) thumbnail of an image or icon file.
    /// Supports PNG, BMP, ICO, JPG and GIF (the formats GDI+ can read via System.Drawing).
    /// </summary>
    internal static class IconThumbnailLoader
    {
        /// <summary>
        /// Loads the file and returns a new size x size bitmap thumbnail.
        /// Returns null on error (corrupt file, unsupported format, ...).
        /// </summary>
        public static Image LoadThumbnail(string filePath, int size)
        {
            try
            {
                if (string.Equals(Path.GetExtension(filePath), ".ico", StringComparison.OrdinalIgnoreCase))
                {
                    // For .ico, ask directly for the variant of the requested size (if the file has one);
                    // otherwise Windows picks the closest one and it's scaled below.
                    using (var icon = new Icon(filePath, new Size(size, size)))
                    using (var iconBitmap = icon.ToBitmap())
                    {
                        return ResizeToSquare(iconBitmap, size);
                    }
                }

                using (var original = Image.FromFile(filePath))
                {
                    return ResizeToSquare(original, size);
                }
            }
            catch
            {
                // A corrupt / unsupported file is simply skipped.
                return null;
            }
        }

        private static Bitmap ResizeToSquare(Image source, int size)
        {
            var bmp = new Bitmap(size, size);
            bmp.SetResolution(source.HorizontalResolution <= 0 ? 96 : source.HorizontalResolution,
                               source.VerticalResolution <= 0 ? 96 : source.VerticalResolution);

            using (var g = Graphics.FromImage(bmp))
            {
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.Clear(Color.Transparent);

                // Keep the aspect ratio and center the thumbnail so it isn't distorted.
                float scale = Math.Min((float)size / source.Width, (float)size / source.Height);
                int drawWidth = Math.Max(1, (int)Math.Round(source.Width * scale));
                int drawHeight = Math.Max(1, (int)Math.Round(source.Height * scale));
                int offsetX = (size - drawWidth) / 2;
                int offsetY = (size - drawHeight) / 2;

                g.DrawImage(source, new Rectangle(offsetX, offsetY, drawWidth, drawHeight));
            }

            return bmp;
        }
    }
}
