using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Text;

namespace Tactile.Graphics
{
    class ChapterTransitionAlternateTitle : Stereoscopic_Graphic_Object
    {
        enum LabelState { None, FadeIn, FadeOut }

        private TextSprite Label;
        private LabelState State;
        private Parchment_Box Window;

        public ChapterTransitionAlternateTitle(string label)
        {
            Label = new TextSprite(
                Config.UI_FONT, Global.Content, "HelpBlue",
                new Vector2(8, 2),
                label);
            Label.opacity = 0;
            Label.stereoscopic = Config.CH_TRANS_BANNER_DEPTH;

            Window = new Parchment_Box(
                Math.Max(72, ((Label.text_width + 7) / 8) * 8 + 16), 20);
            Window.opacity = 0;

            this.draw_offset = new Vector2(40, 0);
        }

        public void FadeIn()
        {
            State = LabelState.FadeIn;
        }

        public void FadeOut()
        {
            State = LabelState.FadeOut;
        }

        public void Update()
        {
            switch (State)
            {
                case LabelState.FadeIn:
                    if (Label.opacity < 255)
                    {
                        Label.opacity += 6;
                        Window.opacity = Label.opacity;
                    }

                    this.draw_offset.X *= 0.92f;
                    if (Math.Abs(this.draw_offset.X) < 1f)
                        this.draw_offset.X = 0;
                    break;
                case LabelState.FadeOut:
                    Label.opacity -= 8;
                    Window.opacity = Label.opacity;

                    if (Label.opacity <= 0)
                        State = LabelState.None;
                    break;
                case LabelState.None:
                default:
                    break;
            }

            Label.update();
            Window.opacity = Label.opacity;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Window.draw(spriteBatch, -(this.loc + this.draw_offset - this.offset));

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            Label.draw(spriteBatch, -(this.loc + this.draw_offset - this.offset));
            spriteBatch.End();
        }
    }
}
