using System;
using System.Windows;

namespace LegendaryClient.Forms
{
    /// <summary>
    /// Represents a Framework element which is acting as a border for a window.
    /// </summary>
    public class WindowBorder
    {
        /// <summary>
        /// The element which is acting as the border.
        /// </summary>
        public FrameworkElement Element { get; private set; }

        /// <summary>
        /// The position of the border.
        /// </summary>
        public BorderPosition Position { get; private set; }

        /// <summary>
        /// Creates a new window border using the specified element and position.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="element"></param>
        public WindowBorder(BorderPosition position, FrameworkElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            Position = position;
            Element = element;
        }
    }
}
