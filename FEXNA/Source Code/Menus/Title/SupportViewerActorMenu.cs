using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Help;
using FEXNA.Graphics.Text;
using FEXNA.Graphics.Windows;
using FEXNA.Windows.Command;

namespace FEXNA.Menus.Title
{
    class SupportViewerActorMenu : StandardMenu
    {
        const int FACE_CLIP_BOTTOM = 0;

        private int ActorId;
        private bool AtBase;
        private Button_Description FieldBaseButton;
        private WindowCommandSupportViewerActor Window;
        private Face_Sprite Face;
        private Prepartions_Item_Window FaceWindow;
        private Sprite NameBanner;
        private FE_Text Name;
        private Menu_Background Background;

        public SupportViewerActorMenu(int actorId, IHasCancelButton menu) : base(menu)
        {
            ActorId = actorId;
            Window = new WindowCommandSupportViewerActor(
                ActorId,
                new Vector2(Config.WINDOW_WIDTH - 152, 16));

            // Background
            Background = new Menu_Background();
            Background.texture = Global.Content.Load<Texture2D>(
                @"Graphics\Pictures\Preparation_Background");
            (Background as Menu_Background).vel = new Vector2(0, -1 / 3f);
            (Background as Menu_Background).tile = new Vector2(1, 2);
            Background.stereoscopic = Config.MAPMENU_BG_DEPTH;
            // Face Window
            FaceWindow = new Prepartions_Item_Window(false);
            FaceWindow.width = 128;
            FaceWindow.height = 96;
            FaceWindow.loc = new Vector2(24, 16);
            FaceWindow.color_override = 0;
            // Face
            string filename = "";
            if (Global.data_actors.ContainsKey(ActorId))
                filename = Global.data_actors[ActorId].Name;
            Face = new Face_Sprite(filename);
            Face.convo_placement_offset = true;
            Face.loc = FaceWindow.loc + new Vector2(
                FaceWindow.width / 2, FaceWindow.height + FACE_CLIP_BOTTOM - 2);
            Face.mirrored = true;
            if (Global.face_data.ContainsKey(filename))
                Face.expression = Global.face_data[filename].StatusFrame;
            Face.blink(3);
            Face.phase_in();
            // Name Banner
            NameBanner = new Sprite(
                Global.Content.Load<Texture2D>(@"Graphics/Pictures/Portrait_bg"));
            NameBanner.src_rect = new Rectangle(0, 114 + 25 * 0, 93, 25);
            NameBanner.loc = FaceWindow.loc + new Vector2(FaceWindow.width / 2 - 48, FaceWindow.height);
            NameBanner.draw_offset = new Vector2(2, 0);
            // Name
            Name = new FE_Text(
                "FE7_Text",
                Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_White"),
                NameBanner.loc + new Vector2(48, 4),
                Global.data_actors[ActorId].Name.Split(Constants.Actor.ACTOR_NAME_DELIMITER)[0]);
            Name.offset.X = (int)System.Math.Ceiling(Name.text_width / 2f);
        }

        #region StandardMenu Abstract
        public override int Index { get { return Window.index; } }

        protected override bool SelectedTriggered(bool active)
        {
            if (!active)
                return false;
            
            return Window.is_selected();
        }

        protected override void UpdateStandardMenu(bool active)
        {
            // Needed to animate map sprites
            if (this.HidesParent)
                Global.game_system.update_timers();

            Window.update(active);

            Background.update();
            FaceWindow.update();
            Face.update();
            NameBanner.update();
            Name.update();

            // Switch field/base support mode
            if (FieldBaseButton != null)
                FieldBaseButton.Update(active);

            bool switchField = active && Global.Input.triggered(Inputs.X);
            if (FieldBaseButton != null)
            {
                switchField |= FieldBaseButton.consume_trigger(MouseButtons.Left) ||
                    FieldBaseButton.consume_trigger(TouchGestures.Tap);
            }
            if (switchField)
                OnFieldBaseSwitched(new EventArgs());
        }

        protected override void UpdateAncillary()
        {
            base.UpdateAncillary();

            if (Input.ControlSchemeSwitched)
                SetFieldBase(AtBase);
        }

        #endregion

        public void SetFieldBase(bool atBase)
        {
            AtBase = atBase;

            Window.SetAtBase(AtBase);

            FieldBaseButton = Button_Description.button(Inputs.X, 80);
            FieldBaseButton.description = AtBase ? "Base" : "Field";
            FieldBaseButton.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
        }

        public event EventHandler<EventArgs> FieldBaseSwitched;
        protected void OnFieldBaseSwitched(EventArgs e)
        {
            if (FieldBaseSwitched != null)
                FieldBaseSwitched(this, e);
        }

        protected override bool CanceledTriggered(bool active)
        {
            bool cancel = Window.is_canceled() || base.CanceledTriggered(active);
            if (active)
            {
                cancel |= Global.Input.triggered(Inputs.B);
                cancel |= Global.Input.mouse_click(MouseButtons.Right);
            }
            return cancel;
        }

        protected override bool IsActive(bool active)
        {
            return base.IsActive(active) && !Global.scene.is_message_window_active;
        }

        internal bool GetSupportConvo(out string key, out int level)
        {
            key = Window.SelectedKey;
            level = Window.SelectedLevel;
            if (!Global.data_supports.ContainsKey(key) || level >= Global.data_supports[key].MaxLevel)
                return false;

            return true;
        }
        
        #region IMenu
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (DataDisplayed)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Background.draw(spriteBatch);
                FaceWindow.draw(spriteBatch);
                NameBanner.draw(spriteBatch);
                Name.draw(spriteBatch);
                spriteBatch.End();

                Rectangle faceClip = new Rectangle(
                    (int)Face.loc.X + 8 - (FaceWindow.width / 2),
                    (int)Face.loc.Y - (120 + FACE_CLIP_BOTTOM),
                    FaceWindow.width - 16, 120);
                Face.draw(spriteBatch, Vector2.Zero, faceClip);

                Window.draw(spriteBatch);

                if (FieldBaseButton != null && DataDisplayed)
                {
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                    FieldBaseButton.Draw(spriteBatch);
                    spriteBatch.End();
                }

                base.Draw(spriteBatch);
            }
        }
        #endregion
    }
}
