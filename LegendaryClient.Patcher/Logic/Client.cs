#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

#endregion

namespace LegendaryClient.Patcher.Logic
{
    internal class Client
    {
        /// <summary>
        ///     So LegendaryClient.Patcher knows if it should patch League of Legends
        /// </summary>
        internal static bool LoLDataIsUpToDate = false;

        /// <summary>
        ///     The most up to date version int of League of Legends
        /// </summary>
        internal static string LatestLoLDataVersion = "";

        /// <summary>
        ///     The users version of League of Legends
        /// </summary>
        internal static string LoLDataVersion = "";

        /// <summary>
        ///     Where LegendaryClient Was Launched, Used for the Patcher
        /// </summary>
        internal static String ExecutingDirectory = "";

        /// <summary>
        ///     MainWindow
        /// </summary>
        internal static Window Win;

        /// <summary>
        ///     Used to swich pages
        /// </summary>
        internal static ContentControl MainHolder;

        /// <summary>
        ///     Used to Create an overlay or a notification
        /// </summary>
        internal static Grid OverlayGrid;

        /// <summary>
        ///     Used to Create an overlay or a notification
        /// </summary>
        internal static ContentControl OverlayContainer;


        internal static Type CurrentPage; //Stop changing to same page
        internal static List<Page> CachedPages = new List<Page>();

        internal static void SwitchPage<T>(bool fade = false, params object[] args)
        {
            if (CurrentPage == typeof (T))
                return;

            var instance = (Page) Activator.CreateInstance(typeof (T), args);
            CurrentPage = typeof (T);

            {
                bool foundPage = false;
                foreach (Page p in CachedPages.Where(p => p.GetType() == typeof (T)))
                {
                    instance = p;
                    foundPage = true;
                }

                if (!foundPage)
                    CachedPages.Add(instance);
            }

            if (fade)
            {
                var fadeOutAnimation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.25));
                fadeOutAnimation.Completed += (x, y) =>
                {
                    MainHolder.Content = instance.Content;
                    var fadeInAnimation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.25));
                    MainHolder.BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);
                };
                MainHolder.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);
            }
            else
            {
                MainHolder.Content = instance.Content;
            }
        }
    }
}