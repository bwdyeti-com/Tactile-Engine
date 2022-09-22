using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Help;
using Tactile.Menus.Preparations;
using Tactile.Windows.Command;
using TactileLibrary;

namespace Tactile
{
    class Window_Home_Base : Window_Setup
    {
        public bool talk_events_exist { get { return Global.game_state.has_ready_base_events(); } }

        protected override Vector2 command_window_loc { get { return new Vector2(8, 40); } }

        internal override ConsumedInput selected_index
        {
            get { return CommandWindow.selected_index(); }
        }

        public Window_Home_Base() : base(false)
        {
            Background.texture = Global.Content.Load<Texture2D>(@"Graphics/Panoramas/" + Global.game_system.home_base_background);
            (Background as Menu_Background).vel = new Vector2(0, 0);
            (Background as Menu_Background).tile = new Vector2(0, 0);
        }

        protected override void initialize_sprites()
        {
            base.initialize_sprites();
            refresh_talk_ready();
            CommandWindow.set_text_color(2, "Grey");
            Banner.src_rect = new Rectangle(0, 216, 320, 32);
            InfoWindow.loc = new Vector2(8, 128 + 32);
            InfoWindow.src_rect = new Rectangle(0, 176, 112, 32);
        }

        protected override void create_start_button()
        {
            StartButton = Button_Description.button(Inputs.Start,
                Config.WINDOW_WIDTH - 72);
            StartButton.description = "Leave";
            StartButton.stereoscopic = Config.PREPMAIN_INFO_DEPTH;
        }

        public void refresh_talk_ready()
        {
            if (!talk_events_exist)
                CommandWindow.set_text_color(1, "Grey");
            else
                CommandWindow.set_text_color(1, "White");
            if ((HomeBaseChoices)CommandWindow.index == HomeBaseChoices.Talk)
                refresh_text();
        }

        protected override List<string> command_window_string()
        {
            return new List<string> { "Manage", "Talk", "Codex", "Options", "Save" };
        }

        public override void refresh()
        {
            Goal.text = Global.game_system.Objective_Text;
            Goal.offset = new Vector2(Font_Data.text_width(Goal.text) / 2, 0);
            AvgLvl.loc = new Vector2(108, 136);
            AvgLvl.text = (Global.battalion.deployed_average_level / Global.ActorConfig.ExpToLvl).ToString();
            Funds.text = Global.battalion.gold.ToString();
        }

        protected override void refresh_text()
        {
            switch ((HomeBaseChoices)CommandWindow.index)
            {
                // Manage
                case HomeBaseChoices.Manage:
                    HelpText.text = Global.system_text["Prep Manage"];
                    break;
                // Talk
                case HomeBaseChoices.Talk:
                    if (talk_events_exist)
                        HelpText.text = Global.system_text["Prep Talk"];
                    else
                        HelpText.text = Global.system_text["Prep Disabled"];
                    break;
                // Codex
                case HomeBaseChoices.Codex:
                    HelpText.text = Global.system_text["Prep Disabled"];
                    break;
                // Options
                case HomeBaseChoices.Options:
                    HelpText.text = Global.system_text["Prep Options"];
                    break;
                // Save
                case HomeBaseChoices.Save:
                    HelpText.text = Global.system_text["Prep Save"];
                    break;
            }
        }
        internal void refresh_manage_text(HomeBaseManageChoices choice)
        {
            switch (choice)
            {
                // Organize
                case HomeBaseManageChoices.Organize:
                    HelpText.text = Global.system_text["Prep Organize"];
                    break;
                // Items
                case HomeBaseManageChoices.Trade:
                    HelpText.text = Global.system_text["Prep Items"];
                    break;
                // Support
                case HomeBaseManageChoices.Support:
                    HelpText.text = Global.system_text["Prep Support"];
                    break;
            }
        }

        protected override void Activate()
        {
            CommandWindow.greyed_cursor = false;
            CommandWindow.active = true;
            refresh_text();
        }
        protected override void Deactivate()
        {
            CommandWindow.greyed_cursor = true;
            CommandWindow.active = false;
        }

        public Vector2 SelectedOptionLocation
        {
            get
            {
                return CommandWindow.loc + new Vector2(0,
                    (CommandWindow.selected_index().Index) * 16);
            }
        }

        protected override void command_window_canceled() { }

        protected override void draw_background(SpriteBatch sprite_batch)
        {
            if (Background != null)
            {
                Effect effect = Global.effect_shader();
                if (effect != null)
                {
                    effect.CurrentTechnique = effect.Techniques["Tone"];
                    effect.Parameters["tone"].SetValue(Global.game_state.screen_tone.to_vector_4(1.0f));
                }
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, effect);
                Background.draw(sprite_batch);
                sprite_batch.End();
            }
        }

        protected override void draw_info(SpriteBatch sprite_batch)
        {
            Counter.draw(sprite_batch);
        }
    }
}
