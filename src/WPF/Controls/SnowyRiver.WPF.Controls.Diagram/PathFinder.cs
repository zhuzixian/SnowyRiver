using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SnowyRiver.WPF.Controls.Diagram
{
    internal class PathFinder
    {
        private const int Margin = 10;

        internal static List<Point> GetConnectionLine(ConnectorInfo source, ConnectorInfo sink, bool showLastLine)
        {
            var linePoints = new List<Point>();

            var rectSource = GetRectWidthMargin(source, Margin);
            var rectSink = GetRectWidthMargin(sink, Margin);

            var startPoint = GetOffsetPoint(source, rectSource);
            var endPoint = GetOffsetPoint(sink, rectSink);

            linePoints.Add(startPoint);
            var currentPoint = startPoint;

            if (!rectSink.Contains(currentPoint) && !rectSource.Contains(endPoint))
            {
                while (true)
                {
                    if (IsPointVisible(currentPoint, endPoint, [rectSource, rectSink]))
                    {
                        linePoints.Add(endPoint);
                        currentPoint = endPoint;
                        break;
                    }

                    var neighbour = GetNearestVisibleNeighborSink(currentPoint, endPoint, sink, rectSource, rectSink);
                    if (!double.IsNaN(neighbour.X))
                    {
                        linePoints.Add(neighbour);
                        linePoints.Add(endPoint);
                        currentPoint = endPoint;
                        break;
                    }

                    if (currentPoint == startPoint)
                    {
                        var p = GetNearestNeighborSource(source, endPoint, rectSource, rectSink, out var flag);
                        linePoints.Add(p);
                        currentPoint = p;

                        if (!IsRectVisible(currentPoint, rectSink, [rectSource]))
                        {
                            GetOppositeCorners(source.Orientation, rectSource, out var p1, out var p2);
                            if (flag)
                            {
                                linePoints.Add(p1);
                                currentPoint = p1;
                            }
                            else
                            {
                                linePoints.Add(p2);
                                currentPoint = p2;
                            }

                            if (!IsRectVisible(currentPoint, rectSink, [rectSource]))
                            {
                                if (flag)
                                {
                                    linePoints.Add(p2);
                                    currentPoint = p2;
                                }
                                else
                                {
                                    linePoints.Add(p1);
                                    currentPoint = p1;
                                }
                            }
                        }
                    }
                    else
                    {
                        GetNeighborCorners(sink.Orientation, rectSink, out var s1, out var s2);
                        GetOppositeCorners(sink.Orientation, rectSink, out var n1, out var n2);

                        var n1IsVisible = IsPointVisible(currentPoint, n1, [rectSource, rectSink]);
                        var n2IsVisible = IsPointVisible(currentPoint, n2, [rectSource, rectSink]);

                        if (n1IsVisible && n2IsVisible)
                        {
                            if (rectSource.Contains(n1))
                            {
                                linePoints.Add(n2);
                                if (rectSource.Contains(s2))
                                {
                                    linePoints.Add(n1);
                                    linePoints.Add(s1);
                                }
                                else
                                {
                                    linePoints.Add(s2);
                                }

                                linePoints.Add(endPoint);
                                currentPoint = endPoint;
                                break;
                            }

                            if (rectSource.Contains(n2))
                            {
                                linePoints.Add(n1);
                                if (rectSource.Contains(s1))
                                {
                                    linePoints.Add(n2);
                                    linePoints.Add(s2);
                                }
                                else
                                {
                                    linePoints.Add(s1);
                                }

                                linePoints.Add(endPoint);
                                currentPoint = endPoint;
                                break;
                            }

                            if (Distance(n1, endPoint) <= Distance(n2, endPoint))
                            {
                                linePoints.Add(n1);
                                if (rectSource.Contains(s1))
                                {
                                    linePoints.Add(n2);
                                    linePoints.Add(s2);
                                }
                                else
                                {
                                    linePoints.Add(s1);
                                }

                                linePoints.Add(endPoint);
                                currentPoint = endPoint;
                                break;
                            }

                            linePoints.Add(n2);
                            if (rectSource.Contains(s2))
                            {
                                linePoints.Add(n1);
                                linePoints.Add(s1);
                            }
                            else
                            {
                                linePoints.Add(s2);
                            }

                            linePoints.Add(endPoint);
                            currentPoint = endPoint;
                            break;
                        }

                        if (n1IsVisible)
                        {
                            linePoints.Add(n1);
                            if (rectSource.Contains(s1))
                            {
                                linePoints.Add(n2);
                                linePoints.Add(s2);
                            }
                            else
                            {
                                linePoints.Add(s1);
                            }
                            linePoints.Add(endPoint);
                            currentPoint = endPoint;
                            break;
                        }

                        linePoints.Add(n2);
                        if (rectSource.Contains(s2))
                        {
                            linePoints.Add(n1);
                            linePoints.Add(s1);
                        }
                        else
                        {
                            linePoints.Add(s2);
                        }
                        linePoints.Add(endPoint);
                        currentPoint = endPoint;
                        break;
                    }
                }
            }
            else
            {
                linePoints.Add(endPoint);
            }

            linePoints = OptimizeLinePoints(linePoints, [rectSource, rectSink], source.Orientation, sink.Orientation);
            CheckPathEnd(source, sink, showLastLine, linePoints);
            return linePoints;
        }

        private static void CheckPathEnd(ConnectorInfo source, ConnectorInfo sink, bool showLastLine, List<Point> linePoints)
        {
            if (showLastLine)
            {
                var startPoint = new Point(0, 0);
                var endPoint = new Point(0, 0);
                var marginPath = 15;
                switch (source.Orientation)
                {
                    case ConnectorOrientation.Left:
                        startPoint = new Point(source.Position.X - marginPath, source.Position.Y);
                        break;

                    case ConnectorOrientation.Top:
                        startPoint = new Point(source.Position.X, source.Position.Y - marginPath);
                        break;

                    case ConnectorOrientation.Right:
                        startPoint = new Point(source.Position.X + marginPath, source.Position.Y);
                        break;

                    case ConnectorOrientation.Bottom:
                        startPoint = new Point(source.Position.X, source.Position.Y + marginPath);
                        break;
                }

                switch (sink.Orientation)
                {
                    case ConnectorOrientation.Left:
                        endPoint = new Point(sink.Position.X - marginPath, sink.Position.Y);
                        break;

                    case ConnectorOrientation.Top:
                        endPoint = new Point(sink.Position.X, sink.Position.Y - marginPath);
                        break;

                    case ConnectorOrientation.Right:
                        endPoint = new Point(sink.Position.X + marginPath, sink.Position.Y);
                        break;

                    case ConnectorOrientation.Bottom:
                        endPoint = new Point(sink.Position.X, sink.Position.Y + marginPath);
                        break;
                }

                linePoints.Add(startPoint);
                linePoints.Add(endPoint);
            }
            else
            {
                linePoints.Insert(0, source.Position);
                linePoints.Add(sink.Position);
            }
        }

        private static List<Point> OptimizeLinePoints(List<Point> linePoints, Rect[] rectangles, 
            ConnectorOrientation sourceOrientation, ConnectorOrientation sinkOrientation)
        {
            var points = new List<Point>();
            int cut = 0;

            for (var i = 0; i < linePoints.Count; i++)
            {
                if (i >= cut)
                {
                    for(var k = linePoints.Count - 1; k > i; k--)
                    {
                        if (IsPointVisible(linePoints[i], linePoints[k], rectangles))
                        {
                            cut = k;
                            break;
                        }
                    }
                    points.Add(linePoints[i]);
                }
            }

            for (var j = 0; j < points.Count - 1; j++)
            {
                if (Math.Abs(points[j].X - points[j + 1].X) > 0.1 && Math.Abs(points[j].Y - points[j + 1].Y) > 0.1)
                {
                    var orientationFrom = j == 0 
                        ? sourceOrientation 
                        : GetOrientation(points[j], points[j - 1]);
                    var orientationTo = j == points.Count - 2 
                        ? sinkOrientation 
                        : GetOrientation(points[j + 1], points[j + 2]);

                    if (orientationFrom is ConnectorOrientation.Left or ConnectorOrientation.Right
                        && orientationTo is ConnectorOrientation.Left or ConnectorOrientation.Right)
                    {
                        var centerX = Math.Min(points[j].X,
                            points[j + 1].X + Math.Abs(points[j].X - points[j + 1].X) / 2);
                        points.Insert(j+1, new Point(centerX, points[j].Y));
                        points.Insert(j+2, new Point(centerX, points[j + 2].Y));
                        if (points.Count - 1 > j + 3)
                        {
                            points.RemoveAt(j + 3);
                        }

                        return points;
                    }

                    if (orientationFrom is ConnectorOrientation.Top or ConnectorOrientation.Bottom
                        && orientationTo is ConnectorOrientation.Top or ConnectorOrientation.Bottom)
                    {
                        var centerY = Math.Min(points[j].Y,
                            points[j + 1].Y + Math.Abs(points[j].Y - points[j + 1].Y) / 2);
                        points.Insert(j + 1, new Point(points[j].X, centerY));
                        points.Insert(j + 2, new Point(points[j + 2].X, centerY));
                        if (points.Count - 1 > j + 3)
                        {
                            points.RemoveAt(j + 3);
                        }

                        return points;
                    }

                    if (orientationFrom is ConnectorOrientation.Left or ConnectorOrientation.Right
                        && orientationTo is ConnectorOrientation.Top or ConnectorOrientation.Bottom)
                    {
                        points.Insert(j + 1, new Point(points[j + 1].X, points[j].Y));
                        return points;
                    }

                    if (orientationFrom is ConnectorOrientation.Top or ConnectorOrientation.Bottom
                        && orientationTo is ConnectorOrientation.Left or ConnectorOrientation.Right)
                    {
                        points.Insert(j + 1, new Point(points[j].X, points[j + 1].Y));
                        return points;
                    }
                }
            }

            return points;
        }

        private static ConnectorOrientation GetOrientation(Point p1, Point p2)
        {
            if (Math.Abs(p1.X - p2.X) < 0.1)
            {
                return p1.Y >= p2.Y ? ConnectorOrientation.Bottom : ConnectorOrientation.Top;
            }

            if (Math.Abs(p1.Y - p2.Y) < 0.1)
            {
                return p1.X >= p2.X ? ConnectorOrientation.Right : ConnectorOrientation.Left;
            }

            throw new Exception("Failed to retrieve orientation");
        }

        private static void GetOppositeCorners(ConnectorOrientation orientation, Rect rect, out Point p1, out Point p2)
        {
            switch (orientation)
            {
                case ConnectorOrientation.Left:
                    p1 = rect.TopRight;
                    p2 = rect.BottomRight;
                    break;

                case ConnectorOrientation.Top:
                    p1 = rect.BottomLeft;
                    p2 = rect.BottomRight;
                    break;

                case ConnectorOrientation.Right:
                    p1 = rect.TopLeft;
                    p2 = rect.BottomLeft;
                    break;

                case ConnectorOrientation.Bottom:
                    p1 = rect.TopLeft;
                    p2 = rect.TopRight;
                    break;

                default:
                    throw new Exception("No opposite corners found!");
            }
        }

        private static bool IsRectVisible(Point fromPoint, Rect targetRect, Rect[] rectangles)
        {
            if (IsPointVisible(fromPoint, targetRect.TopLeft, rectangles))
                return true;

            if (IsPointVisible(fromPoint, targetRect.TopRight, rectangles))
                return true;

            if (IsPointVisible(fromPoint, targetRect.BottomLeft, rectangles))
                return true;

            if (IsPointVisible(fromPoint, targetRect.BottomRight, rectangles))
                return true;

            return false;
        }

        private static Point GetNearestNeighborSource(ConnectorInfo source, Point endPoint, 
            Rect rectSource, Rect rectSink, out bool flag)
        {
            GetNeighborCorners(source.Orientation, rectSource, out var p1, out var p2);

            if (rectSink.Contains(p1))
            {
                flag = false;
                return p2;
            }

            if (rectSink.Contains(p2))
            {
                flag = true;
                return p1;
            }

            if (Distance(p1, endPoint) <= Distance(p2, endPoint))
            {
                flag = true;
                return p1;
            }

            flag = false;
            return p2;
        }

        private static Point GetNearestVisibleNeighborSink(Point currentPoint, Point endPoint, ConnectorInfo sink,
            Rect rectSource, Rect rectSink)
        {
            GetNeighborCorners(sink.Orientation, rectSink, out var p1, out var p2);

            var p1IsVisible = IsPointVisible(currentPoint, p1, [rectSource, rectSink]);
            var p2IsVisible = IsPointVisible(currentPoint, p2, [rectSource, rectSink]);

            if (p1IsVisible)
            {
                if (p2IsVisible)
                {
                    if (rectSink.Contains(p1))
                        return p2;

                    if (rectSink.Contains(p2))
                        return p1;

                    return Distance(p1, endPoint) <= Distance(p2, endPoint) ? p1 : p2;
                }

                return p1;
            }

            return p2IsVisible ? p2 : new Point(double.NaN, double.NaN);
        }

        private static double Distance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        private static void GetNeighborCorners(ConnectorOrientation orientation, Rect rect, 
            out Point p1, out Point p2)
        {
            switch (orientation)
            {
                case ConnectorOrientation.Left:
                    p1 = rect.TopLeft;
                    p2 = rect.BottomLeft;
                    break;

                case ConnectorOrientation.Top:
                    p1 = rect.TopLeft;
                    p2 = rect.TopRight;
                    break;

                case ConnectorOrientation.Right:
                    p1 = rect.TopRight;
                    p2 = rect.BottomRight;
                    break;

                case ConnectorOrientation.Bottom:
                    p1 = rect.BottomLeft;
                    p2 = rect.BottomRight;
                    break;

                default:
                    throw new Exception("No neighbor corners found!");
            }
        }
            

        private static bool IsPointVisible(Point fromPoint, Point targetPoint, Rect[] rectangles)
        {
            return rectangles
                .All(rect => !RectangleIntersectsLine(rect, fromPoint, targetPoint));
        }

        private static bool RectangleIntersectsLine(Rect rect, Point startPoint, Point endPoint)
        {
            rect.Inflate(-1, -1);
            return rect.IntersectsWith(new Rect(startPoint, endPoint));
        }

        private static Point GetOffsetPoint(ConnectorInfo connector, Rect rect)
        {
            var offsetPoint = new Point();

            switch (connector.Orientation)
            {
                case ConnectorOrientation.Left:
                    offsetPoint = new Point(rect.Left, connector.Position.Y);
                    break;

                case ConnectorOrientation.Top:
                    offsetPoint = new Point(connector.Position.X, rect.Top);
                    break;

                case ConnectorOrientation.Right:
                    offsetPoint = new Point(rect.Right, connector.Position.Y);
                    break;

                case ConnectorOrientation.Bottom:
                    offsetPoint = new Point(connector.Position.X, rect.Bottom);
                    break;
            }

            return offsetPoint;
        }

        private static Rect GetRectWidthMargin(ConnectorInfo connectorInfo, double margin)
        {
            var rect = new Rect(connectorInfo.DesignerItemLeft, connectorInfo.DesignerItemTop, 
                connectorInfo.DesignerItemSize.Width, connectorInfo.DesignerItemSize.Height);
            rect.Inflate(margin, margin);
            return rect;
        }
    }
}
