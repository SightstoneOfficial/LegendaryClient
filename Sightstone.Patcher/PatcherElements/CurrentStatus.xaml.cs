using System.Windows.Media;

namespace Sightstone.Patcher.PatcherElements
{
    /// <summary>
    /// Interaction logic for CurrentStatus.xaml
    /// </summary>
    public partial class CurrentStatus
    {
        public CurrentStatus()
        {
            InitializeComponent();
        }

        public void UpdateStatus(Status status)
        {
            var bc = new BrushConverter();
            switch(status)
            {
                case Status.Up:
                    StatusEllipse.Fill = (Brush)bc.ConvertFrom("#FF23AA4B");
                    break;
                case Status.Lagging:
                    StatusEllipse.Fill = (Brush)bc.ConvertFrom("#FFE6912D");
                    break;
                case Status.Down:
                    StatusEllipse.Fill = (Brush)bc.ConvertFrom("#FFD21414");
                    break;
            }
        }
    }

    public enum Status
    {
        Up,
        Lagging,
        Down
    }
}
