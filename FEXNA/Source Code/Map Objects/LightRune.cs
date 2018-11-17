using System;
using Microsoft.Xna.Framework;
using System.IO;
using System.Linq;
using FEXNAVector2Extension;
using FEXNAVersionExtension;

namespace FEXNA
{
    internal class LightRune : Combat_Map_Object
    {
        const int MAX_HP = 50;
        const int TURN_DAMAGE = 10;

        protected int Team;
        protected int MaxHp = MAX_HP, Hp = MAX_HP;
        protected int Def = 0;
        protected bool NoDegrading;

        #region Serialization
        public void write(BinaryWriter writer)
        {
            writer.Write(Id);
            Loc.write(writer);
            Real_Loc.write(writer);
            writer.Write(Facing);
            writer.Write(Frame);

            writer.Write(Team);
            writer.Write(MaxHp);
            writer.Write(Hp);
            writer.Write(Def);
            writer.Write(NoDegrading);
        }

        public void read(BinaryReader reader)
        {
            Id = reader.ReadInt32();
            Loc = Loc.read(reader);
            Real_Loc = Real_Loc.read(reader);
            Facing = reader.ReadInt32();
            Frame = reader.ReadInt32();

            Team = reader.ReadInt32();
            MaxHp = reader.ReadInt32();
            Hp = reader.ReadInt32();
            Def = reader.ReadInt32();
            if (!Global.LOADED_VERSION.older_than(0, 6, 2, 1)) // This is a suspend load, so this isn't needed for public release //Debug
                NoDegrading = reader.ReadBoolean();
        }
        #endregion

        #region Accessors
        public override string name { get { return "Light Rune"; } }

        public override int maxhp { get { return MaxHp; } }
        public override int hp
        {
            get { return Hp; }
            set { Hp = Math.Min(MaxHp, value); }
        }
        public override bool is_dead { get { return hp <= 0; } }

        public override int def { get { return Def; } }

        public override int team { get { return Team; } }
        #endregion

        public LightRune() { }
        public LightRune(int id, Vector2 loc, int team, bool degrades = true)
        {
            Id = id;
            force_loc(loc);

            Team = team;
            MaxHp = Hp = MAX_HP;
            Facing = 2;
            NoDegrading = !degrades;

            init_sprites();
        }

        public void update()
        {
            update_frame();
        }

        protected void update_frame()
        {
            Frame = Global.game_system.unit_anim_idle_frame;
        }

        public void new_turn()
        {
            if (!NoDegrading)
                Hp -= TURN_DAMAGE;
        }

        public override bool is_attackable_team(int other_team)
        {
            foreach (int[] group in Constants.Team.TEAM_GROUPS)
                if (group.Contains(Team))
                    if (group.Contains(other_team))
                        return false;
            return true;
        }

        #region Sprite Handling
        public virtual void init_sprites()
        {
            ((Scene_Map)Global.scene).add_map_sprite(Id);
            refresh_sprite();
        }

        public string map_sprite_name
        {
            get
            {
                string name = "Map" + "Light Rune";
                if (!Global.content_exists(@"Graphics/Characters/" + name))
                    return "";
                return name;
            }
        }

        public void refresh_sprite()
        {
            if (Global.scene.is_map_scene)
            {
                string name = map_sprite_name;
                ((Scene_Map)Global.scene).refresh_map_sprite(
                    Id, Team, name, false);
            }
        }

        public override void update_sprite(Sprite sprite)
        {
            base.update_sprite(sprite);
            sprite.update();
            sprite.frame = (Facing / 2 - 1) * ((Graphics.Map.Character_Sprite)sprite).frame_count + Frame;
            sprite.draw_offset = new Vector2(TILE_SIZE / 2, TILE_SIZE);
            //int a = ((int)Global.player.loc.X - 17) * 16 + 240; @Debug
            //int b = ((int)Global.player.loc.Y - 7) * 16 + 160;
            sprite.tint = new Color(232, 232, 232, 176);
        }
        #endregion
    }
}
