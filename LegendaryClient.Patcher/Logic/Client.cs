using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace LegendaryClient.Patcher.Logic
{
    class Client
    {
        /// <summary>
        /// So LegendaryClient.Patcher knows if it should patch League of Legends
        /// </summary>
        internal static bool LoLDataIsUpToDate = false;

        /// <summary>
        /// We need to know the region to get the right language local.
        /// </summary>
        internal static string Region;

        /// <summary>
        /// The most up to date version int of League of Legends
        /// </summary>
        internal static string LatestLolDataVersion = "";

        /// <summary>
        /// The users version of League of Legends
        /// </summary>
        internal static string LolDataVersion = "";

        /// <summary>
        /// Where LegendaryClient Was Launched, Used for the Patcher
        /// </summary>
        internal static String ExecutingDirectory = "";

        /// <summary>
        /// MainWindow
        /// </summary>
        internal static Window Win;

        /// <summary>
        /// Used to swich pages
        /// </summary>
        internal static ContentControl MainHolder;

        /// <summary>
        /// Used to Create an overlay or a notification
        /// </summary>
        internal static Grid OverlayGrid;

        /// <summary>
        /// Used to Create an overlay or a notification
        /// </summary>
        internal static ContentControl OverlayContainer;


        internal static Type CurrentPage; //Stop changing to same page
        internal static List<Page> CachedPages = new List<Page>();
        internal static void SwitchPage<T>(bool Fade = false, params object[] Arguments)
        {
            if (CurrentPage == typeof(T))
                return;

            Page instance = (Page)Activator.CreateInstance(typeof(T), Arguments);
            CurrentPage = typeof(T);

            {
                bool FoundPage = false;
                foreach (Page p in CachedPages)
                {
                    if (p.GetType() == typeof(T))
                    {
                        instance = p;
                        FoundPage = true;
                    }
                }

                if (!FoundPage)
                    CachedPages.Add(instance);
            }

            if (Fade)
            {
                var fadeOutAnimation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.25));
                fadeOutAnimation.Completed += (x, y) =>
                {
                    MainHolder.Content = instance.Content;
                    var fadeInAnimation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.25));
                    MainHolder.BeginAnimation(ContentControl.OpacityProperty, fadeInAnimation);
                };
                MainHolder.BeginAnimation(ContentControl.OpacityProperty, fadeOutAnimation);
            }
            else
            {
                MainHolder.Content = instance.Content;
            }
        }
    }
}
