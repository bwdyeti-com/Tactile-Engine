using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TactileLibrary
{
    public class RectangleExclusion : IEnumerable<Rectangle>
    {
        private HashSet<Rectangle> Rectangles = new HashSet<Rectangle>();

        public RectangleExclusion(Rectangle rect, IEnumerable<Rectangle> exclusions)
        {
            Rectangles.Add(rect);
            foreach (var exclusion in exclusions)
            {
                Rectangles = new HashSet<Rectangle>(
                    exclude_rectangle(Rectangles, exclusion));
            }
        }

        public IEnumerator<Rectangle> GetEnumerator()
        {
            foreach (var rect in Rectangles)
                yield return rect;
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static IEnumerable<Rectangle> exclude_rectangle(
            IEnumerable<Rectangle> rectangles, Rectangle rect)
        {
            rect = normalize_rectangle(rect);

            foreach (var src_rect in rectangles)
            {
                foreach (var result_rect in exclude_rectangle(src_rect, rect))
                {
                    if (result_rect.Width != 0 && result_rect.Height != 0)
                        yield return result_rect;
                }
            }
        }
        private static IEnumerable<Rectangle> exclude_rectangle(
            Rectangle src_rect, Rectangle rect)
        {
            src_rect = normalize_rectangle(src_rect);
            if (!src_rect.Intersects(rect))
            {
                yield return src_rect;
            }
            else
            {
                // Get top
                if (rect.Y > src_rect.Y)
                {
                    yield return new Rectangle(
                        src_rect.X, src_rect.Y,
                        src_rect.Width, rect.Y - src_rect.Y);
                }
                // Left or right
                //if ()
                {
                    int top = Math.Max(src_rect.Y, rect.Y);
                    int height = Math.Min(src_rect.Bottom, rect.Bottom) - top;
                    // Get left
                    if (rect.X > src_rect.X)
                    {
                        yield return new Rectangle(
                            src_rect.X, top,
                            rect.X - src_rect.X, height);
                    }
                    // Get right
                    if (rect.Right < src_rect.Right)
                    {
                        yield return new Rectangle(
                            rect.Right, top,
                            src_rect.Right - rect.Right, height);
                    }
                }
                // Get bottom
                if (rect.Bottom < src_rect.Bottom)
                {
                    yield return new Rectangle(
                        src_rect.X, rect.Bottom,
                        src_rect.Width, src_rect.Bottom - rect.Bottom);
                }
                /*
                // If rect in completely inside
                if (src_rect.Contains(rect))
                {
                    yield return src_rect;
                }
                else
                {
                    yield return src_rect;
                }*/
            }
        }

        private static Rectangle normalize_rectangle(Rectangle rect)
        {
            rect = new Rectangle(
                rect.Left, rect.Top,
                rect.Right - rect.Left,
                rect.Bottom - rect.Top);
            return rect;
        }
    }
}
