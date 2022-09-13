using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.ConfigData;
using Tactile.Graphics.Text;
using TactileLibrary;

namespace Tactile.Graphics.UnitScreen
{
    class UnitScreenRow : Stereoscopic_Graphic_Object
    {
        const int STATUS_ICONS_AT_ONCE = 3;
        const int SUPPORT_COLUMN_SIZE = 56;

        private List<Sprite> Data;
        public UnitScreenRow(UnitScreenData[] configs, int[] pages, Game_Unit unit)
        {
            Data = new List<Sprite>();

            for (int i = 0; i < configs.Length; i++)
            {
                var config = configs[i];
                object value = config.GetValue(unit, pages[i]);
                string textColor = config.GetTextColor(unit);

                switch (config.Output)
                {
                    case UnitScreenOutput.Text:
                    default:
                        AddText(config, value, textColor);
                        break;
                    case UnitScreenOutput.TextDivisor:
                        AddTextDivisor(config, value, textColor);
                        break;
                    case UnitScreenOutput.Affinity:
                        AddAffinity(config, value);
                        break;
                    case UnitScreenOutput.Status:
                        AddStatus(config, value);
                        break;
                    case UnitScreenOutput.Inventory:
                        AddInventory(config, value);
                        break;
                    case UnitScreenOutput.Skills:
                        AddSkills(config, value);
                        break;
                    case UnitScreenOutput.Supports:
                        AddSupports(config, value, textColor);
                        break;
                }
            }
        }

        private void AddText(UnitScreenData config, object value, string textColor)
        {
            string font = Config.UI_FONT;
            if (config.LargeText)
                font += "L";

            TextSprite text;
            switch (config.Align)
            {
                case ParagraphAlign.Right:
                    text = new RightAdjustedText();
                    break;
                default:
                    text = new TextSprite();
                    break;
            }

            text.loc = new Vector2(config.Offset + config.DataOffset, 0);
            text.SetFont(font, Global.Content, textColor, Config.UI_FONT);
            text.text = value == null ? "Test" : value.ToString();

            if (config.Align == ParagraphAlign.Center)
                text.offset.X = text.text_width / 2;

            Data.Add(text);
        }

        private void AddTextDivisor(UnitScreenData config, object value, string textColor)
        {
            TextSprite text = new TextSprite();
            text.loc = new Vector2(config.Offset + config.DataOffset, 0);
            text.SetFont(Config.UI_FONT, Global.Content, "White", Config.UI_FONT);
            text.draw_offset = new Vector2(1, 0);
            text.text = "/";

            Data.Add(text);

            AddText(config, value, textColor);
        }

        private void AddAffinity(UnitScreenData config, object value)
        {
            var icon = new Icon_Sprite();
            icon.texture = Global.Content.Load<Texture2D>(@"Graphics/Icons/Affinity Icons");
            icon.size = new Vector2(16, 16);
            icon.loc = new Vector2(config.Offset + config.DataOffset, 0);
            icon.index = (int)value;

            switch (config.Align)
            {
                case ParagraphAlign.Left:
                default:
                    break;
                case ParagraphAlign.Center:
                    icon.offset.X = icon.size.X / 2;
                    break;
                case ParagraphAlign.Right:
                    icon.offset.X = icon.size.X;
                    break;
            }

            Data.Add(icon);
        }

        private void AddStatus(UnitScreenData config, object value)
        {
            var states = (List<Tuple<int, int>>)value;
            int count = Math.Min(STATUS_ICONS_AT_ONCE, states.Count);

            for (int i = 0; i < count; i++)
            {
                int offset = i * 16;

                var icon = new Status_Icon_Sprite();
                icon.texture = Global.Content.Load<Texture2D>(@"Graphics/Icons/Statuses");
                icon.size = new Vector2(16, 16);
                icon.loc = new Vector2(config.Offset + config.DataOffset + offset, 0);
                icon.index = Global.data_statuses[states[i].Item1].Image_Index;
                icon.counter = states[i].Item2;

                switch (config.Align)
                {
                    case ParagraphAlign.Left:
                    default:
                        break;
                    case ParagraphAlign.Center:
                        icon.offset.X = (icon.size.X * count) / 2;
                        break;
                    case ParagraphAlign.Right:
                        icon.offset.X = (icon.size.X * count);
                        break;
                }

                Data.Add(icon);
            }
        }

