using System.Globalization;

namespace ActiveCanvasEngine.TestApp
{
    /// <summary>
    /// Test / demo application for the ActiveCanvas engine - every toolbar button exercises one feature.
    /// Keys: Esc = cancel drawing, Enter = finish drawing, Delete = remove the selected item.
    /// </summary>
    public partial class MainForm : Form
    {
        private ActiveCanvas engine;
        private bool _isDark;

        // OSM demo location - the drawing origin (0,0) is anchored here when the map is turned on
        private const double DemoLatitude = 48.172969;   // Duslo, Sala
        private const double DemoLongitude = 17.940856;
        private bool _geoDemoAdded;

        public MainForm()
        {
            InitializeComponent();
            engine = new ActiveCanvas(pictureBox1);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DemoScene.Build(engine);

            toolStripButton3D2Dview.Image = imageList1.Images[1];

            // Status bar, right: GPS position under the cursor while the OSM layer is on.
            pictureBox1.MouseMove += (s, e) =>
            {
                var geo = engine.IsOpenStreetMapEnabled ? engine.ScreenToLatLon(e.Location) : null;
                toolStripStatusLabelCoords.Text = geo == null ? "" : string.Format(CultureInfo.InvariantCulture,
                    "Lat {0:F6}   Lon {1:F6}   |   OSM zoom {2}", geo.Value.Latitude, geo.Value.Longitude, engine.OsmCurrentZoom);
            };

            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                switch (e.KeyCode)
                {
                    case Keys.Escape: engine.CancelDrawing(); break;
                    case Keys.Enter: engine.FinishDrawing(); break;
                    case Keys.Delete when !engine.IsDrawing: engine.RemoveSelected(); break;
                }
            };

            // Property dialogs on double-click. For points, a few rows are hidden to show hiddenProperties.
            engine.PointDoubleClicked += (s, p) =>
                ShowPropertyDialog(p, hiddenProperties: new[] { "Id", "HoverEnabled", "Draggable" });
            engine.PolylineDoubleClicked += (s, pl) => ShowPropertyDialog(pl);
            engine.PolygonDoubleClicked += (s, pg) => ShowPropertyDialog(pg);
            engine.ImageDoubleClicked += (s, im) => ShowPropertyDialog(im);
            engine.ShapeDoubleClicked += (s, sh) => ShowPropertyDialog(sh);
            engine.ConnectorDoubleClicked += (s, c) => ShowPropertyDialog(c);

            // Status bar, left: what just happened - a quick way to see the engine's events at work.
            engine.PointPlaced += (s, p) => ShowStatus(string.Format(CultureInfo.InvariantCulture,
                "Point placed at X = {0:F2}, Y = {1:F2}", engine.ToRaw(p.World).X, engine.ToRaw(p.World).Y));
            engine.PolylineDrawingFinished += (s, pl) => ShowStatus($"Polyline finished ({pl.Vertices.Count} vertices)");
            engine.PolygonDrawingFinished += (s, pg) => ShowStatus($"Polygon finished ({pg.Vertices.Count} vertices)");
            engine.MeasurementCompleted += (s, meters) => ShowStatus(string.Format(CultureInfo.InvariantCulture,
                "Distance: {0:F2} m", meters));
            engine.ItemRemoved += (s, item) => ShowStatus($"Removed: {item.GetType().Name}");
        }

        private void ShowStatus(string message) => toolStripStatusLabelInfo.Text = message;

        private void ShowPropertyDialog(object item, string[]? hiddenProperties = null)
        {
            var grid = engine.CreatePropertyGrid(item, hiddenProperties);
            if (grid == null) return; // unknown item type

            using var dlg = new Form
            {
                Text = "Properties",
                Width = 320,
                Height = 420,
                StartPosition = FormStartPosition.CenterParent
            };
            dlg.Controls.Add(grid); // the grid is Dock = Fill
            dlg.ShowDialog(this);
        }

        // ---- Drawing / placing ----

        private void toolStripButtonAddPoint_Click(object sender, EventArgs e)
        {
            engine.StartPlacingPoints(label: "New point", brush: new SolidBrush(Color.Crimson),
                shape: ActiveCanvas.PointShape.Diamond, size: 8f, continuous: false);
        }

        private void toolStripButtonAddPolyLine_Click(object sender, EventArgs e)
        {
            engine.StartDrawingPolyline("New route", new Pen(Color.ForestGreen, 3f));
            ShowStatus("Click to add vertices, double-click or Enter to finish, Esc to cancel");
        }

        private void toolStripButtonAddPolygon_Click(object sender, EventArgs e)
        {
            engine.StartDrawingPolygon("New area", new SolidBrush(Color.FromArgb(128, Color.LightBlue)), new Pen(Color.DarkBlue, 2f));
            ShowStatus("Click to add vertices, double-click or Enter to finish, Esc to cancel");
        }

