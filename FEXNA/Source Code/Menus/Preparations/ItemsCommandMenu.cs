using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Windows.Command;

namespace FEXNA.Menus.Preparations
{
    class ItemsCommandMenu : CommandMenu
    {
        private int ActorId;

        public Game_Actor actor { get { return Global.game_actors[ActorId]; } }
        
        public ItemsCommandMenu(int actorId)
        {
            Window = NewWindow();
            CreateCancelButton(null);

            ActorId = actorId;
            Refresh();
        }

        protected virtual Window_Command NewWindow()
        {
            List<string> strs = new List<string> { "Trade", "List", "Convoy", "Give All", "Use", "Restock", "Optimize" };
            if (Global.game_system.home_base ||
                    (Global.battalion.has_convoy && Global.game_battalions.active_convoy_shop != null))
                strs.Add("Shop");
            var commandWindow = new Window_Command(new Vector2(Config.WINDOW_WIDTH - 128, Config.WINDOW_HEIGHT - 100), 56, strs);
            if (Global.battalion.actors.Count <= 1)
                commandWindow.set_text_color(0, "Grey");
            if (Global.battalion.actors.Count <= 1 &&
                    !Global.battalion.has_convoy)
                commandWindow.set_text_color(1, "Grey");
            if (!Global.battalion.has_convoy)
            {
                commandWindow.set_text_color(2, "Grey");
                commandWindow.set_text_color(6, "Grey");
                commandWindow.set_text_color(7, "Grey");
            }
            else if (Global.game_battalions.active_convoy_shop == null)
                commandWindow.set_text_color(7, "Grey");
            commandWindow.size_offset = new Vector2(0, -8);
            commandWindow.text_offset = new Vector2(0, -4);
            commandWindow.glow_width = 56 - 8;
            commandWindow.glow = true;
            commandWindow.bar_offset = new Vector2(-8, 0);
            commandWindow.texture = Global.Content.Load<Texture2D>(
                @"Graphics\Windowskins\Preparations_Item_Options_Window");
            commandWindow.color_override = 0;
            commandWindow.set_columns(2);
            commandWindow.stereoscopic = Config.PREPITEM_WINDOW_DEPTH;

            return commandWindow;
        }

        public virtual void Refresh()
        {
            // Use
            if (this.actor.can_use_convoy_item())
                Window.set_text_color(4, "White");
            else
                Window.set_text_color(4, "Grey");

            // Give All
            if (Global.battalion.has_convoy && this.actor.CanGiveAny)
                Window.set_text_color(3, "White");
            else
                Window.set_text_color(3, "Grey");

            // Restock
            if (Global.battalion.has_convoy && this.actor.CanRestock())
                Window.set_text_color(5, "White");
            else
                Window.set_text_color(5, "Grey");
        }

        public virtual void ToUseItem()
        {
            Window.immediate_index = 4;
        }
    }
}
