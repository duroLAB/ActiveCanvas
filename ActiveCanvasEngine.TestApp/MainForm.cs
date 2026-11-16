using System.Globalization;

namespace ActiveCanvasEngine.TestApp
{
    /// <summary>
    /// Test / demo application for the ActiveCanvas engine. Top bar: scene, backgrounds, map and view;
    /// bar above the canvas: drawing tools; right panel: properties of the selected item and a log of the
    /// engine's events; status bar: coordinates under the cursor, zoom and view.
    /// Keys: Esc = cancel the current tool, Enter = finish drawing, Delete = remove the selected item.
    /// </summary>
    public partial class MainForm : Form
    {
        private enum Tool { Select, Point, Polyline, Polygon, Icon, Measure }

        private const int MaxLogLines = 500;

        private readonly ActiveCanvas engine;
        private readonly System.Windows.Forms.Timer _statusTimer = new() { Interval = 250 };
        private UiPalette _palette = UiPalette.Light;
        private Tool _tool = Tool.Select;
        private PropertyGrid? _propertyGrid;
        private object? _propertyItem;

        // OSM demo location - the drawing origin (0,0) is anchored here when the map is turned on
        private const double DemoLatitude = 48.172969;   // Duslo, Sala
        private const double DemoLongitude = 17.940856;
        private bool _geoDemoAdded;

        public MainForm()
        {
            InitializeComponent();
            engine = new ActiveCanvas(pictureBox1);
        }

        private void MainForm_Load(object? sender, EventArgs e)
        {
            splitMain.Panel2MinSize = 220;
            splitMain.SplitterDistance = Math.Max(200, splitMain.Width - LogicalToDeviceUnits(330));

            ApplyTheme(dark: false);
            DemoScene.Build(engine);

            pictureBox1.MouseMove += PictureBox_MouseMove;
            pictureBox1.MouseLeave += (s, a) => { lblCoords.Text = "X –   Y –"; lblLatLon.Text = ""; };
            pictureBox1.MouseUp += (s, a) => RefreshToolState();

            KeyDown += MainForm_KeyDown;
            HookEngineEvents();

            _statusTimer.Tick += (s, a) => UpdateViewStatus();
            _statusTimer.Start();
            FormClosed += (s, a) => { _statusTimer.Dispose(); engine.Dispose(); };

            lblCoords.Text = "X –   Y –";
            UpdateViewStatus();
            ShowProperties(null);
            ShowStatus("Ready - pick a tool above the canvas, or click an item to see its properties");
        }

        private void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (splitMain.Panel2.ContainsFocus)
                return;   // keys typed into the property grid belong to it

            switch (e.KeyCode)
            {
                case Keys.Escape: SetTool(Tool.Select); break;
                case Keys.Enter: engine.FinishDrawing(); RefreshToolState(); break;
                case Keys.Delete when !engine.IsDrawing: engine.RemoveSelected(); break;
            }
        }

        // ---- Theme and icons ----

        private void ApplyTheme(bool dark)
        {
            _palette = dark ? UiPalette.Dark : UiPalette.Light;
            ModernUi.Apply(this, _palette);
            if (dark) engine.ApplyDarkTheme(); else engine.ApplyLightTheme();

            lblNoSelection.ForeColor = _palette.SubtleText;
            btnTheme.Text = dark ? "Light mode" : "Dark mode";
            ApplyIcons();
        }

        private void ApplyIcons()
        {
            Color c = _palette.Icon;
            void Set(ToolStripItem item, string glyph)
            {
                GlyphIcons.Apply(item, glyph, c, this);
                item.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            }

            Set(btnClearAll, Glyph.ClearAll);
            Set(ddBackgrounds, Glyph.Backgrounds);
            Set(btnMap, Glyph.Map);
            Set(btnOrientation, Glyph.Orientation);
            Set(btn3D, Glyph.View3D);
            Set(ddViews, Glyph.Views);
            Set(btnZoomToFit, Glyph.ZoomToFit);
            Set(btnTheme, _palette == UiPalette.Dark ? Glyph.LightMode : Glyph.DarkMode);

            Set(btnSelect, Glyph.Select);
            Set(btnPoint, Glyph.Point);
            Set(btnPolyline, Glyph.Polyline);
            Set(btnPolygon, Glyph.Polygon);
            Set(btnIcon, Glyph.Icon);
            Set(btnMeasure, Glyph.Measure);
            Set(btnDelete, Glyph.Delete);
            Set(btnClearLog, Glyph.ClearLog);
        }

