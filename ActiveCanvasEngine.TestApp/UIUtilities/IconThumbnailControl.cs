using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace IconPicker
{
    /// <summary>
    /// One item in the icon list: a small square image thumbnail that can be visually marked as
    /// selected (Selected) and reacts to Click / DoubleClick.
    /// </summary>
    [DesignerCategory("")]
    public class IconThumbnailControl : Panel
    {
        private readonly PictureBox _pictureBox;
        private bool _selected;

        /// <summary>Full path of the file this item represents.</summary>
        public string FilePath { get; }

        /// <summary>Whether this item is currently marked as selected.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                BackColor = _selected ? SystemColors.Highlight : SystemColors.Control;
            }
        }

        public IconThumbnailControl(string filePath, Image thumbnail, int size)
        {
            FilePath = filePath;

            Size = new Size(size + 10, size + 10);
            Margin = new Padding(3);
            Padding = new Padding(3);
            BackColor = SystemColors.Control;
            Cursor = Cursors.Hand;

            _pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.CenterImage,
                Image = thumbnail,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            Controls.Add(_pictureBox);

            var tip = new ToolTip();
            tip.SetToolTip(_pictureBox, Path.GetFileName(filePath));
            tip.SetToolTip(this, Path.GetFileName(filePath));

            // Forward mouse events from the inner PictureBox to this item, so a click/double-click
            // works anywhere on the tile, not just on the Panel's border.
            _pictureBox.Click += (s, e) => OnClick(e);
            _pictureBox.DoubleClick += (s, e) => OnDoubleClick(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _pictureBox.Image?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
