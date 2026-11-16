namespace ActiveCanvasEngine.TestApp
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            toolStripMain = new ToolStrip();
            btnClearAll = new ToolStripButton();
            ddBackgrounds = new ToolStripDropDownButton();
            btnMap = new ToolStripButton();
            btnOrientation = new ToolStripButton();
            sepMain1 = new ToolStripSeparator();
            btn3D = new ToolStripButton();
            ddViews = new ToolStripDropDownButton();
            miTopView = new ToolStripMenuItem();
            miFrontView = new ToolStripMenuItem();
            miSideView = new ToolStripMenuItem();
            miIsometricView = new ToolStripMenuItem();
            btnZoomToFit = new ToolStripButton();
            btnTheme = new ToolStripButton();
            splitMain = new SplitContainer();
            pictureBox1 = new PictureBox();
            toolStripDraw = new ToolStrip();
            btnSelect = new ToolStripButton();
            btnPoint = new ToolStripButton();
            btnPolyline = new ToolStripButton();
            btnPolygon = new ToolStripButton();
            btnIcon = new ToolStripButton();
            btnMeasure = new ToolStripButton();
            sepDraw1 = new ToolStripSeparator();
            btnDelete = new ToolStripButton();
            splitSide = new SplitContainer();
            propertyHost = new Panel();
            lblNoSelection = new Label();
            headerProperties = new SectionHeader();
            listEvents = new ListBox();
            toolStripLog = new ToolStrip();
            btnClearLog = new ToolStripButton();
            headerEvents = new SectionHeader();
            statusStrip1 = new StatusStrip();
            lblInfo = new ToolStripStatusLabel();
            lblCoords = new ToolStripStatusLabel();
            lblLatLon = new ToolStripStatusLabel();
            lblScale = new ToolStripStatusLabel();
            lblView = new ToolStripStatusLabel();
            toolStripMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            toolStripDraw.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitSide).BeginInit();
            splitSide.Panel1.SuspendLayout();
            splitSide.Panel2.SuspendLayout();
            splitSide.SuspendLayout();
            propertyHost.SuspendLayout();
            toolStripLog.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            //
            // toolStripMain
            //
            toolStripMain.GripStyle = ToolStripGripStyle.Hidden;
            toolStripMain.Items.AddRange(new ToolStripItem[] { btnClearAll, ddBackgrounds, btnMap, btnOrientation, sepMain1, btn3D, ddViews, btnZoomToFit, btnTheme });
            toolStripMain.Location = new Point(0, 0);
            toolStripMain.Name = "toolStripMain";
            toolStripMain.Padding = new Padding(6, 2, 6, 2);
            toolStripMain.Size = new Size(1264, 31);
            toolStripMain.TabIndex = 0;
            //
            // btnClearAll
            //
            btnClearAll.Name = "btnClearAll";
            btnClearAll.Size = new Size(57, 24);
            btnClearAll.Text = "Clear all";
            btnClearAll.ToolTipText = "Remove all items and background images (Clear, ClearAllBackgroundImages)";
            btnClearAll.Click += btnClearAll_Click;
            //
            // ddBackgrounds
            //
            ddBackgrounds.Name = "ddBackgrounds";
            ddBackgrounds.Size = new Size(91, 24);
            ddBackgrounds.Text = "Backgrounds";
            ddBackgrounds.ToolTipText = "Background images in the plan, front and side view";
            ddBackgrounds.DropDownOpening += ddBackgrounds_DropDownOpening;
            //
            // btnMap
            //
            btnMap.CheckOnClick = true;
            btnMap.Name = "btnMap";
            btnMap.Size = new Size(35, 24);
            btnMap.Text = "Map";
            btnMap.ToolTipText = "OpenStreetMap background (on/off)";
            btnMap.Click += btnMap_Click;
            //
            // btnOrientation
            //
            btnOrientation.Name = "btnOrientation";
            btnOrientation.Size = new Size(75, 24);
            btnOrientation.Text = "Orientation";
            btnOrientation.ToolTipText = "Drawing orientation - bearing of the drawing's up (WorldUpBearingDegrees)";
            btnOrientation.Click += btnOrientation_Click;
            //
            // sepMain1
            //
            sepMain1.Name = "sepMain1";
            sepMain1.Size = new Size(6, 27);
            //
            // btn3D
            //
            btn3D.Name = "btn3D";
            btn3D.Size = new Size(55, 24);
            btn3D.Text = "3D view";
            btn3D.ToolTipText = "Switch between the 2D top view and the 3D isometric view";
            btn3D.Click += btn3D_Click;
            //
            // ddViews
            //
            ddViews.DropDownItems.AddRange(new ToolStripItem[] { miTopView, miFrontView, miSideView, miIsometricView });
            ddViews.Name = "ddViews";
            ddViews.Size = new Size(50, 24);
            ddViews.Text = "View";
            ddViews.ToolTipText = "Standard views";
            //
            // miTopView
            //
            miTopView.Name = "miTopView";
            miTopView.Size = new Size(180, 22);
            miTopView.Text = "Top (plan, X-Y)";
            miTopView.Click += miTopView_Click;
            //
            // miFrontView
            //
            miFrontView.Name = "miFrontView";
            miFrontView.Size = new Size(180, 22);
            miFrontView.Text = "Front (X-Z)";
            miFrontView.Click += miFrontView_Click;
            //
            // miSideView
            //
            miSideView.Name = "miSideView";
            miSideView.Size = new Size(180, 22);
            miSideView.Text = "Side (Y-Z)";
            miSideView.Click += miSideView_Click;
            //
            // miIsometricView
            //
            miIsometricView.Name = "miIsometricView";
            miIsometricView.Size = new Size(180, 22);
            miIsometricView.Text = "Isometric (3D)";
            miIsometricView.Click += miIsometricView_Click;
            //
            // btnZoomToFit
            //
            btnZoomToFit.Name = "btnZoomToFit";
            btnZoomToFit.Size = new Size(77, 24);
            btnZoomToFit.Text = "Zoom to fit";
            btnZoomToFit.ToolTipText = "Zoom to fit everything (ZoomToFullExtent)";
            btnZoomToFit.Click += btnZoomToFit_Click;
            //
            // btnTheme
            //
            btnTheme.Alignment = ToolStripItemAlignment.Right;
            btnTheme.Name = "btnTheme";
            btnTheme.Size = new Size(69, 24);
            btnTheme.Text = "Dark mode";
            btnTheme.ToolTipText = "Switch between the light and dark theme";
            btnTheme.Click += btnTheme_Click;
            //
            // splitMain
            //
            splitMain.Dock = DockStyle.Fill;
            splitMain.FixedPanel = FixedPanel.Panel2;
            splitMain.Location = new Point(0, 31);
            splitMain.Name = "splitMain";
            //
            // splitMain.Panel1
            //
            splitMain.Panel1.Controls.Add(pictureBox1);
            splitMain.Panel1.Controls.Add(toolStripDraw);
            //
            // splitMain.Panel2
            //
            splitMain.Panel2.Controls.Add(splitSide);
            splitMain.Size = new Size(1264, 628);
            splitMain.SplitterDistance = 934;
            splitMain.SplitterWidth = 3;
            splitMain.TabIndex = 1;
            //
            // pictureBox1
            //
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Location = new Point(0, 31);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(934, 597);
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            //
            // toolStripDraw
            //
            toolStripDraw.GripStyle = ToolStripGripStyle.Hidden;
            toolStripDraw.Items.AddRange(new ToolStripItem[] { btnSelect, btnPoint, btnPolyline, btnPolygon, btnIcon, btnMeasure, sepDraw1, btnDelete });
            toolStripDraw.Location = new Point(0, 0);
            toolStripDraw.Name = "toolStripDraw";
            toolStripDraw.Padding = new Padding(6, 2, 6, 2);
            toolStripDraw.Size = new Size(934, 31);
            toolStripDraw.TabIndex = 0;
            //
            // btnSelect
            //
            btnSelect.Checked = true;
            btnSelect.CheckState = CheckState.Checked;
            btnSelect.Name = "btnSelect";
            btnSelect.Size = new Size(42, 24);
            btnSelect.Text = "Select";
            btnSelect.ToolTipText = "Select, drag and pan - cancels the current tool (Esc)";
            btnSelect.Click += btnSelect_Click;
            //
            // btnPoint
            //
            btnPoint.Name = "btnPoint";
            btnPoint.Size = new Size(39, 24);
            btnPoint.Text = "Point";
            btnPoint.ToolTipText = "Place a point (StartPlacingPoints)";
            btnPoint.Click += btnPoint_Click;
            //
            // btnPolyline
            //
            btnPolyline.Name = "btnPolyline";
            btnPolyline.Size = new Size(55, 24);
            btnPolyline.Text = "Polyline";
            btnPolyline.ToolTipText = "Draw a polyline (StartDrawingPolyline) - double-click or Enter to finish";
            btnPolyline.Click += btnPolyline_Click;
            //
            // btnPolygon
            //
            btnPolygon.Name = "btnPolygon";
            btnPolygon.Size = new Size(57, 24);
            btnPolygon.Text = "Polygon";
            btnPolygon.ToolTipText = "Draw a polygon (StartDrawingPolygon) - double-click or Enter to finish";
            btnPolygon.Click += btnPolygon_Click;
            //
            // btnIcon
            //
            btnIcon.Name = "btnIcon";
            btnIcon.Size = new Size(34, 24);
            btnIcon.Text = "Icon";
            btnIcon.ToolTipText = "Place an icon (StartPlacingImages)";
            btnIcon.Click += btnIcon_Click;
            //
            // btnMeasure
            //
            btnMeasure.Name = "btnMeasure";
            btnMeasure.Size = new Size(57, 24);
            btnMeasure.Text = "Measure";
            btnMeasure.ToolTipText = "Measure a distance between two points (StartMeasuring)";
            btnMeasure.Click += btnMeasure_Click;
            //
            // sepDraw1
            //
            sepDraw1.Name = "sepDraw1";
            sepDraw1.Size = new Size(6, 27);
            //
            // btnDelete
            //
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(44, 24);
            btnDelete.Text = "Delete";
            btnDelete.ToolTipText = "Remove the selected item (RemoveSelected, Delete key)";
            btnDelete.Click += btnDelete_Click;
            //
            // splitSide
            //
            splitSide.Dock = DockStyle.Fill;
            splitSide.Location = new Point(0, 0);
            splitSide.Name = "splitSide";
            splitSide.Orientation = Orientation.Horizontal;
            //
            // splitSide.Panel1
            //
            splitSide.Panel1.Controls.Add(propertyHost);
            splitSide.Panel1.Controls.Add(headerProperties);
            //
            // splitSide.Panel2
            //
            splitSide.Panel2.Controls.Add(listEvents);
            splitSide.Panel2.Controls.Add(toolStripLog);
            splitSide.Panel2.Controls.Add(headerEvents);
            splitSide.Size = new Size(327, 628);
            splitSide.SplitterDistance = 380;
            splitSide.SplitterWidth = 3;
            splitSide.TabIndex = 0;
            //
            // propertyHost
            //
            propertyHost.Controls.Add(lblNoSelection);
            propertyHost.Dock = DockStyle.Fill;
            propertyHost.Location = new Point(0, 28);
            propertyHost.Name = "propertyHost";
            propertyHost.Size = new Size(327, 352);
            propertyHost.TabIndex = 1;
            //
            // lblNoSelection
            //
            lblNoSelection.Dock = DockStyle.Fill;
            lblNoSelection.Location = new Point(0, 0);
            lblNoSelection.Name = "lblNoSelection";
            lblNoSelection.Size = new Size(327, 352);
            lblNoSelection.TabIndex = 0;
            lblNoSelection.Text = "Click an item on the canvas to see and edit its properties.";
            lblNoSelection.TextAlign = ContentAlignment.MiddleCenter;
            //
            // headerProperties
            //
            headerProperties.Location = new Point(0, 0);
            headerProperties.Name = "headerProperties";
            headerProperties.Size = new Size(327, 28);
            headerProperties.TabIndex = 0;
            headerProperties.Text = "PROPERTIES";
            //
            // listEvents
            //
            listEvents.BorderStyle = BorderStyle.None;
            listEvents.Dock = DockStyle.Fill;
            listEvents.HorizontalScrollbar = true;
            listEvents.IntegralHeight = false;
            listEvents.Location = new Point(0, 55);
            listEvents.Name = "listEvents";
            listEvents.Size = new Size(327, 190);
            listEvents.TabIndex = 2;
            //
            // toolStripLog
            //
            toolStripLog.GripStyle = ToolStripGripStyle.Hidden;
            toolStripLog.Items.AddRange(new ToolStripItem[] { btnClearLog });
            toolStripLog.Location = new Point(0, 28);
            toolStripLog.Name = "toolStripLog";
            toolStripLog.Size = new Size(327, 27);
            toolStripLog.TabIndex = 1;
            //
            // btnClearLog
            //
            btnClearLog.Name = "btnClearLog";
            btnClearLog.Size = new Size(38, 24);
            btnClearLog.Text = "Clear";
            btnClearLog.ToolTipText = "Clear the event log";
            btnClearLog.Click += btnClearLog_Click;
            //
            // headerEvents
            //
            headerEvents.Location = new Point(0, 0);
            headerEvents.Name = "headerEvents";
            headerEvents.Size = new Size(327, 28);
            headerEvents.TabIndex = 0;
            headerEvents.Text = "EVENT LOG";
            //
            // statusStrip1
            //
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblInfo, lblCoords, lblLatLon, lblScale, lblView });
            statusStrip1.Location = new Point(0, 659);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1264, 22);
            statusStrip1.TabIndex = 2;
            //
            // lblInfo
            //
            lblInfo.Name = "lblInfo";
            lblInfo.Spring = true;
            lblInfo.TextAlign = ContentAlignment.MiddleLeft;
            //
            // lblCoords
            //
            lblCoords.BorderSides = ToolStripStatusLabelBorderSides.Left;
            lblCoords.Name = "lblCoords";
            lblCoords.ToolTipText = "Raw coordinates on the ground (Z = 0) under the cursor (ScreenToGround)";
            //
            // lblLatLon
            //
            lblLatLon.BorderSides = ToolStripStatusLabelBorderSides.Left;
            lblLatLon.Name = "lblLatLon";
            lblLatLon.ToolTipText = "GPS position under the cursor (ScreenToLatLon) - while the map is on";
            lblLatLon.Visible = false;
            //
            // lblScale
            //
            lblScale.BorderSides = ToolStripStatusLabelBorderSides.Left;
            lblScale.Name = "lblScale";
            lblScale.ToolTipText = "Zoom (PixelsPerMeter, or the OSM zoom level while the map is on)";
            //
            // lblView
            //
            lblView.BorderSides = ToolStripStatusLabelBorderSides.Left;
            lblView.Name = "lblView";
            lblView.ToolTipText = "Current view (YawDegrees / PitchDegrees)";
            //
            // MainForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1264, 681);
            Controls.Add(splitMain);
            Controls.Add(statusStrip1);
            Controls.Add(toolStripMain);
            KeyPreview = true;
            MinimumSize = new Size(800, 500);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ActiveCanvas Test App";
            Load += MainForm_Load;
            toolStripMain.ResumeLayout(false);
            toolStripMain.PerformLayout();
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel1.PerformLayout();
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            toolStripDraw.ResumeLayout(false);
            toolStripDraw.PerformLayout();
            splitSide.Panel1.ResumeLayout(false);
            splitSide.Panel2.ResumeLayout(false);
            splitSide.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitSide).EndInit();
            splitSide.ResumeLayout(false);
            propertyHost.ResumeLayout(false);
            toolStripLog.ResumeLayout(false);
            toolStripLog.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStrip toolStripMain;
        private ToolStripButton btnClearAll;
        private ToolStripDropDownButton ddBackgrounds;
        private ToolStripButton btnMap;
        private ToolStripButton btnOrientation;
        private ToolStripSeparator sepMain1;
        private ToolStripButton btn3D;
        private ToolStripDropDownButton ddViews;
        private ToolStripMenuItem miTopView;
        private ToolStripMenuItem miFrontView;
        private ToolStripMenuItem miSideView;
        private ToolStripMenuItem miIsometricView;
        private ToolStripButton btnZoomToFit;
        private ToolStripButton btnTheme;
        private SplitContainer splitMain;
        private PictureBox pictureBox1;
        private ToolStrip toolStripDraw;
        private ToolStripButton btnSelect;
        private ToolStripButton btnPoint;
        private ToolStripButton btnPolyline;
        private ToolStripButton btnPolygon;
        private ToolStripButton btnIcon;
        private ToolStripButton btnMeasure;
        private ToolStripSeparator sepDraw1;
        private ToolStripButton btnDelete;
        private SplitContainer splitSide;
        private Panel propertyHost;
        private Label lblNoSelection;
        private SectionHeader headerProperties;
        private ListBox listEvents;
        private ToolStrip toolStripLog;
        private ToolStripButton btnClearLog;
        private SectionHeader headerEvents;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lblInfo;
        private ToolStripStatusLabel lblCoords;
        private ToolStripStatusLabel lblLatLon;
        private ToolStripStatusLabel lblScale;
        private ToolStripStatusLabel lblView;
    }
}