        private void btnTheme_Click(object? sender, EventArgs e) => ApplyTheme(dark: _palette != UiPalette.Dark);

        // ---- Drawing tools ----

        /// <summary>Starts a tool (cancelling the previous one) and marks its button.</summary>
        private void SetTool(Tool tool)
        {
            engine.CancelDrawing();
            _tool = tool;

            switch (tool)
            {
                case Tool.Point:
                    engine.StartPlacingPoints(label: "New point", brush: new SolidBrush(Color.Crimson),
                        shape: ActiveCanvas.PointShape.Diamond, size: 8f, continuous: true);
                    ShowStatus("Click to place points, Esc to stop");
                    break;

                case Tool.Polyline:
                    engine.StartDrawingPolyline("New route", new Pen(Color.ForestGreen, 3f));
                    ShowStatus("Click to add vertices, double-click or Enter to finish, Esc to cancel");
                    break;

                case Tool.Polygon:
                    engine.StartDrawingPolygon("New area", new SolidBrush(Color.FromArgb(128, Color.LightBlue)), new Pen(Color.DarkBlue, 2f));
                    ShowStatus("Click to add vertices, double-click or Enter to finish, Esc to cancel");
                    break;

                case Tool.Icon:
                    if (!StartPlacingIcon()) _tool = Tool.Select;
                    break;

                case Tool.Measure:
                    engine.StartMeasuring();
                    ShowStatus("Click two points to measure the distance");
                    break;
            }

            UpdateToolButtons();
        }

        /// <summary>Goes back to Select when the engine has finished the current tool on its own
        /// (polyline finished, second measuring point placed, ...).</summary>
        private void RefreshToolState()
        {
            bool busy = engine.IsDrawing || engine.IsPlacingPoints || engine.IsPlacingImages || engine.IsMeasuring;
            if (_tool != Tool.Select && !busy)
            {
                _tool = Tool.Select;
                UpdateToolButtons();
            }
        }

        private void UpdateToolButtons()
        {
            btnSelect.Checked = _tool == Tool.Select;
            btnPoint.Checked = _tool == Tool.Point;
            btnPolyline.Checked = _tool == Tool.Polyline;
            btnPolygon.Checked = _tool == Tool.Polygon;
            btnIcon.Checked = _tool == Tool.Icon;
            btnMeasure.Checked = _tool == Tool.Measure;
        }

        private bool StartPlacingIcon()
        {
            string iconsPath = Path.Combine(AppContext.BaseDirectory, "testIcons");
            using var dlg = new IconPicker.IconPickerDialog(iconsPath)
            {
                ThumbnailSize = 32,
                IconsPerRow = 60, // affects only the initial window width
                AllowedExtensions = new[] { ".png", ".bmp", ".ico" }
            };
            ModernUi.Apply(dlg, _palette);

            if (dlg.ShowDialog(this) != DialogResult.OK) return false;

            // LoadImageFile reads the file fully, so it isn't kept locked
            var icon = ActiveCanvas.LoadImageFile(dlg.SelectedFilePath, maxPixelDimension: 256);
            engine.StartPlacingImages(icon, label: Path.GetFileNameWithoutExtension(dlg.SelectedFilePath),
                width: 32, continuous: true);
            ShowStatus("Click to place the icon, Esc to stop");
            return true;
        }

        private void btnSelect_Click(object? sender, EventArgs e) => SetTool(Tool.Select);
        private void btnPoint_Click(object? sender, EventArgs e) => SetTool(Tool.Point);
        private void btnPolyline_Click(object? sender, EventArgs e) => SetTool(Tool.Polyline);
        private void btnPolygon_Click(object? sender, EventArgs e) => SetTool(Tool.Polygon);
        private void btnIcon_Click(object? sender, EventArgs e) => SetTool(Tool.Icon);
        private void btnMeasure_Click(object? sender, EventArgs e) => SetTool(Tool.Measure);

        private void btnDelete_Click(object? sender, EventArgs e)
        {
            if (!engine.RemoveSelected())
                ShowStatus("Nothing selected to remove");
        }

        private void btnClearAll_Click(object? sender, EventArgs e)
        {
            SetTool(Tool.Select);
            engine.Clear();
            engine.ClearAllBackgroundImages();
            _geoDemoAdded = false; // re-added the next time the map is turned on
            ShowProperties(null);
            ShowStatus("Everything cleared");
        }

        // ---- Properties panel and event log ----

