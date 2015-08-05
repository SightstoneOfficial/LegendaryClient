using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Sightstone.Logic.MultiUser;
using Sightstone.Windows;

namespace Sightstone.Controls
{
    public class Screenshot : Image
    {
        public string Path;
        public ScreenshotsPage ScreenshotPage;

        public Screenshot(string Path, ScreenshotsPage sp = null)
        {
            this.Path = Path;
            this.ScreenshotPage = sp;
            this.MouseDown += (s, e) =>
                {
                    View();
                };
        }

        public void View()
        {
            var overlay = new ScreenshotViewer(this.ScreenshotPage.Screenshots.ToList(), Path);
            Client.OverlayContainer.Content = overlay;
            Client.OverlayContainer.Visibility = Visibility.Visible;
        }
    }
}
