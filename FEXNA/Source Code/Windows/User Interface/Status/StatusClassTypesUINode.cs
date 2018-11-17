using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FEXNA.Graphics.Text;
using FEXNA_Library;

namespace FEXNA.Windows.UserInterface.Status
{
    class StatusClassTypesUINode : StatusLabeledTextUINode
    {
        const int ICON_SPACING = 12;
        protected Func<Game_Unit, IEnumerable<ClassTypes>> ClassTypesFormula;
        private int IconOffset;
        protected List<Icon_Sprite> Class_Type_Icons = new List<Icon_Sprite>();

        internal StatusClassTypesUINode(
                string helpLabel,
                string label,
                Func<Game_Unit, IEnumerable<ClassTypes>> classTypesFormula,
                int textOffset = 32,
                int iconOffset = 40)
            : base(helpLabel, label, null, textOffset)
        {
            ClassTypesFormula = classTypesFormula;
            IconOffset = iconOffset;

            Size = new Vector2(iconOffset + 24, 16);
        }

        internal override void refresh(Game_Unit unit)
        {
            Class_Type_Icons.Clear();

            Text.text = "---";
            if (ClassTypesFormula != null)
            {
                var class_types = ClassTypesFormula(unit)
                    .Where(x => Config.CLASS_TYPE_ICONS.Contains(x));
                if (class_types.Any())
                {
                    Text.text = "";

                    int class_type_count = class_types.Count();
                    foreach (ClassTypes type in class_types)
                        if (Config.CLASS_TYPE_ICONS.Contains(type))
                        {
                            var icon = new Icon_Sprite();
                            icon.texture = Global.Content.Load<Texture2D>(@"Graphics/Icons/Class_Types");
                            icon.size = new Vector2(16, 16);
                            icon.columns = 1;
                            icon.draw_offset = new Vector2(
                                IconOffset + ((Class_Type_Icons.Count) * ICON_SPACING) -
                                    ((class_type_count - 1) * (ICON_SPACING / 2)),
                                0);
                            icon.offset = new Vector2(8, 0);
                            icon.index = (int)type;
                            icon.stereoscopic = Config.STATUS_LEFT_WINDOW_DEPTH;

                            Class_Type_Icons.Add(icon);
                        }
                }
            }
        }

        protected override void update_graphics(bool activeNode)
        {
            base.update_graphics(activeNode);

            foreach (Icon_Sprite icon in Class_Type_Icons)
                icon.update();
        }

        protected override void mouse_off_graphic()
        {
            base.mouse_off_graphic();
            foreach (var icon in Class_Type_Icons)
                icon.tint = Color.White;
        }
        protected override void mouse_highlight_graphic()
        {
            base.mouse_highlight_graphic();
            foreach (var icon in Class_Type_Icons)
                icon.tint = Config.MOUSE_OVER_ELEMENT_COLOR;
        }
        protected override void mouse_click_graphic()
        {
            base.mouse_click_graphic();
            foreach (var icon in Class_Type_Icons)
                icon.tint = Config.MOUSE_PRESSED_ELEMENT_COLOR;
        }

        public override void Draw(SpriteBatch sprite_batch, Vector2 draw_offset = default(Vector2))
        {
            base.Draw(sprite_batch, draw_offset);

            foreach (Icon_Sprite icon in Class_Type_Icons)
                icon.draw(sprite_batch, draw_offset - (loc + draw_vector()));
        }
    }
}
