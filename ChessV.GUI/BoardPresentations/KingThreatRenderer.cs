using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using ChessV.Games;

namespace ChessV.GUI
{
  public sealed class KingThreatRenderer
  {
    private readonly KingThreatSnapshot snapshot;
    private readonly Dictionary<int, int> identifiers;
    private readonly Dictionary<int, KingDestinationHint[]> destinations;
    public IReadOnlyDictionary<int, Color> AttackerColors { get; }

    private static readonly Color[] palette =
    {
      Color.FromArgb(0, 114, 178), Color.FromArgb(204, 121, 167),
      Color.FromArgb(230, 159, 0), Color.FromArgb(86, 180, 233),
      Color.FromArgb(148, 103, 189), Color.FromArgb(0, 160, 170),
      Color.FromArgb(140, 86, 75), Color.FromArgb(227, 119, 194)
    };

    public KingThreatRenderer(KingThreatSnapshot snapshot)
    {
      this.snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
      destinations = snapshot.Destinations.GroupBy(h => h.Square)
        .ToDictionary(g => g.Key, g => g.ToArray());
      int[] sources = snapshot.Destinations.SelectMany(h => h.Threats)
        .Select(t => t.SourceSquare).Distinct().OrderBy(s => s).ToArray();
      identifiers = new Dictionary<int, int>();
      var colors = new Dictionary<int, Color>();
      for (int index = 0; index < sources.Length; index++)
      {
        Color color = index < palette.Length ? palette[index] :
          Color.FromArgb(50 + index * 47 % 130, 50 + index * 83 % 130, 190 + index * 17 % 60);
        while (colors.Values.Contains(color))
          color = Color.FromArgb(color.R, color.G, color.B == 255 ? 189 : color.B + 1);
        colors.Add(sources[index], color);
        identifiers.Add(sources[index], index + 1);
      }
      AttackerColors = new ReadOnlyDictionary<int, Color>(colors);
    }

    public void Render(Graphics graphics, Board board, Func<int, Rectangle> map, int? hoveredSquare)
    {
      GraphicsState state = graphics.Save();
      try
      {
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        foreach (var attacker in AttackerColors)
        {
          Rectangle rectangle = map(attacker.Key);
          DrawBorder(graphics, Inset(rectangle, 3), attacker.Value);
          DrawLabel(graphics, rectangle, identifiers[attacker.Key].ToString(), attacker.Value);
        }
        foreach (var destination in destinations)
        {
          Rectangle rectangle = map(destination.Key);
          int[] sources = destination.Value.SelectMany(h => h.Threats)
            .Select(t => t.SourceSquare).Distinct().OrderBy(s => s).ToArray();
          for (int index = 0; index < sources.Length; index++)
          {
            PointF[] segment = BorderSegment(Inset(rectangle, 6), index, sources.Length);
            using var halo = new Pen(Color.FromArgb(225, Color.White), 5);
            using var pen = new Pen(AttackerColors[sources[index]], 2.5f);
            graphics.DrawLines(halo, segment);
            graphics.DrawLines(pen, segment);
          }
          if (sources.Length > 0)
            DrawLabel(graphics, rectangle,
              string.Join(" ", sources.Select(s => identifiers[s])), Color.FromArgb(35, 35, 35));
          if (destination.Value.Any(h => h.FriendlyOccupied))
          {
            using var pen = new Pen(Color.Maroon, 3) { DashStyle = DashStyle.Dash };
            graphics.DrawRectangle(pen, Inset(rectangle, 10));
          }
          if (destination.Value.Any(h => !h.IsComplete))
            DrawLabel(graphics, rectangle, "?", Color.Firebrick);
        }
        if (snapshot.CurrentSquare.IsSafe)
          DrawBorder(graphics, Inset(map(snapshot.KingSquare), 2), Color.ForestGreen);

        if (hoveredSquare.HasValue && destinations.TryGetValue(hoveredSquare.Value, out var hints))
        {
          var paths = hints.SelectMany(h => h.Threats.Select(t => (Threat: t, Castle: h.IsCastling)))
            .Where(p => p.Threat.Path != null)
            .GroupBy(p => p.Threat.SourceSquare + ":" + string.Join(",", p.Threat.Path.Squares))
            .Select(g => g.OrderByDescending(p => p.Castle).First())
            .OrderBy(p => p.Threat.SourceSquare).ToArray();
          for (int index = 0; index < paths.Length; index++)
            DrawPath(graphics, board, map, paths[index].Threat, paths[index].Castle, index, paths.Length);
        }
      }
      finally
      {
        graphics.Restore(state);
      }
    }

    private static Rectangle Inset(Rectangle rectangle, int amount)
    {
      amount = Math.Min(amount, Math.Max(0, rectangle.Width / 2 - 2));
      return new Rectangle(rectangle.Left + amount, rectangle.Top + amount,
        Math.Max(1, rectangle.Width - amount * 2 - 1), Math.Max(1, rectangle.Height - amount * 2 - 1));
    }

    private static void DrawBorder(Graphics graphics, Rectangle rectangle, Color color)
    {
      using var halo = new Pen(Color.FromArgb(225, Color.White), 5);
      using var pen = new Pen(color, 2.5f);
      graphics.DrawRectangle(halo, rectangle);
      graphics.DrawRectangle(pen, rectangle);
    }

