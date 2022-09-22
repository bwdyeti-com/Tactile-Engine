using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Windows.Command;

namespace Tactile.Menus.Options
{
    class SettingsTopMenu : StandardMenu //@Yeti: should probably be a CommandMenu, make that descend from StandardMenu
    {
        protected Window_Command Window;
        private Menu_Background Background;

        public SettingsTopMenu() : base()
        {
            Vector2 loc = new Vector2(Config.WINDOW_WIDTH / 2 - 40, 32);
            List<string> settings = GetSettingsStrings();

            // Window
            Window = new Window_Command(loc, 72, settings);
            Window.text_offset = new Vector2(8, 0);
            Window.glow = true;
            Window.bar_offset = new Vector2(-8, 0);
            Window.WindowVisible = false;
            Window.stereoscopic = Config.PREPMAIN_WINDOW_DEPTH;

            // Background
            Background = new Menu_Background();
            Background.texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Status_Background");
            Background.vel = new Vector2(-0.25f, 0);
            Background.tile = new Vector2(3, 2);
            Background.tint = new Color(128, 128, 128, 255);
            Background.stereoscopic = Config.MAPMENU_BG_DEPTH;
        }

        protected virtual List<string> GetSettingsStrings()
        {
            List<string> settings = new List<string> { "Graphics", "Audio", "Controls" };
            if (Global.gameSettings.General.AnyValidSettings)
                settings.Insert(0, "General");
            return settings;
        }

        #region StandardMenu Abstract
        public override int Index
        {
            get
            {
                int index = Window.index;
                if (!Global.gameSettings.General.AnyValidSettings && index >= 0)
                    index++;
                return index;
            }
        }

        protected override bool SelectedTriggered(bool active)
        {
            if (!active)
                return false;

            bool selected = Window.is_selected();
            selected |= Global.Input.KeyPressed(Microsoft.Xna.Framework.Input.Keys.Enter);
            return selected;
        }

        protected override void UpdateStandardMenu(bool active)
        {
            Window.update(active);
            Background.update();
        }
        #endregion

        protected override int DefaultCancelPosition { get { return Config.WINDOW_WIDTH - 64; } }

        protected override bool CanceledTriggered(bool active)
        {
            bool cancel = base.CanceledTriggered(active);
            if (active)
            {
                cancel |= Window.is_canceled();
                cancel |= Global.Input.mouse_click(MouseButtons.Right);
            }
            return cancel;
        }

        protected override void Activate()
        {
            base.Activate();
            Window.visible = true;
        }

        #region IMenu
        protected override void SelectItem(bool playConfirmSound = false)
        {
            base.SelectItem(playConfirmSound);
            Window.visible = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (DataDisplayed)
                DrawData(spriteBatch);

            base.Draw(spriteBatch);
        }
        #endregion

        protected virtual void DrawData(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Background.draw(spriteBatch);
            spriteBatch.End();

            Window.draw(spriteBatch);
        }
    }
}
