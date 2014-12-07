﻿#region

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

#endregion

namespace LegendaryClient.Controls
{
    public class TextBoxWatermarked : TextBox
    {
        public static readonly DependencyProperty WaterMarkProperty =
            DependencyProperty.Register("Watermark", typeof (string), typeof (TextBoxWatermarked),
                new PropertyMetadata(OnWatermarkChanged));

        private bool _isWatermarked;
        private Binding _textBinding;

        public TextBoxWatermarked()
        {
            Loaded += (s, ea) => ShowWatermark();
        }

        public string Watermark
        {
            get { return (string) GetValue(WaterMarkProperty); }
            set { SetValue(WaterMarkProperty, value); }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            HideWatermark();
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            ShowWatermark();
        }

        private static void OnWatermarkChanged(DependencyObject sender, DependencyPropertyChangedEventArgs ea)
        {
            var tbw = sender as TextBoxWatermarked;
            if (tbw == null || !tbw.IsLoaded)
                return;
            //needed to check IsLoaded so that we didn't dive into the ShowWatermark() routine before initial Bindings had been made
            tbw.ShowWatermark();
        }

        private void ShowWatermark()
        {
            if (!String.IsNullOrEmpty(Text) || String.IsNullOrEmpty(Watermark))
                return;

            _isWatermarked = true;

            //save the existing binding so it can be restored
            _textBinding = BindingOperations.GetBinding(this, TextProperty);

            //blank out the existing binding so we can throw in our Watermark
            BindingOperations.ClearBinding(this, TextProperty);

            //set the signature watermark gray
            Foreground = new SolidColorBrush(Colors.Gray);

            //display our watermark text
            Text = Watermark;
        }

        private void HideWatermark()
        {
            if (!_isWatermarked)
                return;

            _isWatermarked = false;
            ClearValue(ForegroundProperty);
            Text = "";
            if (_textBinding != null)
                SetBinding(TextProperty, _textBinding);
        }
    }
}