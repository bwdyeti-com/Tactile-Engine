using Microsoft.Xna.Framework;
using System.IO;
using ArrayExtension;
using FEXNAVector2Extension;

namespace FEXNA
{
    internal enum Siege_Engine_State { Ready, Fired, Reloading }
    internal class Siege_Engine : Map_Object
    {
        internal const int SIEGE_INVENTORY_INDEX = Constants.Actor.NUM_ITEMS;

        protected FEXNA_Library.Item_Data Item;
        protected Siege_Engine_State State = Siege_Engine_State.Ready;

        #region Serialization
        public void write(BinaryWriter writer)
        {
            writer.Write(Id);
            Loc.write(writer);
            Real_Loc.write(writer);
            Item.write(writer);
            writer.Write((byte)State);
        }

        public void read(BinaryReader reader)
        {
            Id = reader.ReadInt32();
            Loc = Loc.read(reader);
            Real_Loc = Real_Loc.read(reader);
            Item = FEXNA_Library.Item_Data.read(reader);
            State = (Siege_Engine_State)reader.ReadByte();
        }
        #endregion

        #region Accessors
        public FEXNA_Library.Item_Data item { get { return Item; } }

        public bool has_full_ammo { get { return Item.Uses == Item.max_uses; } }

        public bool is_ready { get { return this.has_ammo && State == Siege_Engine_State.Ready; } }
        public bool has_ammo { get { return Item.Uses > 0; } }
        public bool ammo_count_greyed { get { return Item.Uses > 0 && State != Siege_Engine_State.Ready; } }
        #endregion

        public Siege_Engine() { }
        public Siege_Engine(int id, Vector2 loc, FEXNA_Library.Item_Data item)
        {
            Id = id;
            force_loc(loc);
            Item = item;
            Facing = 2;
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

        public void fire()
        {
            State = Siege_Engine_State.Fired;
        }

        public void new_turn()
        {
            if (Constants.Gameplay.SIEGE_RELOADING &&
                !Constants.Gameplay.SIEGE_MANUAL_RELOADING)
            {
                if (State == Siege_Engine_State.Fired)
                    State = Siege_Engine_State.Reloading;
                else if (State == Siege_Engine_State.Reloading)
                    State = Siege_Engine_State.Ready;
            }
        }

        public void reload()
        {
            if (Constants.Gameplay.SIEGE_RELOADING)
            {
                State = Siege_Engine_State.Ready;
                refresh_sprite();
            }
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
                string name = "Map" + Item.name;
                // this error check seems unnecessary //Debug
                //if (!Global.content_exists(@"Graphics/Characters/" + name))
                //    return "";
                return name;
            }
        }
        public string rider_map_sprite_name
        {
            get
            {
                return string.Format("{0}_Rider", Item.name);
            }
        }

        public void refresh_sprite()
        {
            if (Global.scene.is_map_scene)
            {
                string name = map_sprite_name;
                ((Scene_Map)Global.scene).refresh_map_sprite(
                    Id, (State != Siege_Engine_State.Fired || Item.out_of_uses) ? 1 : 0, name, false);
            }
        }

        public override void update_sprite(Sprite sprite)
        {
            base.update_sprite(sprite);
            sprite.update();
            sprite.frame = (Facing / 2 - 1) * ((Graphics.Map.Character_Sprite)sprite).frame_count + Frame;
            sprite.draw_offset = new Vector2(TILE_SIZE / 2, TILE_SIZE);
        }
        #endregion
    }
}
