using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LegendaryChat.Elements
{
    public class FadeLabel : Label
    {
        public byte HoverColor = 236;
        public byte NoHoverColor = 159;
        public bool KeepColor;

        public FadeLabel()
        {
            this.MouseEnter += FadeLabel_MouseEnter;
            this.MouseLeave += FadeLabel_MouseLeave;
            this.Foreground = new SolidColorBrush(Color.FromRgb(NoHoverColor, NoHoverColor, NoHoverColor));
        }

        public FadeLabel(byte HoverColor, byte NoHoverColor)
        {
            this.HoverColor = HoverColor;
            this.NoHoverColor = NoHoverColor;
            this.MouseEnter += FadeLabel_MouseEnter;
            this.MouseLeave += FadeLabel_MouseLeave;
            this.Foreground = new SolidColorBrush(Color.FromRgb(NoHoverColor, NoHoverColor, NoHoverColor));
        }

        void FadeLabel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (KeepColor)
                return;

            FadeOut();
        }

        public void FadeOut()
        {
            FadeLabel FadeLabel = this;
            var changeColorAnimation = new ColorAnimation(Color.FromRgb(NoHoverColor, NoHoverColor, NoHoverColor), TimeSpan.FromSeconds(0.5));
            Storyboard s = new Storyboard();
            s.Duration = new Duration(new TimeSpan(0, 0, 1));
            s.Children.Add(changeColorAnimation);
            Storyboard.SetTarget(changeColorAnimation, FadeLabel);
            Storyboard.SetTargetProperty(changeColorAnimation, new PropertyPath("Foreground.Color"));
            s.Begin();
        }

        void FadeLabel_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            FadeLabel RegionLabel = this;
            var changeColorAnimation = new ColorAnimation(Color.FromRgb(HoverColor, HoverColor, HoverColor), TimeSpan.FromSeconds(0.5));
            Storyboard s = new Storyboard();
            s.Duration = new Duration(new TimeSpan(0, 0, 1));
            s.Children.Add(changeColorAnimation);
            Storyboard.SetTarget(changeColorAnimation, RegionLabel);
            Storyboard.SetTargetProperty(changeColorAnimation, new PropertyPath("Foreground.Color"));
            s.Begin();
        }
    }
}