    private static void DrawLabel(Graphics graphics, Rectangle rectangle, string text, Color color)
    {
      using var font = new Font(SystemFonts.CaptionFont.FontFamily, 7, FontStyle.Bold);
      using var brush = new SolidBrush(color);
      using var background = new SolidBrush(Color.FromArgb(235, Color.White));
      using var format = new StringFormat { Trimming = StringTrimming.EllipsisCharacter };
      float width = Math.Min(rectangle.Width - 12, graphics.MeasureString(text, font).Width);
      var label = new RectangleF(rectangle.Left + 7, rectangle.Top + 5, width, font.Height);
      graphics.FillRectangle(background, label);
      graphics.DrawString(text, font, brush, label, format);
    }

    private static PointF[] BorderSegment(Rectangle rectangle, int index, int count)
    {
      float perimeter = 2 * (rectangle.Width + rectangle.Height);
      float start = perimeter * index / count;
      float end = perimeter * (index + 1) / count;
      var points = new List<PointF> { BorderPoint(rectangle, start) };
      foreach (float corner in new[] { (float)rectangle.Width, rectangle.Width + rectangle.Height,
        2 * rectangle.Width + rectangle.Height })
        if (corner > start && corner < end)
          points.Add(BorderPoint(rectangle, corner));
      points.Add(BorderPoint(rectangle, end));
      return points.ToArray();
    }

    private static PointF BorderPoint(Rectangle rectangle, float distance)
    {
      if (distance <= rectangle.Width)
        return new PointF(rectangle.Left + distance, rectangle.Top);
      distance -= rectangle.Width;
      if (distance <= rectangle.Height)
        return new PointF(rectangle.Right, rectangle.Top + distance);
      distance -= rectangle.Height;
      if (distance <= rectangle.Width)
        return new PointF(rectangle.Right - distance, rectangle.Bottom);
      return new PointF(rectangle.Left, rectangle.Bottom - (distance - rectangle.Width));
    }

    private static PointF Center(Rectangle rectangle)
    {
      return new PointF(rectangle.Left + rectangle.Width / 2f, rectangle.Top + rectangle.Height / 2f);
    }

    private static PointF Edge(Rectangle rectangle, PointF toward)
    {
      PointF center = Center(rectangle);
      float dx = toward.X - center.X;
      float dy = toward.Y - center.Y;
      if (dx == 0 && dy == 0)
        return center;
      float scale = Math.Min(Math.Max(1, rectangle.Width / 2f - 10) / Math.Abs(dx),
        Math.Max(1, rectangle.Height / 2f - 10) / Math.Abs(dy));
      return new PointF(center.X + dx * scale, center.Y + dy * scale);
    }

    private static void DrawPath(Graphics graphics, Board board, Func<int, Rectangle> map,
      CaptureThreat threat, bool castle, int index, int count)
    {
      CapturePath path = threat.Path;
      bool pastScreen = path.ScreenSquare < 0;
      bool pastTarget = false;
      for (int step = 1; step < path.Squares.Count; step++)
      {
        int fromSquare = path.Squares[step - 1];
        int toSquare = path.Squares[step];
        if (fromSquare == path.ScreenSquare)
          pastScreen = true;
        PointF from = Center(map(fromSquare));
        PointF to = Center(map(toSquare));
        bool arrow = toSquare == threat.TargetSquare;
        Color color = pastTarget ? Color.SlateGray : !pastScreen ? Color.ForestGreen :
          castle ? Color.Goldenrod : Color.Firebrick;
        if (step == 1)
          from = Edge(map(fromSquare), to);
        if (arrow)
        {
          to = Edge(map(toSquare), from);
          float dx = to.X - from.X;
          float dy = to.Y - from.Y;
          float length = Math.Max(1, (float)Math.Sqrt(dx * dx + dy * dy));
          float offset = count <= 1 ? 0 : ((float)index / (count - 1) - 0.5f) * map(toSquare).Width * 0.25f;
          to = new PointF(to.X - dy / length * offset, to.Y + dx / length * offset);
        }
        bool wrapped = path.WrapsFiles &&
          Math.Abs(board.GetFile(fromSquare) - board.GetFile(toSquare)) > 1;
        if (wrapped)
        {
          Rectangle first = map(fromSquare);
          Rectangle last = map(toSquare);
          float y = (from.Y + to.Y) / 2;
          float firstEdge = from.X < to.X ? first.Left : first.Right;
          float lastEdge = from.X < to.X ? last.Right : last.Left;
          DrawStroke(graphics, from, new PointF(firstEdge, y), color, false, true);
          DrawStroke(graphics, new PointF(lastEdge, y), to, color, arrow, true);
        }
        else
          DrawStroke(graphics, from, to, color, arrow, path.IsLeap);
        if (toSquare == path.ScreenSquare)
        {
          PointF screen = Center(map(toSquare));
          using var halo = new Pen(Color.White, 5);
          using var pen = new Pen(Color.ForestGreen, 2);
          graphics.DrawEllipse(halo, screen.X - 6, screen.Y - 6, 12, 12);
          graphics.DrawEllipse(pen, screen.X - 6, screen.Y - 6, 12, 12);
        }
        if (arrow)
          pastTarget = true;
      }
    }

    private static void DrawStroke(Graphics graphics, PointF from, PointF to,
      Color color, bool arrow, bool leap)
    {
      using var halo = new Pen(Color.FromArgb(225, Color.White), 4.5f);
      using var pen = new Pen(color, 2.2f);
      using var cap = new AdjustableArrowCap(3, 4, true);
      if (leap)
        halo.DashStyle = pen.DashStyle = DashStyle.Dash;
      if (arrow)
        halo.CustomEndCap = pen.CustomEndCap = cap;
      graphics.DrawLine(halo, from, to);
      graphics.DrawLine(pen, from, to);
    }
  }
}
