using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AroundTheWorldClocks.Controls;

public partial class AnalogClock : UserControl
{
    public AnalogClock()
    {
        InitializeComponent();
        AddHourTicks();
    }

    private void AddHourTicks()
    {
        for (int hour = 0; hour < 12; hour++)
        {
            bool isQuarter = hour % 3 == 0;
            var tick = new Line
            {
                X1 = 50,
                Y1 = 6,
                X2 = 50,
                Y2 = isQuarter ? 15 : 11,
                StrokeThickness = isQuarter ? 3 : 1.5,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                RenderTransform = new RotateTransform(hour * 30, 50, 50),
            };
            tick.SetBinding(Shape.StrokeProperty, new Binding(nameof(Foreground)) { Source = this });

            // Insert right after the face so the hands stay on top.
            Dial.Children.Insert(1, tick);
        }
    }
}