        /// <summary>Shows the item's PropertyGrid (CreatePropertyGrid) in the right panel, or a hint when nothing is selected.</summary>
        private void ShowProperties(object? item)
        {
            if (ReferenceEquals(item, _propertyItem) && (item == null || _propertyGrid != null)) return;

            if (_propertyGrid != null)
            {
                propertyHost.Controls.Remove(_propertyGrid);
                _propertyGrid.Dispose();
                _propertyGrid = null;
            }

            _propertyItem = item;
            _propertyGrid = item == null ? null : engine.CreatePropertyGrid(item);
            if (_propertyGrid != null)
            {
                _propertyGrid.HelpVisible = true;
                ModernUi.Apply(_propertyGrid, _palette);
                propertyHost.Controls.Add(_propertyGrid);
                _propertyGrid.BringToFront();
            }
            lblNoSelection.Visible = _propertyGrid == null;
            headerProperties.Text = item == null ? "Properties" : "Properties - " + item.GetType().Name;
        }

        private void HookEngineEvents()
        {
            engine.SelectionChanged += (s, item) => { ShowProperties(item); Log("SelectionChanged", Describe(item)); };

            engine.PointClicked += (s, p) => Log("PointClicked", Describe(p));
            engine.PolylineClicked += (s, pl) => Log("PolylineClicked", Describe(pl));
            engine.PolygonClicked += (s, pg) => Log("PolygonClicked", Describe(pg));
            engine.ImageClicked += (s, im) => Log("ImageClicked", Describe(im));
            engine.ShapeClicked += (s, sh) => Log("ShapeClicked", Describe(sh));
            engine.ConnectorClicked += (s, c) => Log("ConnectorClicked", Describe(c));

            // Double-click opens the properties in a dialog - for points with a few rows hidden, to show hiddenProperties.
            engine.PointDoubleClicked += (s, p) => { Log("PointDoubleClicked", Describe(p)); ShowPropertyDialog(p, new[] { "Id", "HoverEnabled", "Draggable" }); };
            engine.PolylineDoubleClicked += (s, pl) => { Log("PolylineDoubleClicked", Describe(pl)); ShowPropertyDialog(pl); };
            engine.PolygonDoubleClicked += (s, pg) => { Log("PolygonDoubleClicked", Describe(pg)); ShowPropertyDialog(pg); };
            engine.ImageDoubleClicked += (s, im) => { Log("ImageDoubleClicked", Describe(im)); ShowPropertyDialog(im); };
            engine.ShapeDoubleClicked += (s, sh) => { Log("ShapeDoubleClicked", Describe(sh)); ShowPropertyDialog(sh); };
            engine.ConnectorDoubleClicked += (s, c) => { Log("ConnectorDoubleClicked", Describe(c)); ShowPropertyDialog(c); };

            engine.PointDragCompleted += (s, p) =>
            {
                Log("PointDragCompleted", Describe(p) + " " + FormatRaw(engine.ToRaw(p.World)));
                _propertyGrid?.Refresh();
            };

            engine.DrawingStarted += (s, a) => Log("DrawingStarted", "");
            engine.DrawingVertexAdded += (s, p) => Log("DrawingVertexAdded", FormatRaw(engine.ToRaw(p.World)));
            engine.DrawingCancelled += (s, a) => { Log("DrawingCancelled", ""); RefreshToolState(); };
            engine.PolylineDrawingFinished += (s, pl) =>
            {
                Log("PolylineDrawingFinished", $"{Describe(pl)}, {pl.Vertices.Count} vertices");
                ShowStatus($"Polyline finished ({pl.Vertices.Count} vertices)");
                BeginInvoke(RefreshToolState);
            };
            engine.PolygonDrawingFinished += (s, pg) =>
            {
                Log("PolygonDrawingFinished", $"{Describe(pg)}, {pg.Vertices.Count} vertices");
                ShowStatus($"Polygon finished ({pg.Vertices.Count} vertices)");
                BeginInvoke(RefreshToolState);
            };
            engine.PointPlaced += (s, p) => Log("PointPlaced", FormatRaw(engine.ToRaw(p.World)));
            engine.ImagePlaced += (s, im) => Log("ImagePlaced", Describe(im));
            engine.MeasurementCompleted += (s, meters) =>
            {
                string text = string.Format(CultureInfo.InvariantCulture, "{0:F2} m", meters);
                Log("MeasurementCompleted", text);
                ShowStatus("Distance: " + text);
                BeginInvoke(RefreshToolState);
            };
            engine.ItemRemoved += (s, item) =>
            {
                Log("ItemRemoved", Describe(item));
                if (ReferenceEquals(item, _propertyItem)) ShowProperties(engine.SelectedShape);
            };
        }