        private void toolStripButtonAddIcon_Click(object sender, EventArgs e)
        {
            string iconsPath = Path.Combine(AppContext.BaseDirectory, "testIcons");
            using var dlg = new IconPicker.IconPickerDialog(iconsPath)
            {
                ThumbnailSize = 32,
                IconsPerRow = 60, // affects only the initial window width
                AllowedExtensions = new[] { ".png", ".bmp", ".ico" }
            };

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            // LoadImageFile reads the file fully, so it isn't kept locked
            var icon = ActiveCanvas.LoadImageFile(dlg.SelectedFilePath, maxPixelDimension: 256);
            engine.StartPlacingImages(icon, label: Path.GetFileNameWithoutExtension(dlg.SelectedFilePath),
                width: 32, continuous: false);
        }

        private void toolStripButtonLengthMeasure_Click(object sender, EventArgs e)
        {
            engine.StartMeasuring();
            ShowStatus("Click two points to measure the distance");
        }

        private void toolStripButtonClearAll_Click(object sender, EventArgs e)
        {
            engine.CancelDrawing();
            engine.Clear();
            engine.ClearAllBackgroundImages();
            _geoDemoAdded = false; // re-added the next time the map is turned on
            ShowStatus("Everything cleared");
        }

        // ---- View ----

        private void toolStripButton3D2Dview_Click(object sender, EventArgs e)
        {
            bool isTopView = engine.PitchDegrees >= 89f && engine.PitchDegrees < 91f;
            if (isTopView)
            {
                toolStripButton3D2Dview.Image = imageList1.Images[0];
                engine.ShowIsometricView();
                engine.AllowFreeRotate = true;
            }
            else
            {
                toolStripButton3D2Dview.Image = imageList1.Images[1];
                engine.ShowTopView();
                engine.AllowFreeRotate = false;
            }
            engine.ZoomToFullExtent();
        }

        private void toolStripButtonFullZoom_Click(object sender, EventArgs e)
        {
            engine.ZoomToFullExtent(); // 10% margin; ZoomToFullExtent(0f) fits edge to edge
        }

        private void toolStripButtonDarkMode_Click(object sender, EventArgs e)
        {
            _isDark = !_isDark;

            if (_isDark)
            {
                AppTheme.ApplyDark(this);
                engine.ApplyDarkTheme();
                toolStripButtonDarkMode.Text = "Light mode";
            }
            else
            {
                AppTheme.ApplyLight(this);
                engine.ApplyLightTheme();
                toolStripButtonDarkMode.Text = "Dark mode";
            }
        }

        // ---- Background images ----

        private readonly ContextMenuStrip _backgroundMenu = new ContextMenuStrip();

        /// <summary>Background images: open one per plane (plan / front / side), show/hide, opacity, remove.</summary>
        private void toolStripButtonOpenBackGroundImage_Click(object sender, EventArgs e)
        {
            _backgroundMenu.Items.Clear();
            _backgroundMenu.Items.Add("Open plan view (X-Y)...", null, async (s, a) => await OpenBackgroundAsync(ActiveCanvas.BackgroundPlane.Ground));
            _backgroundMenu.Items.Add("Open front view (X-Z)...", null, async (s, a) => await OpenBackgroundAsync(ActiveCanvas.BackgroundPlane.Front));
            _backgroundMenu.Items.Add("Open side view (Y-Z)...", null, async (s, a) => await OpenBackgroundAsync(ActiveCanvas.BackgroundPlane.Side));

            var planes = new[]
            {
                (Plane: ActiveCanvas.BackgroundPlane.Ground, Name: "plan view"),
                (Plane: ActiveCanvas.BackgroundPlane.Front, Name: "front view"),
                (Plane: ActiveCanvas.BackgroundPlane.Side, Name: "side view")
            };

            foreach (var (plane, name) in planes)
            {
                bool hasImage = engine.HasBackgroundImage(plane);
                _backgroundMenu.Items.Add(new ToolStripSeparator());

                var visible = new ToolStripMenuItem($"Show {name}")
                {
                    Enabled = hasImage,
                    Checked = engine.IsBackgroundVisible(plane)
                };
                visible.Click += (s, a) => engine.SetBackgroundVisible(plane, !engine.IsBackgroundVisible(plane));
                _backgroundMenu.Items.Add(visible);

                var opacity = new ToolStripMenuItem($"Opacity - {name}") { Enabled = hasImage };
                foreach (int percent in new[] { 100, 75, 50, 25 })
                {
                    var option = new ToolStripMenuItem($"{percent} %")
                    {
                        Checked = Math.Abs(engine.GetBackgroundOpacity(plane) * 100f - percent) < 1f
                    };
                    option.Click += (s, a) => engine.SetBackgroundOpacity(plane, percent / 100f);
                    opacity.DropDownItems.Add(option);
                }
                _backgroundMenu.Items.Add(opacity);

                var remove = new ToolStripMenuItem($"Remove {name}") { Enabled = hasImage };
                remove.Click += (s, a) => engine.ClearBackgroundImage(plane);
                _backgroundMenu.Items.Add(remove);
            }

            var button = toolStripButtonOpenBackGroundImage;
            _backgroundMenu.Show(toolStrip1, new Point(button.Bounds.Left, button.Bounds.Bottom));
        }

