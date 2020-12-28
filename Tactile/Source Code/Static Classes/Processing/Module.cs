using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tactile
{
    class Module
    {
        public static void scroll_window_move_up(ref int index, ref int scroll, int distance, int visible_rows, int rows,
            bool can_pass_edge)
        {
            if (distance > 1)
            {
                // If scrolling to the top edge
                if (scroll > 0 && scroll - distance <= 0)
                {
                    index = (index - scroll);
                    scroll = 0;
                }
                // If scrolling over the top edge
                else if (index - distance < 0)
                {
                    if (can_pass_edge)
                    {
                        index = rows + (index - scroll) - visible_rows;
                        if (visible_rows < rows)
                            scroll = rows - visible_rows;
                    }
                }
                // Normal scrolling
                else
                {
                    // Account for scrolling being offscreen
                    scroll = normal_scroll(index, scroll, visible_rows, rows);

                    index -= distance;
                    scroll -= distance;
                }
            }
            else
            {
                // If scrolling over the top edge
                if (index - distance < 0)//index == 0) //Debug
                {
                    if (can_pass_edge)
                    {
                        index = rows - 1;
                        if (visible_rows < rows)
                            scroll = rows - visible_rows;
                    }
                }
                else
                {
                    index--;
                    scroll = normal_scroll(index, scroll, visible_rows, rows);
                }
            }
        }

        public static void scroll_window_move_down(ref int index, ref int scroll, int distance, int visible_rows, int rows,
            bool can_pass_edge)
        {
            if (visible_rows >= rows)
                distance = 1;
            if (distance > 1)
            {
                // If scrolling to the bottom edge
                if (scroll < rows - visible_rows && scroll + distance >= rows - visible_rows)
                {
                    index = rows + (index - scroll) - visible_rows;
                    scroll = rows - visible_rows;
                }
                // If scrolling over the bottom edge
                else if (index + distance >= rows)
                {
                    if (can_pass_edge)
                    {
                        index = (index - scroll);
                        scroll = 0;
                    }
                }
                // Normal scrolling
                else
                {
                    // Account for scrolling being offscreen
                    scroll = normal_scroll(index, scroll, visible_rows, rows);

                    index += distance;
                    scroll += distance;
                }
            }
            else
            {
                // If scrolling to the bottom edge
                if (index + distance >= rows)//index == rows - 1) //Debug
                {
                    if (can_pass_edge)
                    {
                        index = 0;
                        scroll = 0;
                    }
                }
                else
                {
                    index++;
                    // Account for scrolling being offscreen
                    scroll = normal_scroll(index, scroll, visible_rows, rows);
                }
            }
        }

        internal static int normal_scroll(int index, int scroll, int visible_rows, int rows)
        {
            while (index > (visible_rows - 2) + scroll &&
                    scroll < rows - (visible_rows))
                scroll++;
            while (index - 1 < scroll && scroll > 0)
                scroll--;

            return scroll;
        }
    }
}