        private void AddInventory(UnitScreenData config, object value)
        {
            var inventory = (List<Item_Data>)value;

            for (int i = 0; i < inventory.Count; i++)
            {
                Data_Equipment item = inventory[i].to_equipment;
                int offset = (i * 16);

                var icon = new Icon_Sprite();
                string textureName = @"Graphics/Icons/" + item.Image_Name;
                if (Global.content_exists(textureName))
                    icon.texture = Global.Content.Load<Texture2D>(textureName);
                icon.size = new Vector2(16, 16);
                icon.loc = new Vector2(config.Offset + config.DataOffset + offset, 0);
                icon.index = item.Image_Index;

                switch (config.Align)
                {
                    case ParagraphAlign.Left:
                    default:
                        break;
                    case ParagraphAlign.Center:
                        icon.offset.X = (icon.size.X * inventory.Count) / 2;
                        break;
                    case ParagraphAlign.Right:
                        icon.offset.X = (icon.size.X * inventory.Count);
                        break;
                }

                Data.Add(icon);
            }
        }

        private void AddSkills(UnitScreenData config, object value)
        {
            var skills = (List<int>)value;

            for (int i = 0; i < skills.Count; i++)
            {
                int offset = (i * Config.SKILL_ICON_SIZE);

                var icon = new Icon_Sprite();
                string textureName = @"Graphics/Icons/" + Global.data_skills[skills[i]].Image_Name;
                if (Global.content_exists(textureName))
                    icon.texture = Global.Content.Load<Texture2D>(textureName);
                icon.size = new Vector2(Config.SKILL_ICON_SIZE, Config.SKILL_ICON_SIZE);
                icon.loc = new Vector2(config.Offset + config.DataOffset + offset, 0);
                icon.index = Global.data_skills[skills[i]].Image_Index;

                switch (config.Align)
                {
                    case ParagraphAlign.Left:
                    default:
                        break;
                    case ParagraphAlign.Center:
                        icon.offset.X = (icon.size.X * skills.Count) / 2;
                        break;
                    case ParagraphAlign.Right:
                        icon.offset.X = (icon.size.X * skills.Count);
                        break;
                }

                Data.Add(icon);
            }
        }

        private void AddSupports(UnitScreenData config, object value, string textColor)
        {
            List<Tuple<int, bool>> readySupports;
            if (value == null)
                readySupports = new List<Tuple<int, bool>>();
            else
                readySupports = (List<Tuple<int, bool>>)value;

            string font = Config.UI_FONT;
            if (config.LargeText)
                font += "L";

            for (int i = 0; i < Tactile.Windows.Map.Window_Unit.SUPPORTS_PER_PAGE; i++)
            {
                TextSprite text = new TextSprite();
                text.loc = new Vector2(config.Offset + config.DataOffset + i * SUPPORT_COLUMN_SIZE, 0);
                if (i < readySupports.Count)
                {
                    text.SetFont(font, Global.Content, readySupports[i].Item2 ? textColor : "Grey", Config.UI_FONT);
                    text.text = Global.game_actors[readySupports[i].Item1].name;
                }
                else
                {
                    text.SetFont(font, Global.Content, textColor, Config.UI_FONT);
                    text.text = "---";
                }

                Data.Add(text);
            }


            /*
            for (int i = 0; i < Tactile.Windows.Map.Window_Unit.SUPPORTS_PER_PAGE; i++)
            {
                Allies.Add(new TextSprite());
                Allies[i].loc = new Vector2(8 + 56 * i, 0);
                Allies[i].SetFont(Config.UI_FONT);
                if (ready_supports.Count > (i + start_index) && Global.map_exists &&
                        Global.game_state.is_support_blocked(unit.actor.id, ready_supports[i], true))
                    Allies[i].SetColor(Global.Content, "Grey");
                else
                    Allies[i].SetColor(Global.Content, "White");
                Allies[i].text = (ready_supports.Count > (i + start_index)) ? Global.game_actors[ready_supports[i + start_index]].name : "---";
            }*/
        }

        public void Update()
        {
            foreach (var sprite in Data)
                sprite.update();
        }

        public void Draw(SpriteBatch spriteBatch, RasterizerState rasterState, Vector2 drawOffset = default(Vector2))
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, rasterState);

            foreach (var sprite in Data)
            {
                sprite.draw(spriteBatch, drawOffset - (this.loc + this.draw_vector()));
            }

            spriteBatch.End();
        }
    }
}
