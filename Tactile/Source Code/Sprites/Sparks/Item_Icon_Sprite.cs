using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactile
{
    class Item_Icon_Sprite : Icon_Sprite
    {
        protected bool Flash = false;
        protected int Flash_Timer;
        protected int Flash_Time_Max = 60;
        protected Color Flash_Color = Color.White;
        protected bool Scissor = false;
        protected static RasterizerState Scissor_State = new RasterizerState { ScissorTestEnable = true };

        #region Accessors
        public override Texture2D texture
        {
            get { return Texture; }
            set
            {
                Texture = value;
                columns = Texture == null ? 1 : (int)(Texture.Width / size.X);
            }
        }

        public bool flash { set { Flash = value; } }

        public int flash_time_max { set { Flash_Time_Max = value; } }

        public Color flash_color { set { Flash_Color = value; } }

        public bool scissor { set { Scissor = value; } }
        #endregion

        public Item_Icon_Sprite()
        {
            size = new Vector2(Config.ITEM_ICON_SIZE, Config.ITEM_ICON_SIZE);
        }

        public override void update()
        {
            base.update();
            Flash_Timer = (Flash_Timer + 1) % Flash_Time_Max;
        }

        public override void draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            Effect icon_shader = Global.effect_shader();
            if (icon_shader != null)
            {
                icon_shader.CurrentTechnique = icon_shader.Techniques["Technique1"];
                icon_shader.Parameters["color_shift"].SetValue(new Color(Flash_Color.R, Flash_Color.G, Flash_Color.B, Flash ?
                    (Flash_Timer <= (Flash_Time_Max / 2) ? Flash_Timer : Flash_Time_Max - Flash_Timer) * 255 / (Flash_Time_Max / 2) : 0).ToVector4());
                icon_shader.Parameters["opacity"].SetValue(1f);
            }
            if (Scissor)
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, Scissor_State, icon_shader);
            else
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, icon_shader);
            base.draw(sprite_batch, draw_offset);
            sprite_batch.End();
        }
    }
}
