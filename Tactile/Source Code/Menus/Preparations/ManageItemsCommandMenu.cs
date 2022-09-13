using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Windows.Command;

namespace Tactile.Menus.Preparations
{
    class ManageItemsCommandMenu : ItemsCommandMenu
    {
        public ManageItemsCommandMenu(int actorId, IHasCancelButton menu = null)
            : base(actorId, menu) { }

        protected override Window_Command NewWindow()
        {
            List<string> strs = new List<string> { "Trade", "List", "Convoy", "Give All", "Optimize" };
            var commandWindow = new Window_Command(
                new Vector2(Config.WINDOW_WIDTH - 128, Config.WINDOW_HEIGHT - 108),
                56,
                strs);
            if (Global.battalion.actors.Count <= 1)
                commandWindow.set_text_color(0, "Grey");
            if (Global.battalion.actors.Count <= 1 &&
                    Global.battalion.convoy_id == -1)
                commandWindow.set_text_color(1, "Grey");
            if (Global.battalion.convoy_id == -1)
            {
                commandWindow.set_text_color(2, "Grey");
                commandWindow.set_text_color(3, "Grey");
                commandWindow.set_text_color(4, "Grey");
            }
            commandWindow.size_offset = new Vector2(0, -8);
            commandWindow.text_offset = new Vector2(0, -4);
            commandWindow.glow_width = 56 - 8;
            commandWindow.glow = true;
            commandWindow.bar_offset = new Vector2(-8, 0);
            commandWindow.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Preparations_Item_Options_Window");
            commandWindow.color_override = 0;
            commandWindow.set_columns(2);
            commandWindow.stereoscopic = Config.PREPITEM_WINDOW_DEPTH;

            return commandWindow;
        }

        public override void Refresh()
        {
            // Give All
            if (Global.battalion.has_convoy && this.actor.CanGiveAny)
                Window.set_text_color(3, "White");
            else
                Window.set_text_color(3, "Grey");
        }

        public override void ToUseItem() { }
    }
}
