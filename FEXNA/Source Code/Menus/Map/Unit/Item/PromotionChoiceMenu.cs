using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA.Windows.Command;

namespace FEXNA.Menus.Map.Unit.Item
{
    class PromotionChoiceMenu : CommandMenu, IFadeMenu, IHasCancelButton
    {
        const int BLACK_SCEEN_FADE_IN_TIMER = 16;
        const int BLACK_SCEEN_FADE_OUT_TIMER = 16;
        const int BLACK_SCREEN_FADE_IN_HOLD_TIMER = 8;
        const int BLACK_SCREEN_FADE_OUT_HOLD_TIMER = 4;

        const int WIDTH = 88;
        const int MAX_ROWS = 7;
        internal const int BATTLER_X = 240;

        private int UnitId;
        private bool DataDisplayed = false;
        private bool Closing = false;
        private Vector2 BattlerOffset = Vector2.Zero;
        private bool ChangingSprite = false;
        private bool Cancellable;
        public bool AnimatedConfirm { get; private set; }
        private bool Confirming;

        private Battler_Sprite Battler;
        private Battle_Platform Platform;
        private Message_Box Message;
        private Sprite TextBg;
        private FE_Text Name, NameBg;
        private Sprite Background;

        internal Game_Unit Unit { get { return Global.game_map.units[UnitId]; } }

        public PromotionChoiceMenu(Game_Unit unit, bool standaloneMenu = false, bool animateConfirm = false)
            : base(NewWindow(unit), null)
        {
            UnitId = unit.id;
            Cancellable = !standaloneMenu;
            AnimatedConfirm = animateConfirm;

            SetSprites();
            ChangeBattler();

            if (Cancellable)
            {
                CreateCancelButton(
                    32,
                    Config.MAPCOMMAND_WINDOW_DEPTH);
            }
            else
                CancelButton = null;
        }

        private static Window_Command NewWindow(Game_Unit unit)
        {
            List<string> commands = new List<string>();
            foreach (int classId in PromotionChoices(unit))
                commands.Add(Global.data_classes[classId].name);
            
            Window_Command window;
            if (commands.Count <= MAX_ROWS)
                window = new Window_Command(new Vector2(24, 8), WIDTH, commands);
            else
                window = new Window_Command_Scrollbar(new Vector2(24, 8), WIDTH + 8, MAX_ROWS, commands);
            window.text_offset = new Vector2(4, 0);
            window.stereoscopic = Config.MAPCOMMAND_WINDOW_DEPTH;
            window.help_stereoscopic = Config.MAPCOMMAND_HELP_DEPTH;
            return window;
        }