        private void Log(string eventName, string detail)
        {
            string line = $"{DateTime.Now:HH:mm:ss}  {eventName}" + (detail.Length > 0 ? "  " + detail : "");
            listEvents.BeginUpdate();
            listEvents.Items.Add(line);
            while (listEvents.Items.Count > MaxLogLines)
                listEvents.Items.RemoveAt(0);
            listEvents.TopIndex = listEvents.Items.Count - 1;
            listEvents.EndUpdate();
        }

        private void btnClearLog_Click(object? sender, EventArgs e) => listEvents.Items.Clear();

        /// <summary>Type and label of an item, e.g. Polygon "Field".</summary>
        private static string Describe(object? item)
        {
            if (item == null) return "(none)";
            string? label = item.GetType().GetProperty("Label")?.GetValue(item) as string;
            return string.IsNullOrEmpty(label) ? item.GetType().Name : $"{item.GetType().Name} \"{label}\"";
        }

        private static string FormatRaw(System.Numerics.Vector3 raw) =>
            string.Format(CultureInfo.InvariantCulture, "X {0:F2}  Y {1:F2}  Z {2:F2}", raw.X, raw.Y, raw.Z);

        private void ShowStatus(string message) => lblInfo.Text = message;

        private void ShowPropertyDialog(object item, string[]? hiddenProperties = null)
        {
            var grid = engine.CreatePropertyGrid(item, hiddenProperties);
            if (grid == null) return; // unknown item type

            using var dlg = new Form
            {
                Text = "Properties",
                Width = 320,
                Height = 420,
                StartPosition = FormStartPosition.CenterParent,
                ShowInTaskbar = false
            };
            dlg.Controls.Add(grid); // the grid is Dock = Fill
            ModernUi.Apply(dlg, _palette);
            dlg.ShowDialog(this);
            _propertyGrid?.Refresh();
        }

        // ---- Status bar ----

        private void PictureBox_MouseMove(object? sender, MouseEventArgs e)
        {
            var ground = engine.ScreenToGround(e.Location);
            lblCoords.Text = ground == null ? "X –   Y –" : string.Format(CultureInfo.InvariantCulture,
                "X {0:F2}   Y {1:F2}", ground.Value.X, ground.Value.Y);

            var geo = engine.IsOpenStreetMapEnabled ? engine.ScreenToLatLon(e.Location) : null;
            lblLatLon.Text = geo == null ? "" : string.Format(CultureInfo.InvariantCulture,
                "Lat {0:F6}   Lon {1:F6}", geo.Value.Latitude, geo.Value.Longitude);
        }

        /// <summary>Zoom and view in the status bar - polled, since the wheel and rotation are handled inside the engine.</summary>
        private void UpdateViewStatus()
        {
            lblLatLon.Visible = engine.IsOpenStreetMapEnabled;
            lblScale.Text = engine.IsOpenStreetMapEnabled
                ? $"OSM zoom {engine.OsmCurrentZoom}"
                : string.Format(CultureInfo.InvariantCulture, "{0:0.###} px/m", engine.PixelsPerMeter);

            float yaw = engine.YawDegrees, pitch = engine.PitchDegrees;
            bool Near(float a, float b) => Math.Abs(a - b) < 0.5f;
            string view = Near(pitch, 90f) ? "Top view (2D)"
                : Near(pitch, 0f) && Near(yaw, 0f) ? "Front view"
                : Near(pitch, 0f) && Near(yaw, 90f) ? "Side view"
                : string.Format(CultureInfo.InvariantCulture, "3D  yaw {0:F0}°  pitch {1:F0}°", yaw, pitch);
            lblView.Text = view;
            btn3D.Checked = !Near(pitch, 90f);
        }

        // ---- View ----

        private void btn3D_Click(object? sender, EventArgs e)
        {
            bool isTopView = engine.PitchDegrees >= 89f && engine.PitchDegrees < 91f;
            if (isTopView) ShowView(engine.ShowIsometricView, freeRotate: true);
            else ShowView(engine.ShowTopView, freeRotate: false);
        }

        private void miTopView_Click(object? sender, EventArgs e) => ShowView(engine.ShowTopView, freeRotate: false);
        private void miFrontView_Click(object? sender, EventArgs e) => ShowView(engine.ShowFrontView, freeRotate: false);
        private void miSideView_Click(object? sender, EventArgs e) => ShowView(engine.ShowSideView, freeRotate: false);
        private void miIsometricView_Click(object? sender, EventArgs e) => ShowView(engine.ShowIsometricView, freeRotate: true);

