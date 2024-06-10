using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SnowyRiver.WPF.Controls.Diagram
{
    public class Connection : Control
    {
        static Connection()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Connection), new FrameworkPropertyMetadata(typeof(Connection)));
        }

        private Connector? _source;
        public Connector? Source
        {
            get => _source;
            set
            {
                if (_source != value)
                {
                    if (_source != null)
                    {
                        _source.PropertyChanged -= OnConnectorPositionChanged;
                        _source.Connections.Remove(this);
                    }

                    _source = value;

                    if (_source != null)
                    {
                        _source.Connections.Add(this);
                        _source.PropertyChanged += OnConnectorPositionChanged;
                    }

                    UpdatePathGeometry();
                }
            }
        }

        private Connector? _sink;
        public Connector? Sink
        {
            get => _sink;
            set
            {
                if (_sink != value)
                {
                    if (_sink != null)
                    {
                        _sink.PropertyChanged -= OnConnectorPositionChanged;
                        _sink.Connections.Remove(this);
                    }

                    _sink = value;

                    if (_sink != null)
                    {
                        _sink.Connections.Add(this);
                        _sink.PropertyChanged += OnConnectorPositionChanged;
                    }

                    UpdatePathGeometry();
                }
            }
        }



        private void OnConnectorPositionChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is "Position")
            {
                UpdatePathGeometry();
            }
        }


        private void UpdatePathGeometry()
        {
            if (Source != null && Sink != null)
            {
                var geometry = new PathGeometry();
                List<Point> linePoints = PathFinder.GetConnectionLine(Source.GetInfo(), Sink.GetInfo(), true);
                if (linePoints.Count > 0)
                {
                    PathFigure figure = new PathFigure();
                    figure.StartPoint = linePoints[0];
                    linePoints.Remove(linePoints[0]);
                    figure.Segments.Add(new PolyLineSegment(linePoints, true));
                    geometry.Figures.Add(figure);

                    this.PathGeometry = geometry;
                }
            }
        }
    }
}
