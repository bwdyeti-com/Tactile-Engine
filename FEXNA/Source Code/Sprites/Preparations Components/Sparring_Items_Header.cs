//Sparring
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEXNA.Graphics.Preparations
{
    class Sparring_Items_Header : Pick_Units_Items_Header
    {
        private Weapon_Level_Rank Weapon_Icon;

        public Sparring_Items_Header(int actor_id, int width) : base(actor_id, width) { }

        protected override void initialize_hp()
        {
            Weapon_Icon = new Weapon_Level_Rank();
            Weapon_Icon.loc = new Vector2(104, 16);
            /*Hp_Label = new FE_Text();
            Hp_Label.loc = new Vector2(112, 24);
            Hp_Label.Font = "FE7_TextL";
            Hp_Label.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Yellow");
            Hp_Label.text = "HP";*/
            /*Hp = new FE_Text_Int();
            Hp.loc = new Vector2(144, 24);
            Hp.Font = "FE7_TextL";
            Hp.texture = Global.Content.Load<Texture2D>(@"Graphics/Fonts/FE7_Text_Blue");*/
        }

        protected override void set_hp(Game_Actor actor)
        {
            FEXNA_Library.WeaponType type = actor.determine_sparring_weapon_type();
            Weapon_Icon.type = type.Key;
            Weapon_Icon.set_data(0f,
                actor.weapon_level_letter(type) ==
                    FEXNA_Library.Data_Weapon.WLVL_LETTERS[FEXNA_Library.Data_Weapon.WLVL_LETTERS.Length - 1] ? "Green" : "Blue",
                actor.weapon_level_letter(type));


            //Weapon_Icon.index = (int)type;
            //Hp.text = type == 0 ? "--" : actor.weapon_level_letter(type);// actor.maxhp.ToString();
        }

        protected override void draw_hp(SpriteBatch sprite_batch, Vector2 draw_offset)
        {
            Vector2 offset = draw_vector() - draw_offset;

            Weapon_Icon.draw(sprite_batch, -(loc + offset));
            //Hp_Label.draw(sprite_batch, -(loc + offset));
            //Hp.draw(sprite_batch, -(loc + offset));
        }
    }
}