        private void ShowView(Action show, bool freeRotate)
        {
            show();
            engine.AllowFreeRotate = freeRotate;
            engine.ZoomToFullExtent();
            UpdateViewStatus();
        }

        private void btnZoomToFit_Click(object? sender, EventArgs e)
        {
            engine.ZoomToFullExtent(); // 10% margin; ZoomToFullExtent(0f) fits edge to edge
        }

        // ---- Background images ----

        /// <summary>Background images: open one per plane (plan / front / side), show/hide, opacity, remove.</summary>
        private void ddBackgrounds_DropDownOpening(object? sender, EventArgs e)
        {
            var items = ddBackgrounds.DropDownItems;
            items.Clear();

            Bitmap? openIcon = GlyphIcons.Get(Glyph.Image, _palette.Icon, this, 16);
            items.Add("Open plan view (X-Y)...", openIcon, async (s, a) => await OpenBackgroundAsync(ActiveCanvas.BackgroundPlane.Ground));
            items.Add("Open front view (X-Z)...", openIcon, async (s, a) => await OpenBackgroundAsync(ActiveCanvas.BackgroundPlane.Front));
            items.Add("Open side view (Y-Z)...", openIcon, async (s, a) => await OpenBackgroundAsync(ActiveCanvas.BackgroundPlane.Side));

            var planes = new[]
            {
                (Plane: ActiveCanvas.BackgroundPlane.Ground, Name: "plan view"),
                (Plane: ActiveCanvas.BackgroundPlane.Front, Name: "front view"),
                (Plane: ActiveCanvas.BackgroundPlane.Side, Name: "side view")
            };

            foreach (var (plane, name) in planes)
            {
                bool hasImage = engine.HasBackgroundImage(plane);
                items.Add(new ToolStripSeparator());

                var visible = new ToolStripMenuItem($"Show {name}")
                {
                    Enabled = hasImage,
                    Checked = engine.IsBackgroundVisible(plane)
                };
                visible.Click += (s, a) => engine.SetBackgroundVisible(plane, !engine.IsBackgroundVisible(plane));
                items.Add(visible);

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
                items.Add(opacity);

                var remove = new ToolStripMenuItem($"Remove {name}") { Enabled = hasImage };
                remove.Click += (s, a) => engine.ClearBackgroundImage(plane);
                items.Add(remove);
            }
        }

        private async Task OpenBackgroundAsync(ActiveCanvas.BackgroundPlane plane)
        {
            // A background image is plain local meters / S-JTSK here, not Web Mercator - and it would be
            // hidden under the map anyway (OsmHidesBackgroundImage) - so opening one switches the map off.
            if (btnMap.Checked)
            {
                btnMap.Checked = false;
                SetMapEnabled(false);
            }

            await BackgroundImageOpener.ShowAsync(this, engine, plane);
            if (plane == ActiveCanvas.BackgroundPlane.Ground || !engine.HasBackgroundImage(plane)) return;

            // An elevation is only seen face-on in its own view - switch there so it's visible right away.
            ShowView(plane == ActiveCanvas.BackgroundPlane.Front ? engine.ShowFrontView : engine.ShowSideView, freeRotate: false);
        }

        // ---- OpenStreetMap and drawing orientation ----

        private void btnMap_Click(object? sender, EventArgs e) => SetMapEnabled(btnMap.Checked);

        /// <summary>
        /// Everything in this test app is drawn in local "drawing" meters (like a CAD drawing). The geo
        /// anchor places that drawing on the map: drawing 0,0 = the demo location, rotated by the bearing
        /// set with the Orientation button. The world coordinates never change - the map is fitted under them.
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
                btn3D.Enabled = ddViews.Enabled = !engine.IsNorthUpLocked;
            }
            else
            {
                engine.DisableOpenStreetMap();   // the background image (if any) shows again, the view turns back
                btn3D.Enabled = ddViews.Enabled = true;
                lblLatLon.Text = "";
                engine.ZoomToFullExtent();
            }
            UpdateViewStatus();
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
        private void btnOrientation_Click(object? sender, EventArgs e)
        {
            using var dlg = new Form
            {
                Text = "Drawing orientation",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = false,
                ShowInTaskbar = false,
                AutoScaleMode = AutoScaleMode.Font,
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
            ModernUi.Apply(dlg, _palette);

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                engine.WorldUpBearingDegrees = (float)input.Value;
                Log("WorldUpBearingDegrees", input.Value.ToString(CultureInfo.InvariantCulture) + "°");
            }
        }
    }
}
