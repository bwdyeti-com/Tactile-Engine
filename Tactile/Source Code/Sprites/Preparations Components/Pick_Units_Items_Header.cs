using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile.Graphics.Preparations
{
    class Pick_Units_Items_Header : Sprite
    {
        const string FILENAME = @"Graphics/Windowskins/Pick_Units_Components";
        protected int Width;
        protected Miniface Face;
        protected Icon_Sprite Affinity_Icon;
        protected TextSprite Name;
        protected TextSprite Lvl_Label, Exp_Label, Hp_Label;
        protected RightAdjustedText Lvl, Exp, Hp;

        public Pick_Units_Items_Header(int actor_id, int width)
        {
            texture = Global.Content.Load<Texture2D>(FILENAME);
            initialize_sprites();
            set_actor(actor_id);
            Width = width;
        }

        protected void initialize_sprites()
        {
            Face = new Miniface();
            Face.loc = new Vector2(24, 8);
            Affinity_Icon = new Icon_Sprite();
            Affinity_Icon.texture = Global.Content.Load<Texture2D>(@"Graphics/Icons/Affinity Icons");
            Affinity_Icon.size = new Vector2(16, 16);
            Affinity_Icon.loc = new Vector2(96, 8);
            Name = new TextSprite();
            Name.loc = new Vector2(96, 8);
            Name.SetFont(Config.UI_FONT, Global.Content, "White");
            Lvl_Label = new TextSprite();
            Lvl_Label.loc = new Vector2(40, 24);
            Lvl_Label.SetFont(Config.UI_FONT + "L", Global.Content, "Yellow", Config.UI_FONT);
            Lvl_Label.text = "LV";
            Exp_Label = new TextSprite();
            Exp_Label.loc = new Vector2(80, 24);
            Exp_Label.SetFont(Config.UI_FONT + "L", Global.Content, "Yellow", Config.UI_FONT);
            Exp_Label.text = "E";
            Lvl = new RightAdjustedText();
            Lvl.loc = new Vector2(72, 24);
            Lvl.SetFont(Config.UI_FONT, Global.Content, "Blue");
            Exp = new RightAdjustedText();
            Exp.loc = new Vector2(104, 24);
            Exp.SetFont(Config.UI_FONT, Global.Content, "Blue");

            initialize_hp();
        }

        protected virtual void initialize_hp()
        {
            Hp_Label = new TextSprite();
            Hp_Label.loc = new Vector2(112, 24);
            Hp_Label.SetFont(Config.UI_FONT + "L", Global.Content, "Yellow", Config.UI_FONT);
            Hp_Label.text = "HP";
            Hp = new RightAdjustedText();
            Hp.loc = new Vector2(144, 24);
            Hp.SetFont(Config.UI_FONT, Global.Content, "Blue");
        }

        public void set_actor(int actor_id)
        {
            Game_Actor actor = Global.game_actors[actor_id];
            Face.set_actor(actor);
            Affinity_Icon.index = (int)actor.affin;
            Name.text = actor.name;
            Name.offset = new Vector2(Font_Data.text_width(Name.text) / 2, 0);
            Affinity_Icon.offset = new Vector2(Name.offset.X + 16, 0);
            Lvl.text = actor.level.ToString();
            Exp.text = actor.can_level() ? actor.exp.ToString() : "--";

            set_hp(actor);
        }

        protected virtual void set_hp(Game_Actor actor)
        {
            Hp.text = actor.maxhp.ToString();
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            if (texture != null)
                if (visible)
                {
                    Vector2 offset = draw_vector() - draw_offset;

                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                    int x = 0;
                    // Left
                    sprite_batch.Draw(texture, (loc + offset + new Vector2(x, 0)),
                        new Rectangle(32, 0, 48, 41), tint, angle, this.offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    x += 48;
                    // Center
                    while (x + 16 <= Width)
                    {
                        sprite_batch.Draw(texture, (loc + offset + new Vector2(x, 0)),
                            new Rectangle(80, 0, 8, 41), tint, angle, this.offset, scale,
                            mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                        x += 8;
                    }
                    // Right
                    sprite_batch.Draw(texture, (loc + offset + new Vector2(x, 0)),
                        new Rectangle(88, 0, 16, 41), tint, angle, this.offset, scale,
                        mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Z);
                    sprite_batch.End();
                    // Miniface
                    Face.draw(sprite_batch, -(loc + offset));

                    sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                    // Affinity
                    Affinity_Icon.draw(sprite_batch, -(loc + offset));
                    // Labels
                    Lvl_Label.draw(sprite_batch, -(loc + offset));
                    Exp_Label.draw(sprite_batch, -(loc + offset));
                    // Data
                    Name.draw(sprite_batch, -(loc + offset));
                    Lvl.draw(sprite_batch, -(loc + offset));
                    Exp.draw(sprite_batch, -(loc + offset));

                    draw_hp(sprite_batch, draw_offset);
                    sprite_batch.End();
                }
        }

        protected virtual void draw_hp(SpriteBatch sprite_batch, Vector2 draw_offset)
        {
            Vector2 offset = draw_vector() - draw_offset;

            Hp_Label.draw(sprite_batch, -(loc + offset));
            Hp.draw(sprite_batch, -(loc + offset));
        }
    }
}
