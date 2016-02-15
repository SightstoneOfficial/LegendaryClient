#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Sightstone.Patcher.Logic.Region;

#endregion

namespace Sightstone.Patcher.Logic
{
    internal class Client
    {
        /// <summary>
        /// Resource Text Dictionary
        /// </summary>
        internal static ResourceDictionary Dict = new ResourceDictionary();

        /// <summary>
        /// Get the text from the ResourceDictionary
        /// </summary>
        /// <param name="key">The key in the <see cref="Dict"/></param>
        /// <returns>The <see cref="Dict"/> value for the key or the <see cref="key"/> if not found</returns>
        public static string GetDictText(string key)
        {
            foreach (var keys in Dict.Keys.Cast<string>().Where(keys => keys == key))
            {
                return ((string)Dict[keys]).Replace("\n", Environment.NewLine);
            }
            return key;
        }

        /// <summary>
        ///     So Sightstone.Patcher knows if it should patch League of Legends
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
        ///     Where Sightstone Was Launched, Used for the Patcher
        /// </summary>
        internal static string ExecutingDirectory = "";

        /// <summary>
        ///     MainWindow
        /// </summary>
        internal static MainWindow Win;

        internal static SplashPage SplashPage;

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

        /// <summary>
        ///     Used to play sounds
        /// </summary>
        internal static MediaElement SoundPlayer;

        /// <summary>
        /// The region to patch to
        /// </summary>
        internal static MainRegion Region;

        internal static Type CurrentPage; //Stop changing to same page
        internal static List<Page> CachedPages = new List<Page>();

        // ReSharper disable once InconsistentNaming
        internal static void RunOnUIThread(Action function)
        {
            MainHolder.Dispatcher.BeginInvoke(DispatcherPriority.Input, function);
        }

        // ReSharper disable once InconsistentNaming
        internal static async void RunAsyncOnUIThread(Action function)
        {
            await MainHolder.Dispatcher.BeginInvoke(DispatcherPriority.Input, function);
        }

        internal static List<T> GetInstances<T>()
        {
            return (from t in Assembly.GetExecutingAssembly().GetTypes()
                    where t.BaseType == (typeof(T)) && t.GetConstructor(Type.EmptyTypes) != null
                    select (T)Activator.CreateInstance(t)).ToList();
        }

        internal static Label RegionLabel;

        //TODO: Add ability to change number in settings
        internal static int MaximumWebClient = 5;

        internal static void SwitchPage<T>(bool fade = false, params object[] args)
        {
            if (CurrentPage == typeof (T))
                return;

            var instance = (Page) Activator.CreateInstance(typeof (T), args);
            CurrentPage = typeof (T);

            {
                var foundPage = false;
                foreach (var p in CachedPages.Where(p => p.GetType() == typeof (T)))
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