        private async Task OpenBackgroundAsync(ActiveCanvas.BackgroundPlane plane)
        {
            // A background image is plain local meters / S-JTSK here, not Web Mercator - and it would be
            // hidden under the map anyway (OsmHidesBackgroundImage) - so opening one switches the map off.
            if (toolStripButtonOsm.Checked)
            {
                toolStripButtonOsm.Checked = false;
                SetMapEnabled(false);
            }

            await BackgroundImageOpener.ShowAsync(this, engine, plane);
            if (plane == ActiveCanvas.BackgroundPlane.Ground || !engine.HasBackgroundImage(plane)) return;

            // An elevation is only seen face-on in its own view - switch there so it's visible right away.
            if (plane == ActiveCanvas.BackgroundPlane.Front) engine.ShowFrontView();
            else engine.ShowSideView();

            toolStripButton3D2Dview.Image = imageList1.Images[0]; // we're out of the top view now
            engine.ZoomToFullExtent();
        }

        // ---- OpenStreetMap and drawing orientation ----

        private void toolStripButtonOsm_Click(object sender, EventArgs e) => SetMapEnabled(toolStripButtonOsm.Checked);

        /// <summary>
        /// Everything in this test app is drawn in local "drawing" meters (like a CAD drawing). The geo
        /// anchor places that drawing on the map: drawing 0,0 = the demo location, rotated by the bearing
        /// set with the N° button. The world coordinates never change - the map is fitted under them.
        /// </summary>
        private void SetMapEnabled(bool enabled)
        {
            if (enabled)
            {
                if (!engine.HasGeoAnchor)
                    engine.SetGeoAnchor(DemoLatitude, DemoLongitude); // drawing origin (0,0) lies here

                if (!_geoDemoAdded)
                {
                    AddGeoDemo();
                    _geoDemoAdded = true;
                }

                engine.OsmUserAgent = $"ActiveCanvas.TestApp/{typeof(MainForm).Assembly.GetName().Version?.ToString(3)} (+https://github.com/duroLAB/SimpleInteractiveEngine)";
                engine.EnableOpenStreetMap();   // switches to top view, north up (OsmLockNorthUp = true by default)
                engine.ZoomToLatLon(DemoLatitude, DemoLongitude, zoomLevel: 16);

                // The map keeps the view locked north-up, so 3D/rotation makes no sense meanwhile.
                engine.AllowFreeRotate = false;
                toolStripButton3D2Dview.Image = imageList1.Images[1];
                toolStripButton3D2Dview.Enabled = !engine.IsNorthUpLocked;
            }
            else
            {
                engine.DisableOpenStreetMap();   // the background image (if any) shows again, the view turns back
                toolStripButton3D2Dview.Enabled = true;
                toolStripStatusLabelCoords.Text = "";
                engine.ZoomToFullExtent();
            }
        }

        /// <summary>An item placed by GPS coordinates, to check it sits on the right spot of the map.</summary>
        private void AddGeoDemo()
        {
            // Converted to drawing coordinates through the geo anchor (and the bearing valid right now) -
            // it stays where it is in the drawing if the bearing is changed later. CreatePointLatLon only
            // creates the point, so it's added to Points explicitly.
            var marker = engine.CreatePointLatLon(DemoLatitude, DemoLongitude, label: "Duslo")
                .WithBrush(new SolidBrush(Color.Crimson))
                .WithShape(ActiveCanvas.PointShape.Star)
                .WithSize(10f);
            engine.Points.Add(marker);
            engine.Render();
        }

        /// <summary>Asks where the drawing's "up" really points (e.g. an AutoCAD background in local
        /// coordinates) - the engine then shows the north arrow accordingly and fits the map under the drawing.</summary>
        private void toolStripButtonNorth_Click(object sender, EventArgs e)
        {
            using var dlg = new Form
            {
                Text = "Drawing orientation",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = false,
                ClientSize = new Size(340, 120)
            };

            var label = new Label
            {
                Text = "Where the drawing's \"up\" really points (bearing in degrees):\n0 = north, 90 = east, 135 = south-east, 270 = west",
                Location = new Point(12, 12),
                AutoSize = true
            };
            var input = new NumericUpDown
            {
                Location = new Point(12, 60),
                Width = 100,
                Minimum = -360,
                Maximum = 360,
                DecimalPlaces = 1,
                Value = (decimal)(engine.WorldUpBearingDegrees % 360f)
            };
            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(174, 59), Size = new Size(75, 25) };
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(253, 59), Size = new Size(75, 25) };

            dlg.Controls.AddRange(new Control[] { label, input, ok, cancel });
            dlg.AcceptButton = ok;
            dlg.CancelButton = cancel;

            if (dlg.ShowDialog(this) == DialogResult.OK)
                engine.WorldUpBearingDegrees = (float)input.Value;
        }
    }
}
