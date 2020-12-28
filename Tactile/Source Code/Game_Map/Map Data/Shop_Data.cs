using System.Collections.Generic;
using System.IO;
using TactileLibrary;
using ArrayExtension;
using ListExtension;

namespace Tactile.Map
{
    internal class Shop_Data
    {
        string Face;
        string Song;
        int[] Offsets;
        bool Secret;
        bool Arena;
        List<string> Text = new List<string>();
        List<ShopItemData> Items = new List<ShopItemData>();

        #region Serialization
        public void write(BinaryWriter writer)
        {
            writer.Write(Face);
            writer.Write(Song);
            Offsets.write(writer);
            writer.Write(Secret);
            writer.Write(Arena);
            Text.write(writer);
            Items.write(writer);
        }

        public static Shop_Data read(BinaryReader reader)
        {
            Shop_Data result = new Shop_Data("", "", new int[0], false, false);
            result.Face = reader.ReadString();
            result.Song = reader.ReadString();
            result.Offsets = result.Offsets.read(reader);
            result.Secret = reader.ReadBoolean();
            result.Arena = reader.ReadBoolean();
            result.Text.read(reader);
            result.Items.read(reader);
            return result;
        }
        #endregion

        #region Accessors
        public string face { get { return Face; } }

        public string song { get { return Song; } }

        public int[] offsets { get { return Offsets; } }

        public bool secret { get { return Secret; } }

        public bool arena { get { return Arena; } }

        public List<ShopItemData> items { get { return Items; } }
        #endregion

        public Shop_Data(string face, string song, int[] offsets, bool secret, bool arena)
        {
            Face = face;
            Song = song;
            Offsets = offsets;
            Secret = secret;
            Arena = arena;
        }

        public void add_text(string str)
        {
            Text.Add(str);
        }
        public void add_text(IEnumerable<string> strs)
        {
            Text.AddRange(strs);
        }

        public string text(int index)
        {
            if (index >= Text.Count)
            {
                string text_key = string.Format("Shop {0} {1}", Face, index);
                if (Global.system_text.ContainsKey(text_key))
                    return Global.system_text[text_key];
                return "Well?";
            }
            return Text[index];
        }

        public void add_item(ShopItemData item)
        {
            Items.Add(item);
        }
        public void add_items(IEnumerable<ShopItemData> items)
        {
            Items.AddRange(items);
        }
    }
}
