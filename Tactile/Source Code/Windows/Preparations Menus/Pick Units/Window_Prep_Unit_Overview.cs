using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Help;
using Tactile.Graphics.Preparations;
using Tactile.Graphics.Text;
using Tactile.Windows.Command.Items;

namespace Tactile.Windows.Map
{
    abstract class Window_Prep_Unit_Overview : Map_Window_Base
    {
        const int BLACK_SCEEN_FADE_TIMER = 8;
        const int BLACK_SCREEN_HOLD_TIMER = 4;

        protected bool Trading = false;
        protected Window_Prep_Items_Unit Unit_Window;
        protected Window_Command_Item_Preparations Item_Window;
        protected Pick_Units_Items_Header Item_Header;
        protected Button_Description Select;
        protected Sprite Backing_1;

        #region Accessors
        public bool ready { get { return this.ready_for_inputs; } }

        public int actor_id
        {
            get { return Unit_Window.actor_id; }
            set
            {
                Unit_Window.actor_id = value;
                Unit_Window.refresh_scroll();
                refresh();
            }
        }
        #endregion

        protected override void set_black_screen_time()
        {
            Black_Screen_Fade_Timer = BLACK_SCEEN_FADE_TIMER;
            Black_Screen_Hold_Timer = BLACK_SCREEN_HOLD_TIMER;
            base.set_black_screen_time();
        }

        protected virtual void initialize_sprites()
        {
            // Black Screen
            Black_Screen = new Sprite();
            Black_Screen.texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            Black_Screen.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Black_Screen.tint = new Color(0, 0, 0, 255);
            // Background
            Background = new Menu_Background();
            Background.texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Preparation_Background");
            (Background as Menu_Background).vel = new Vector2(0, -1 / 3f);
            (Background as Menu_Background).tile = new Vector2(1, 2);
            Background.stereoscopic = Config.PREP_BG_DEPTH;

            Backing_1 = new Sprite();
            Backing_1.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Preparations_Screen");
            Backing_1.loc = new Vector2(24, 160);
            Backing_1.src_rect = new Rectangle(0, 112, 104, 32);
            Backing_1.tint = new Color(224, 224, 224, 128);
            Backing_1.stereoscopic = Config.PREPUNIT_INPUTHELP_DEPTH;

            refresh_input_help();

            refresh();
        }

        protected abstract void refresh_input_help();

        protected virtual void refresh()
        {
            // Item Window
            Item_Window = new Window_Command_Item_Preparations(
                Unit_Window.actor_id, new Vector2(8, 52), true);
            Item_Window.face_shown = false;
            Item_Window.stereoscopic = Config.PREPUNIT_UNIT_INFO_DEPTH;
            Item_Header = new Pick_Units_Items_Header(Unit_Window.actor_id, Item_Window.width);
            Item_Header.loc = Item_Window.loc - new Vector2(4, 36);
            Item_Header.stereoscopic = Config.PREPUNIT_UNIT_INFO_DEPTH;
        }

        public event EventHandler<EventArgs> Status;
        protected void OnStatus(EventArgs e)
        {
            if (Status != null)
                Status(this, e);
        }

        public event EventHandler<EventArgs> Unit;
        protected void OnUnit(EventArgs e)
        {
            if (Unit != null)
                Unit(this, e);
        }

        #region Update
        protected override void UpdateMenu(bool active)
        {
            Unit_Window.update(active && ready);

            base.UpdateMenu(active && this.ready);
        }

        protected override void UpdateAncillary()
        {
            if (Input.ControlSchemeSwitched)
                refresh_input_help();
        }

        protected virtual void Unit_Window_IndexChanged(object sender, EventArgs e)
        {
            refresh();
        }

        protected override void close()
        {
            OnClosing(new EventArgs());

            _Closing = true;
            Black_Screen_Timer = Black_Screen_Hold_Timer + (Black_Screen_Fade_Timer * 2);
            if (Black_Screen != null)
                Black_Screen.visible = true;
            Global.game_system.Preparations_Actor_Id = actor_id;
        }
        #endregion

        #region Draw
        protected virtual void draw_info(SpriteBatch sprite_batch)
        {
            Backing_1.draw(sprite_batch);
            Select.Draw(sprite_batch);
        }

        protected override void draw_window(SpriteBatch sprite_batch)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            draw_info(sprite_batch);
            sprite_batch.End();

            // Unit Window
            Unit_Window.draw(sprite_batch);

            //Item Windows
            Item_Window.draw(sprite_batch);

            // Headers
            Item_Header.draw(sprite_batch);
        }
        #endregion
    }
}
