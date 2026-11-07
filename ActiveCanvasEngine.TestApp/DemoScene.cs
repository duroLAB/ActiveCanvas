using System.Drawing;
using System.Drawing.Drawing2D;

namespace ActiveCanvasEngine.TestApp
{
    /// <summary>
    /// Demo scene for the test app - one example of every primitive the engine supports, in two groups:
    /// on the left basic geometry (points, polylines, polygons, icons), on the right a small diagram
    /// (shape markers, connectors, hatch fills). Call it e.g. from Form_Load:
    ///     DemoScene.Build(engine);
    /// </summary>
    public static class DemoScene
    {
        public static void Build(ActiveCanvas engine)
        {
            BuildGeometry(engine, offsetX: -24f);
            BuildDiagram(engine);
            engine.ZoomToFullExtent(); // 10% margin around everything
        }

        /// <summary>Points, polylines, polygons and icons - shifted left by offsetX so they don't overlap the diagram.</summary>
        private static void BuildGeometry(ActiveCanvas engine, float offsetX)
        {
            float x(float value) => value + offsetX;

            // ---- Points ----
            engine.AddPoint(x(0), 0, 0, "Basic point");
            
            engine.AddPoint(x(4), 2, 0, "Custom look",
                brush: new SolidBrush(Color.MediumPurple),
                pen: new Pen(Color.Black, 1.5f),
                size: 9f);

            // selectable (e.g. a click opens a dialog), but not draggable - a locked position
            engine.AddPoint(x(4), -2, 0, "Locked point", selectable: true, draggable: false);

            // purely decorative - can't be clicked, dragged or hovered
            var decorative = engine.CreatePoint(x(-3), 3, 0, "Decoration")
                .WithBrush(new SolidBrush(Color.LightGray))
                .WithSelectable(false)
                .WithHover(false);
            engine.Points.Add(decorative);

            engine.AddPoint(x(2), 6, 0, "Star",
                brush: new SolidBrush(Color.Gold), shape: ActiveCanvas.PointShape.Star, size: 10f);

            // ---- Polylines - their vertices are ordinary, draggable points ----
            engine.AddPolyline(
                new[] { (x(0), 0f, 0f), (x(3), 4f, 0f), (x(7), 3f, 0f), (x(9), 6f, 0f) },
                label: "Route A",
                linePen: new Pen(Color.ForestGreen, 3f));

            var start = engine.CreatePoint(x(-5), -2, 0, "Start").WithBrush(new SolidBrush(Color.OrangeRed));
            var end = engine.CreatePoint(x(-2), -4, 0, "End").WithBrush(new SolidBrush(Color.OrangeRed));
            engine.AddPolyline(new[] { start, end }, label: "Route B")
                .WithLabelPosition(x(-3.5f), -3f, 0f); // custom label position instead of the midpoint

            // ---- Polygons ----
            engine.AddPolygon(
                new[] { (x(10), 0f, 0f), (x(15), 0f, 0f), (x(15), 4f, 0f), (x(10), 4f, 0f) },
                label: "Zone A");

            // square vertices for a more technical look
            var r1 = engine.CreatePoint(x(10), -6, 0).WithShape(ActiveCanvas.PointShape.Square).WithSize(6f);
            var r2 = engine.CreatePoint(x(14), -6, 0).WithShape(ActiveCanvas.PointShape.Square).WithSize(6f);
            var r3 = engine.CreatePoint(x(12), -2, 0).WithShape(ActiveCanvas.PointShape.Square).WithSize(6f);
            engine.AddPolygon(new[] { r1, r2, r3 },
                label: "No entry",
                fillBrush: new SolidBrush(Color.FromArgb(70, Color.OrangeRed)),
                outlinePen: new Pen(Color.OrangeRed, 2f));

            // ---- Icons ----
            engine.AddImage(CreatePinIcon(Color.Crimson), x(6), 8, 0, "Warehouse", width: 28);   // fixed pixel size, like a map pin

            engine.AddImage(CreatePinIcon(Color.SteelBlue), x(-6), 6, 0, "Hall 1", width: 4f)    // real-world size, grows with zoom
                .WithScaleWithZoom(true);

            engine.AddImage(CreatePinIcon(Color.Goldenrod), x(0), 8, 0, "Substation", width: 24, draggable: false);
        }