        private void SetSprites()
        {
            BattlerOffset = new Vector2(this.OffscreenOffset, 0);

            // Platform
            Platform = new Battle_Platform(false);
            Platform.loc_1 = new Vector2((BATTLER_X - 123) + 127 - 87, 88);
            Platform.loc_2 = new Vector2((BATTLER_X - 123) + 127, 88);

            int terrainTag = this.Unit.terrain_id();
            string terrainName;
            if (Global.data_terrains[terrainTag].Platform_Rename.Length > 0)
                terrainName = Global.data_terrains[terrainTag].Platform_Rename;
            else
                terrainName = Global.data_terrains[terrainTag].Name;

            string platformName;
            if (Global.game_system.preparations)
                platformName = @"Graphics/Battlebacks/" + "Floor" + "-Melee"; //@Debug
            else if (Global.content_exists(@"Graphics/Battlebacks/" + terrainName + "-Melee"))
                platformName = @"Graphics/Battlebacks/" + terrainName + "-Melee";
            else
                platformName = @"Graphics/Battlebacks/" + "Plains" + "-Melee";
            Platform.platform_1 = Global.Content.Load<Texture2D>(platformName);
            Platform.platform_2 = Platform.platform_1;

            // Text Box
            Message = new Message_Box(88, 144, 224, 2, false, "White");
            Message.text_speed = Constants.Message_Speeds.Max;
            Message.silence();
            Message.block_skip();
            Message.stereoscopic = Config.REEL_TEXT_BOX_DEPTH;

            TextBg = new Sprite(Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Class_Reel_Window"));
            TextBg.loc = new Vector2(80, 144);
            TextBg.stereoscopic = Config.REEL_TEXT_BOX_DEPTH;

            // Name
            Name = new FE_Text();
            Name.loc = new Vector2(128, 8);
            Name.Font = "FE7_Reel2";
            Name.visible = false;
            Name.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Reel2");
            Name.stereoscopic = Config.REEL_CLASS_NAME_DEPTH;
            NameBg = new FE_Text();
            NameBg.loc = new Vector2(128, 8);
            NameBg.draw_offset = new Vector2(2, 2);
            NameBg.Font = "FE7_Reel2";
            NameBg.visible = false;
            NameBg.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Reel2");
            NameBg.tint = new Color(0, 0, 0, 255);
            NameBg.stereoscopic = Config.REEL_CLASS_NAME_SHADOW_DEPTH;

            if (Global.game_system.preparations && !Global.game_state.battle_active)
            {
                // Background
                Background = new Menu_Background();
                Background.texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Preparation_Background");
                (Background as Menu_Background).vel = new Vector2(0, -1 / 3f);
                (Background as Menu_Background).tile = new Vector2(1, 2);
                Background.stereoscopic = Config.PREP_BG_DEPTH;
            }
        }

        private static List<int> PromotionChoices(Game_Unit unit)
        {
            return unit.actor.actor_class.Promotion.Keys.ToList();
        }

        private void ChangeBattler()
        {
            ChangingSprite = true;

            int promotionClassId = this.PromotionChoice;
            var promotionClass = Global.data_classes[promotionClassId];

            // Name
            Name.text = promotionClass.name;
            NameBg.text = Name.text;

            // Description
            string description;
            if (Class_Reel_Data.DATA.Any(x => x.Value.Class_Id == promotionClassId))
                description = Class_Reel_Data.DATA
                    .First(x => x.Value.Class_Id == promotionClassId).Value.Description;
            else
                description = promotionClass.Description;
            Message.set_text("" + description);
        }

        private void RefreshBattler()
        {
            ChangingSprite = false;
            Name.visible = true;
            NameBg.visible = true;

            int promotionClassId = this.PromotionChoice;
            var battlerData = new BattlerSpriteData(this.Unit, promotionClassId);

            Battler = new Battler_Sprite(battlerData, true, 1);
            Battler.loc = new Vector2(BATTLER_X, 176);
            Battler.offset.Y = 120;
            Battler.visible = true;
            Battler.initialize_animation();
            Battler.reset_pose();
            Battler.start_battle();
            Battler.stereoscopic = Config.BATTLE_BATTLERS_DEPTH;
        }

        private int OffscreenOffset { get { return Config.WINDOW_WIDTH - (BATTLER_X - 88); } }

        public int PromotionChoice { get { return PromotionChoices(this.Unit)[Window.index]; } }

        public void AnimateConfirmation()
        {
            Confirming = true;
        }

        public event EventHandler<EventArgs> Opened;
        protected void OnOpened(EventArgs e)
        {
            if (Opened != null)
                Opened(this, e);
        }

        public event EventHandler<EventArgs> Closed;
        protected void OnClosed(EventArgs e)
        {
            if (Closed != null)
                Closed(this, e);
        }

        public event EventHandler<EventArgs> Confirmed;
        protected void OnConfirmed(EventArgs e)
        {
            if (Confirmed != null)
                Confirmed(this, e);
        }

        #region IFadeMenu
        public void FadeShow()
        {
            //@Debug: Global.game_system.play_se(System_Sounds.Open); // maybe not
            Global.game_map.HideUnits();
            DataDisplayed = true;
        }
        public void FadeHide()
        {
            Global.game_map.ShowUnits();
            DataDisplayed = false;
        }

        public ScreenFadeMenu FadeInMenu(bool skipFadeIn)
        {
            return new ScreenFadeMenu(
                skipFadeIn ? 0 : BLACK_SCEEN_FADE_IN_TIMER,
                BLACK_SCREEN_FADE_IN_HOLD_TIMER,
                BLACK_SCEEN_FADE_IN_TIMER,
                true,
                this);
        }
        public ScreenFadeMenu FadeOutMenu()
        {
            return new ScreenFadeMenu(
                BLACK_SCEEN_FADE_OUT_TIMER,
                BLACK_SCREEN_FADE_OUT_HOLD_TIMER,
                false,
                this);
        }

        public void FadeOpen()
        {
            Global.game_system.play_se(System_Sounds.Open);
            OnOpened(new EventArgs());
        }
        public void FadeClose()
        {
            OnClosed(new EventArgs());
        }
        #endregion

        #region IMenu
        public override bool HidesParent
        {
            get
            {
                if (Global.game_system.preparations)
                    return false;
                return DataDisplayed || !Closing;
            }
        }

        protected override void UpdateMenu(bool active)
        {
            active &= DataDisplayed;

            Message.update();
            if (Background != null)
                Background.update();
            if (Battler != null)
                Battler.update();

            if (active)
            {
                // Slide offscreen
                if (Confirming || ChangingSprite)
                {
                    if (BattlerOffset.X >= this.OffscreenOffset)
                    {
                        if (Confirming)
                            OnConfirmed(new EventArgs());
                        else
                            RefreshBattler();
                    }
                    else
                        BattlerOffset.X = Additional_Math.int_closer(
                            (int)BattlerOffset.X, this.OffscreenOffset, 12);
                }
                // Slide onscreen
                else
                {
                    BattlerOffset.X = Additional_Math.int_closer(
                        (int)BattlerOffset.X, 0, 12);
                }
            }

            active &= !Confirming;

            int index = Window.index;
            base.UpdateMenu(active);
            if (index != Window.index)
            {
                ChangeBattler();
            }
        }

        protected override void SelectItem(bool playConfirmSound)
        {
            if (BattlerOffset.X == 0)
                base.SelectItem();
        }

        protected override void Cancel()
        {
            if (!Cancellable)
                Global.game_system.play_se(System_Sounds.Buzzer);
            else
            {
                base.Cancel();
                Closing = true;
            }
        }

        public override void Draw(
            SpriteBatch spriteBatch,
            GraphicsDevice device,
            RenderTarget2D[] renderTargets)
        {
            if (DataDisplayed)
            {
                if (Background != null)
                {
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    Background.draw(spriteBatch);
                    spriteBatch.End();
                }

                Platform.draw(spriteBatch, -BattlerOffset, -BattlerOffset);

                if (!Confirming)
                {
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    NameBg.draw(spriteBatch);
                    spriteBatch.End();
                }

                DrawBattler(spriteBatch, device,
                    renderTargets[0], renderTargets[2], renderTargets[3]);

                if (!Confirming)
                {
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    TextBg.draw(spriteBatch);
                    Name.draw(spriteBatch);
                    spriteBatch.End();

                    // Message
                    Message.draw_background(spriteBatch);
                    Message.draw_faces(spriteBatch);
                    Message.draw_foreground(spriteBatch);

                    base.Draw(spriteBatch, device, renderTargets);
                }
            }
        }

        private void DrawBattler(
            SpriteBatch spriteBatch,
            GraphicsDevice device,
            RenderTarget2D finalRender,
            RenderTarget2D tempRender,
            RenderTarget2D effectRender)
        {
            if (Battler != null)
            {
                BattleSpriteRenderer battlerRenderer = new BattleSpriteRenderer(
                false, -BattlerOffset, Vector2.Zero, Vector2.Zero);
                battlerRenderer.draw(spriteBatch, device,
                    new Tuple<Battler_Sprite, bool>(Battler, false), null,
                    finalRender, tempRender, effectRender);
            }
        }
        #endregion
    }
}
