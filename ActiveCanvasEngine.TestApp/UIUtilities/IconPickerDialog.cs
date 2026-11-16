using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace IconPicker
{
    /// <summary>
    /// A simple dialog for picking an image/icon (PNG, BMP, ICO, ...) from a predefined folder - the icons
    /// are shown as small thumbnails in a grid (10 per row, 32x32 px by default). The user can also change
    /// the folder, or pick any other file through a standard OpenFileDialog instead.
    ///
    /// Example:
    ///
    ///     using (var dlg = new IconPickerDialog(@"C:\MyApp\Resources\Icons"))
    ///     {
    ///         dlg.ThumbnailSize = 32;
    ///         dlg.IconsPerRow = 10;
    ///         dlg.AllowedExtensions = new[] { ".png", ".bmp", ".ico" };
    ///
    ///         if (dlg.ShowDialog(this) == DialogResult.OK)
    ///         {
    ///             string selectedPath = dlg.SelectedFilePath;
    ///             pictureBoxPreview.Image = Image.FromFile(selectedPath);
    ///         }
    ///     }
    ///
    /// NOTE on [Browsable(false)]/[DesignerSerializationVisibility(Hidden)] below: this dialog is only ever
    /// created in code (new IconPickerDialog(...)) and should never end up as a component dropped onto
    /// another Form in the VS Designer. Since it derives from Form, though, the Designer would - if it ever
    /// got there (dragged by mistake, or on a project reload) - try to generate serialization code for all
    /// its public properties into InitializeComponent(). Hiding those properties from the Designer prevents that.
    ///
    /// [DesignerCategory("")] on the class also covers the case that has nothing to do with dropping it
    /// onto a form - VS scans every public class derived from Form/Control/Component in the background for
    /// the Toolbox/IntelliSense, whether or not anyone uses it in the Designer. The empty category tells
    /// that scan "don't analyze this class as designable at all".
    /// </summary>
    [DesignerCategory("")]
    public class IconPickerDialog : Form
    {
        private readonly TextBox _txtDirectory;
        private readonly Button _btnBrowseDir;
        private readonly FlowLayoutPanel _flpIcons;
        private readonly Button _btnCustomFile;
        private readonly Label _lblStatus;
        private readonly Button _btnOK;
        private readonly Button _btnCancel;

        private IconThumbnailControl? _selectedThumb;

        /// <summary>Folder the icon/image thumbnails are loaded from.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string IconsDirectory
        {
            get => _txtDirectory.Text;
            set => _txtDirectory.Text = value ?? string.Empty;
        }

        /// <summary>Allowed file extensions (with the dot), e.g. ".png", ".bmp", ".ico".</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string[] AllowedExtensions { get; set; } = new string[] { ".png", ".bmp", ".ico" };

        /// <summary>Thumbnail size in pixels (square). Default 32.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ThumbnailSize { get; set; } = 32;

        /// <summary>Icons per row at the default window width (affects only the initial width - when the window is resized, the grid simply wraps).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int IconsPerRow { get; set; } = 10;

        /// <summary>
        /// Path of the file the user picked - by double-click / OK in the icon list, or via the
        /// "Other file..." button. Valid only if ShowDialog() returned DialogResult.OK.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedFilePath { get; private set; } = string.Empty;

        public IconPickerDialog() : this(null)
        {
        }

        public IconPickerDialog(string? initialDirectory)
        {
            Text = "Choose an icon";
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = true;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.Sizable;
            Font = SystemFonts.MessageBoxFont;

            // --- top panel: folder + button to change it ---
            var pnlTop = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 36,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(8, 6, 8, 4)
            };
            pnlTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            pnlTop.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34f));

            _txtDirectory = new TextBox { Dock = DockStyle.Fill };
            _btnBrowseDir = new Button { Text = "...", Dock = DockStyle.Fill };
            _btnBrowseDir.Click += BtnBrowseDir_Click;

            pnlTop.Controls.Add(_txtDirectory, 0, 0);
            pnlTop.Controls.Add(_btnBrowseDir, 1, 0);

            // --- bottom panel: other file / status / OK / Cancel ---
            var pnlBottom = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 44,
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(8, 6, 8, 6)
            };
            pnlBottom.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            pnlBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            pnlBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 84f));
            pnlBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 84f));

            _btnCustomFile = new Button { Text = "Other file...", AutoSize = true, Dock = DockStyle.Fill };
            _btnCustomFile.Click += BtnCustomFile_Click;

            _lblStatus = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.Gray,
                Padding = new Padding(10, 0, 0, 0),
                AutoEllipsis = true
            };

            _btnOK = new Button { Text = "OK", Dock = DockStyle.Fill, DialogResult = DialogResult.OK, Enabled = false };
            _btnOK.Click += (s, e) => ConfirmSelection(_selectedThumb);

            _btnCancel = new Button { Text = "Cancel", Dock = DockStyle.Fill, DialogResult = DialogResult.Cancel };

            pnlBottom.Controls.Add(_btnCustomFile, 0, 0);
            pnlBottom.Controls.Add(_lblStatus, 1, 0);
            pnlBottom.Controls.Add(_btnOK, 2, 0);
            pnlBottom.Controls.Add(_btnCancel, 3, 0);

            // --- middle: grid of icon thumbnails ---
            _flpIcons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(6),
                BackColor = SystemColors.Window,
                BorderStyle = BorderStyle.Fixed3D
            };

            // The order matters: Fill first, then Top/Bottom (see docking in WinForms).
            Controls.Add(_flpIcons);
            Controls.Add(pnlBottom);
            Controls.Add(pnlTop);

            AcceptButton = _btnOK;
            CancelButton = _btnCancel;

            // Window width chosen so that IconsPerRow icons fit into one row at the default size.
            int itemOuterSize = ThumbnailSize + 10 /* item padding */ + 6 /* margin between items */;
            int desiredWidth = itemOuterSize * IconsPerRow + 40 /* borders + scrollbar */;
            ClientSize = new Size(Math.Max(360, desiredWidth), 420);
            MinimumSize = new Size(320, 260);

            Load += IconPickerDialog_Load;

            if (!string.IsNullOrWhiteSpace(initialDirectory))
                IconsDirectory = initialDirectory;
        }

        private void IconPickerDialog_Load(object? sender, EventArgs e)
        {
            LoadIcons();
        }

        private void BtnBrowseDir_Click(object? sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                if (!string.IsNullOrWhiteSpace(IconsDirectory) && Directory.Exists(IconsDirectory))
                    fbd.SelectedPath = IconsDirectory;

                if (fbd.ShowDialog(this) == DialogResult.OK)
                {
                    IconsDirectory = fbd.SelectedPath;
                    LoadIcons();
                }
            }
        }

        private void BtnCustomFile_Click(object? sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog
            {
                Title = "Choose an image file",
                Filter = BuildFileFilter(),
                CheckFileExists = true
            })
            {
                if (!string.IsNullOrWhiteSpace(IconsDirectory) && Directory.Exists(IconsDirectory))
                    ofd.InitialDirectory = IconsDirectory;

                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    SelectedFilePath = ofd.FileName;
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }

        private string BuildFileFilter()
        {
            string extsPattern = string.Join(";", AllowedExtensions.Select(x => "*" + x));
            return $"Images ({extsPattern})|{extsPattern}|All files (*.*)|*.*";
        }

        /// <summary>Reloads the thumbnails from the current folder (IconsDirectory).</summary>
        public void LoadIcons()
        {
            ClearThumbnails();

            string dir = IconsDirectory;
            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
            {
                _lblStatus.Text = "The folder doesn't exist.";
                return;
            }

            List<string> files;
            try
            {
                files = Directory.EnumerateFiles(dir)
                    .Where(f => AllowedExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                    .OrderBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch (Exception ex)
            {
                _lblStatus.Text = "Error reading the folder: " + ex.Message;
                return;
            }

            _flpIcons.SuspendLayout();

            int loaded = 0;
            int failed = 0;

            foreach (var file in files)
            {
                Image? thumb = IconThumbnailLoader.LoadThumbnail(file, ThumbnailSize);
                if (thumb == null)
                {
                    failed++;
                    continue;
                }

                var item = new IconThumbnailControl(file, thumb, ThumbnailSize);
                item.Click += Thumbnail_Click;
                item.DoubleClick += Thumbnail_DoubleClick;
                _flpIcons.Controls.Add(item);
                loaded++;
            }

            _flpIcons.ResumeLayout();

            _lblStatus.Text = failed > 0
                ? $"Loaded {loaded} items ({failed} could not be loaded)."
                : $"Loaded {loaded} items.";

            _selectedThumb = null;
            _btnOK.Enabled = false;
        }

        private void ClearThumbnails()
        {
            foreach (Control c in _flpIcons.Controls)
                c.Dispose();
            _flpIcons.Controls.Clear();
        }

        private void Thumbnail_Click(object? sender, EventArgs e)
        {
            SelectThumbnail((IconThumbnailControl)sender!);
        }

        private void Thumbnail_DoubleClick(object? sender, EventArgs e)
        {
            var thumb = (IconThumbnailControl)sender!;
            SelectThumbnail(thumb);
            ConfirmSelection(thumb);
        }

        private void SelectThumbnail(IconThumbnailControl thumb)
        {
            if (_selectedThumb != null)
                _selectedThumb.Selected = false;

            _selectedThumb = thumb;
            _selectedThumb.Selected = true;
            _btnOK.Enabled = true;
        }

        private void ConfirmSelection(IconThumbnailControl? thumb)
        {
            if (thumb == null)
                return;

            SelectedFilePath = thumb.FilePath;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}