        /// <summary>Shape markers with inner text, connectors and hatch fills - a small flowchart-style diagram.</summary>
        private static void BuildDiagram(ActiveCanvas engine)
        {
            var input = engine.AddShape(0, 0, 0, ActiveCanvas.MarkerShapeType.Rectangle, label: "Input",
                brush: new SolidBrush(Color.FromArgb(80, Color.Orange)), width: 2f);
            var output = engine.AddShape(6, 3, 0, ActiveCanvas.MarkerShapeType.Rectangle, label: "Output",
                brush: new SolidBrush(Color.FromArgb(80, Color.Orange)), width: 2f);

            // elbow-routed connector that re-picks its attachment sides while the shapes are dragged
            engine.AddConnector(input, output, showArrow: true, routing: ActiveCanvas.ConnectorRouting.Orthogonal)
                .WithAutoAnchors()
                .WithPen(new Pen(Color.SteelBlue, 3f))
                .WithLabel("flow");

            engine.AddShape(0, 6, 0, ActiveCanvas.MarkerShapeType.Circle,
                label: "Sensor", brush: new SolidBrush(Color.Crimson), width: 2f);

            // custom outline, normalized to -0.5..0.5
            engine.AddCustomShape(3, 6, 0,
                new[] { (0f, -0.5f), (0.35f, 0.1f), (0.15f, 0.1f), (0.15f, 0.5f),
                        (-0.15f, 0.5f), (-0.15f, 0.1f), (-0.35f, 0.1f) },
                label: "Direction",
                brush: new SolidBrush(Color.DodgerBlue),
                pen: new Pen(Color.Navy, 1.5f),
                width: 2);

            // rounded rectangle in real-world size with auto-fitting inner text
            engine.AddShape(10, 5, 0, ActiveCanvas.MarkerShapeType.RoundedRectangle,
                    label: "Zone B", brush: new SolidBrush(Color.FromArgb(80, Color.Orange)),
                    pen: new Pen(Color.OrangeRed, 2f), width: 4f, height: 3f)
                .WithScaleWithZoom(true)
                .WithCornerRadius(0.5f)
                .WithInnerText("p = 102 kPa\nT = 20 °C\nm = 25 kg/h");

            engine.AddShape(10, 0, 0, ActiveCanvas.MarkerShapeType.Rectangle,
                    brush: new SolidBrush(Color.LightSteelBlue), pen: new Pen(Color.Navy, 0.5f), width: 1f, height: 2f)
                .WithScaleWithZoom(true)
                .WithLabel("Warehouse A").WithLabelAnchor(ActiveCanvas.LabelAnchor.Bottom)
                .WithInnerText("Capacity: 500 pcs\nLast inventory: 03/2025")
                .WithInnerTextColor(Color.DarkBlue)
                .WithAutoScaleInnerText(true, minSize: 6f, maxSize: 20f);

            // hatch / dot pattern fills
            engine.AddShape(0, -4, 0, ActiveCanvas.MarkerShapeType.Rectangle, "Zone 0",
                brush: HatchBrushes.CreateDotPatternBrush(spacing: 12f, dotRadius: 2f),
                pen: new Pen(Color.Black, 1f), width: 2f, height: 2f);
            engine.AddShape(3, -4, 0, ActiveCanvas.MarkerShapeType.Rectangle, "Zone 1",
                brush: new HatchBrush(HatchStyle.DiagonalCross, Color.Black, Color.Transparent),
                pen: new Pen(Color.Black, 1f), width: 2f, height: 2f);
            engine.AddShape(6, -4, 0, ActiveCanvas.MarkerShapeType.Rectangle, "Zone 2",
                brush: new HatchBrush(HatchStyle.ForwardDiagonal, Color.Black, Color.Transparent),
                pen: new Pen(Color.Black, 1f), width: 2f, height: 2f);
        }

        /// <summary>Generates a simple "pin" icon (circle + point) in code, so the demo doesn't depend on an
        /// external image file.</summary>
        private static Bitmap CreatePinIcon(Color color)
        {
            var bmp = new Bitmap(32, 32);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var brush = new SolidBrush(color);
            using var outline = new Pen(Color.White, 2f);

            var points = new[]
            {
                new PointF(16, 30),
                new PointF(6, 14),
                new PointF(26, 14)
            };
            g.FillPolygon(brush, points);
            g.FillEllipse(brush, 4, 0, 24, 24);
            g.DrawEllipse(outline, 5, 1, 22, 22);

            return bmp;
        }
    }
}
