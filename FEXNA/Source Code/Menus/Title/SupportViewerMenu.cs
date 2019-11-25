using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Windows;
using FEXNA.Windows.Title;

namespace FEXNA.Menus.Title
{
    class SupportViewerMenu : StandardMenu
    {
        const int FACE_CLIP_BOTTOM = 16;
        const string AUGURY_FACE = "Niime";

        private WindowSupportViewerActorList Window;
        private Face_Sprite AuguryFace;
        private Prepartions_Item_Window FaceWindow;
        private Text_Window TextBox;
        private Menu_Background Background;

        public SupportViewerMenu() : base()
        {
            // Window
            Window = new WindowSupportViewerActorList();
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
            FaceWindow.height = 72;
            FaceWindow.loc = new Vector2(24, 8);
            FaceWindow.color_override = 0;
            // Face
            AuguryFace = new Face_Sprite(AUGURY_FACE);
            AuguryFace.convo_placement_offset = true;
            AuguryFace.loc = FaceWindow.loc + new Vector2(
                FaceWindow.width / 2, FaceWindow.height + FACE_CLIP_BOTTOM - 2);
            AuguryFace.mirrored = true;
            AuguryFace.expression = 1;
            AuguryFace.blink(3);
            AuguryFace.phase_in();
            // Text Box
            TextBox = new Text_Window(152, 48);
            TextBox.loc = FaceWindow.loc + new Vector2(112, 16);
            TextBox.SetSpeakerArrow(24, SpeakerArrowSide.Left);
            TextBox.opacity = 255;
            TextBox.text_set("Select whose conversation\nyou want to read.");
        }

        #region StandardMenu Abstract
        public override int Index
        {
            get { return Window.index; }
        }

        protected override bool SelectedTriggered(bool active)
        {
            if (!active)
                return false;

            bool selected = Window.consume_triggered(Inputs.A, MouseButtons.Left, TouchGestures.Tap).IsSomething;
            return selected;
        }

        protected override void UpdateStandardMenu(bool active)
        {
            // Needed to animate map sprites
            Global.game_system.update_timers();

            Window.update(active);
            Background.update();
            FaceWindow.update();
            AuguryFace.update();
            TextBox.update();
        }
        #endregion
        
        public int ActorId { get { return Window.actor_id; } }
        
        protected override int DefaultCancelPosition { get { return 16; } }

        protected override bool CanceledTriggered(bool active)
        {
            bool cancel = base.CanceledTriggered(active);
            if (active)
            {
                cancel |= Global.Input.triggered(Inputs.B);
                cancel |= Global.Input.mouse_click(MouseButtons.Right);
            }
            return cancel;
        }

        #region IMenu
        public override bool HidesParent { get { return DataDisplayed; } }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (DataDisplayed)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Background.draw(spriteBatch);
                FaceWindow.draw(spriteBatch);
                spriteBatch.End();
                
                Rectangle faceClip = new Rectangle(
                    (int)AuguryFace.loc.X + 8 - (FaceWindow.width / 2),
                    (int)AuguryFace.loc.Y - (96 + FACE_CLIP_BOTTOM),
                    FaceWindow.width - 16, 96);
                AuguryFace.draw(spriteBatch, Vector2.Zero, faceClip);

                TextBox.draw(spriteBatch);

                Window.draw(spriteBatch);
            }

            base.Draw(spriteBatch);
        }
        #endregion
    }
}
