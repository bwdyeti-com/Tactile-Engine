using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FEXNA
{
    partial class Window_Status
    {
        /* //Debug
        readonly static List<string> Help_Text_Keys = new List<string>
        {
            "Lvl", "Exp", "Hp", "Lives",
            "Atk", "Hit", "Dodge", "Range", "Crit", "Avoid", "AS",
            "Skl", "Spd", "Lck", "Def", "Res", "Move", "Con", "Aid", "Trv", "Type", "Rating",
            "Sword", "Lance", "Axe", "Bow", "Fire", "Thund", "Wind", "Light", "Dark", "Staff",
            "Cond", "Status1", "Status2", "Status3", "Bond", "BAtk", "BHit", "BCrt", "BDef", "BAvo", "BDod", "Affin"
        };

        readonly static Dictionary<string, Help_Node_Data>[] Help_Data = new Dictionary<string, Help_Node_Data>[] {
            new Dictionary<string, Help_Node_Data>
            {
                #region Page 1
                { "Name", new Help_Node_Data(new Vector2(112, 4), new Vector2(32, 0), "Name", new Dictionary<int,string> {
                    { 2, "Class" }, { 6, "Range" }})},
                { "Class", new Help_Node_Data(new Vector2(104, 24), new Vector2(32, 0), "Class", new Dictionary<int,string> {
                    { 2, "Lvl" }, { 6, "Atk" }, { 8, "Name" }})},
                { "Lvl", new Help_Node_Data(new Vector2(104, 40), new Vector2(32, 0), "Lvl", new Dictionary<int,string> {
                    { 2, "Hp" }, { 6, "Exp" }, { 8, "Class" }})},
                { "Exp", new Help_Node_Data(new Vector2(136, 40), new Vector2(32, 0), "Exp", new Dictionary<int,string> {
                    { 2, "Hp" }, { 4, "Lvl" }, { 6, "Hit" }, { 8, "Class" }})},
                { "Hp", new Help_Node_Data(new Vector2(104, 56), new Vector2(32, 0), "Hp", new Dictionary<int,string> {
                    { 2, "Move" }, { 6, "Lives" }, { 8, "Lvl" }})},
                { "Lives", new Help_Node_Data(new Vector2(180, 60), new Vector2(32, 0), "Lives", new Dictionary<int,string> {
                    { 2, "Item1" }, { 4, "Hp" }, { 6, "Dodge" }, { 8, "Exp" }})},
                { "Atk", new Help_Node_Data(new Vector2(204, 24), new Vector2(0, 0), "Atk", new Dictionary<int,string> {
                    { 2, "Hit" }, { 4, "Class" }, { 6, "Crit" }, { 8, "Range" }})},
                { "Hit", new Help_Node_Data(new Vector2(204, 40), new Vector2(0, 0), "Hit", new Dictionary<int,string> {
                    { 2, "Dodge" }, { 4, "Exp" }, { 6, "Avoid" }, { 8, "Atk" }})},
                { "Dodge", new Help_Node_Data(new Vector2(204, 56), new Vector2(0, 0), "Dodge", new Dictionary<int,string> {
                    { 2, "Item1" }, { 4, "Lives" }, { 6, "AS" }, { 8, "Hit" }})},
                { "Range", new Help_Node_Data(new Vector2(260, 8), new Vector2(0, 0), "Range", new Dictionary<int,string> {
                    { 2, "Crit" }, { 4, "Name" }})},
                { "Crit", new Help_Node_Data(new Vector2(260, 24), new Vector2(0, 0), "Crit", new Dictionary<int,string> {
                    { 2, "Avoid" }, { 4, "Atk" }, { 8, "Range" }})},
                { "Avoid", new Help_Node_Data(new Vector2(260, 40), new Vector2(0, 0), "Avoid", new Dictionary<int,string> {
                    { 2, "AS" }, { 4, "Hit" }, { 8, "Crit" }})},
                { "AS", new Help_Node_Data(new Vector2(260, 56), new Vector2(0, 0), "AS", new Dictionary<int,string> {
                    { 2, "Item1" }, { 4, "Dodge" }, { 8, "Avoid" }})},

                { "Pow", new Help_Node_Data(new Vector2(16, 88), new Vector2(0, 0), "Pow", new Dictionary<int,string> {
                    { 2, "Skl" }, { 6, "Move" }, { 8, "Hp" }})},
                { "Skl", new Help_Node_Data(new Vector2(16, 104), new Vector2(0, 0), "Skl", new Dictionary<int,string> {
                    { 2, "Spd" }, { 6, "Con" }, { 8, "Pow" }})},
                { "Spd", new Help_Node_Data(new Vector2(16, 120), new Vector2(0, 0), "Spd", new Dictionary<int,string> {
                    { 2, "Lck" }, { 6, "Aid" }, { 8, "Skl" }})},
                { "Lck", new Help_Node_Data(new Vector2(16, 136), new Vector2(0, 0), "Lck", new Dictionary<int,string> {
                    { 2, "Def" }, { 6, "Trv" }, { 8, "Spd" }})},
                { "Def", new Help_Node_Data(new Vector2(16, 152), new Vector2(0, 0), "Def", new Dictionary<int,string> {
                    { 2, "Res" }, { 6, "Type" }, { 8, "Lck" }})},
                { "Res", new Help_Node_Data(new Vector2(16, 168), new Vector2(0, 0), "Res", new Dictionary<int,string> {
                    { 6, "Rating" }, { 8, "Def" }})},
                { "Move", new Help_Node_Data(new Vector2(80, 88), new Vector2(0, 0), "Move", new Dictionary<int,string> {
                    { 2, "Con" }, { 4, "Pow" }, { 6, "Item1" }, { 8, "Hp" }})},
                { "Con", new Help_Node_Data(new Vector2(80, 104), new Vector2(0, 0), "Con", new Dictionary<int,string> {
                    { 2, "Aid" }, { 4, "Skl" }, { 6, "Item2" }, { 8, "Move" }})},
                { "Aid", new Help_Node_Data(new Vector2(80, 120), new Vector2(0, 0), "Aid", new Dictionary<int,string> {
                    { 2, "Trv" }, { 4, "Spd" }, { 6, "Item3" }, { 8, "Con" }})},
                { "Trv", new Help_Node_Data(new Vector2(80, 136), new Vector2(0, 0), "Trv", new Dictionary<int,string> {
                    { 2, "Type" }, { 4, "Lck" }, { 6, "Item4" }, { 8, "Aid" }})},
                { "Type", new Help_Node_Data(new Vector2(80, 152), new Vector2(0, 0), "Type", new Dictionary<int,string> {
                    { 2, "Rating" }, { 4, "Def" }, { 6, "Item5" }, { 8, "Trv" }})},
                { "Rating", new Help_Node_Data(new Vector2(80, 168), new Vector2(0, 0), "Rating", new Dictionary<int,string> {
                    { 4, "Res" }, { 6, "Item6" }, { 8, "Type" }})},

                { "Item1", new Help_Node_Data(new Vector2(176, 88), new Vector2(0, 0), "Item", new Dictionary<int,string> {
                    { 2, "Item2" }, { 4, "Move" }, { 8, "Dodge" }})},
                { "Item2", new Help_Node_Data(new Vector2(176, 104), new Vector2(0, 0), "Item", new Dictionary<int,string> {
                    { 2, "Item3" }, { 4, "Con" }, { 8, "Item1" }})},
                { "Item3", new Help_Node_Data(new Vector2(176, 120), new Vector2(0, 0), "Item", new Dictionary<int,string> {
                    { 2, "Item4" }, { 4, "Aid" }, { 8, "Item2" }})},
                { "Item4", new Help_Node_Data(new Vector2(176, 136), new Vector2(0, 0), "Item", new Dictionary<int,string> {
                    { 2, "Item5" }, { 4, "Trv" }, { 8, "Item3" }})},
                { "Item5", new Help_Node_Data(new Vector2(176, 152), new Vector2(0, 0), "Item", new Dictionary<int,string> {
                    { 2, "Item6" }, { 4, "Type" }, { 8, "Item4" }})},
                { "Item6", new Help_Node_Data(new Vector2(176, 168), new Vector2(0, 0), "Item", new Dictionary<int,string> {
                    { 4, "Rating" }, { 8, "Item5" }})}
                #endregion
            },
            new Dictionary<string, Help_Node_Data>
            {
                #region Page 2
                { "Name", new Help_Node_Data(new Vector2(112, 4), new Vector2(0, 0), "Name", new Dictionary<int,string> {
                    { 2, "Class" }, { 6, "Range" }})},
                { "Class", new Help_Node_Data(new Vector2(104, 24), new Vector2(0, 0), "Class", new Dictionary<int,string> {
                    { 2, "Lvl" }, { 6, "Atk" }, { 8, "Name" }})},
                { "Lvl", new Help_Node_Data(new Vector2(104, 40), new Vector2(0, 0), "Lvl", new Dictionary<int,string> {
                    { 2, "Hp" }, { 6, "Exp" }, { 8, "Class" }})},
                { "Exp", new Help_Node_Data(new Vector2(136, 40), new Vector2(0, 0), "Exp", new Dictionary<int,string> {
                    { 2, "Hp" }, { 4, "Lvl" }, { 6, "Hit" }, { 8, "Class" }})},
                { "Hp", new Help_Node_Data(new Vector2(104, 56), new Vector2(0, 0), "Hp", new Dictionary<int,string> {
                    { 2, "Skill1" }, { 6, "Lives" }, { 8, "Lvl" }})},
                { "Lives", new Help_Node_Data(new Vector2(180, 60), new Vector2(32, 0), "Lives", new Dictionary<int,string> {
                    { 2, "Item1" }, { 4, "Hp" }, { 6, "Dodge" }, { 8, "Exp" }})},
                { "Atk", new Help_Node_Data(new Vector2(204, 24), new Vector2(0, 0), "Atk", new Dictionary<int,string> {
                    { 2, "Hit" }, { 4, "Class" }, { 6, "Crit" }, { 8, "Range" }})},
                { "Hit", new Help_Node_Data(new Vector2(204, 40), new Vector2(0, 0), "Hit", new Dictionary<int,string> {
                    { 2, "Dodge" }, { 4, "Exp" }, { 6, "Avoid" }, { 8, "Atk" }})},
                { "Dodge", new Help_Node_Data(new Vector2(204, 56), new Vector2(0, 0), "Dodge", new Dictionary<int,string> {
                    { 2, "Sword" }, { 4, "Lives" }, { 6, "AS" }, { 8, "Hit" }})},
                { "Range", new Help_Node_Data(new Vector2(260, 8), new Vector2(0, 0), "Range", new Dictionary<int,string> {
                    { 2, "Crit" }, { 4, "Name" }})},
                { "Crit", new Help_Node_Data(new Vector2(260, 24), new Vector2(0, 0), "Crit", new Dictionary<int,string> {
                    { 2, "Avoid" }, { 4, "Atk" }, { 8, "Range" }})},
                { "Avoid", new Help_Node_Data(new Vector2(260, 40), new Vector2(0, 0), "Avoid", new Dictionary<int,string> {
                    { 2, "AS" }, { 4, "Hit" }, { 8, "Crit" }})},
                { "AS", new Help_Node_Data(new Vector2(260, 56), new Vector2(0, 0), "AS", new Dictionary<int,string> {
                    { 2, "Axe" }, { 4, "Dodge" }, { 8, "Avoid" }})},
                    
                { "Skill1", new Help_Node_Data(new Vector2(16, 92), new Vector2(0, 0), "Skill", new Dictionary<int,string> {
                    { 2, "Skill2" }, { 6, "Sword" }, { 8, "Hp" }})},
                { "Skill2", new Help_Node_Data(new Vector2(16, 116), new Vector2(0, 0), "Skill", new Dictionary<int,string> {
                    { 2, "Skill3" }, { 6, "Lance" }, { 8, "Skill1" }})},
                { "Skill3", new Help_Node_Data(new Vector2(16, 140), new Vector2(0, 0), "Skill", new Dictionary<int,string> {
                    { 2, "Skill4" }, { 6, "Fire" }, { 8, "Skill2" }})},
                { "Skill4", new Help_Node_Data(new Vector2(16, 164), new Vector2(0, 0), "Skill", new Dictionary<int,string> {
                    { 6, "Thund" }, { 8, "Skill3" }})},
                    
                { "Sword", new Help_Node_Data(new Vector2(176, 104), new Vector2(0, 0), "Sword", new Dictionary<int,string> {
                    { 2, "Lance" }, { 4, "Skill1" }, { 6, "Axe" }, { 8, "Dodge" }})},
                { "Lance", new Help_Node_Data(new Vector2(176, 120), new Vector2(0, 0), "Lance", new Dictionary<int,string> {
                    { 2, "Fire" }, { 4, "Skill2" }, { 6, "Bow" }, { 8, "Sword" }})},
                { "Fire", new Help_Node_Data(new Vector2(176, 136), new Vector2(0, 0), "Fire", new Dictionary<int,string> {
                    { 2, "Thund" }, { 4, "Skill3" }, { 6, "Light" }, { 8, "Lance" }})},
                { "Thund", new Help_Node_Data(new Vector2(176, 152), new Vector2(0, 0), "Thund", new Dictionary<int,string> {
                    { 2, "Wind" }, { 4, "Skill4" }, { 6, "Dark" }, { 8, "Fire" }})},
                { "Wind", new Help_Node_Data(new Vector2(176, 168), new Vector2(0, 0), "Wind", new Dictionary<int,string> {
                    { 6, "Staff" }, { 4, "Skill4" }, { 8, "Thund" }})},
                { "Axe", new Help_Node_Data(new Vector2(240, 104), new Vector2(0, 0), "Axe", new Dictionary<int,string> {
                    { 2, "Bow" }, { 4, "Sword" }, { 8, "AS" }})},
                { "Bow", new Help_Node_Data(new Vector2(240, 120), new Vector2(0, 0), "Bow", new Dictionary<int,string> {
                    { 2, "Light" }, { 4, "Lance" }, { 8, "Axe" }})},
                { "Light", new Help_Node_Data(new Vector2(240, 136), new Vector2(0, 0), "Light", new Dictionary<int,string> {
                    { 2, "Dark" }, { 4, "Fire" }, { 8, "Bow" }})},
                { "Dark", new Help_Node_Data(new Vector2(240, 152), new Vector2(0, 0), "Dark", new Dictionary<int,string> {
                    { 2, "Staff" }, { 4, "Thund" }, { 8, "Light" }})},
                { "Staff", new Help_Node_Data(new Vector2(240, 168), new Vector2(0, 0), "Staff", new Dictionary<int,string> {
                    { 4, "Wind" }, { 8, "Dark" }})},
                #endregion
            },
            new Dictionary<string, Help_Node_Data>
            {
                #region Page 3
                { "Name", new Help_Node_Data(new Vector2(112, 4), new Vector2(0, 0), "Name", new Dictionary<int,string> {
                    { 2, "Class" }, { 6, "Range" }})},
                { "Class", new Help_Node_Data(new Vector2(104, 24), new Vector2(0, 0), "Class", new Dictionary<int,string> {
                    { 2, "Lvl" }, { 6, "Atk" }, { 8, "Name" }})},
                { "Lvl", new Help_Node_Data(new Vector2(104, 40), new Vector2(0, 0), "Lvl", new Dictionary<int,string> {
                    { 2, "Hp" }, { 6, "Exp" }, { 8, "Class" }})},
                { "Exp", new Help_Node_Data(new Vector2(136, 40), new Vector2(0, 0), "Exp", new Dictionary<int,string> {
                    { 2, "Hp" }, { 4, "Lvl" }, { 6, "Hit" }, { 8, "Class" }})},
                { "Hp", new Help_Node_Data(new Vector2(104, 56), new Vector2(0, 0), "Hp", new Dictionary<int,string> {
                    { 2, "Status3" }, { 6, "Lives" }, { 8, "Lvl" }})},
                { "Lives", new Help_Node_Data(new Vector2(180, 60), new Vector2(32, 0), "Lives", new Dictionary<int,string> {
                    { 2, "Item1" }, { 4, "Hp" }, { 6, "Dodge" }, { 8, "Exp" }})},
                { "Atk", new Help_Node_Data(new Vector2(204, 24), new Vector2(0, 0), "Atk", new Dictionary<int,string> {
                    { 2, "Hit" }, { 4, "Class" }, { 6, "Crit" }, { 8, "Range" }})},
                { "Hit", new Help_Node_Data(new Vector2(204, 40), new Vector2(0, 0), "Hit", new Dictionary<int,string> {
                    { 2, "Dodge" }, { 4, "Exp" }, { 6, "Avoid" }, { 8, "Atk" }})},
                { "Dodge", new Help_Node_Data(new Vector2(204, 56), new Vector2(0, 0), "Dodge", new Dictionary<int,string> {
                    { 2, "Affin" }, { 4, "Lives" }, { 6, "AS" }, { 8, "Hit" }})},
                { "Range", new Help_Node_Data(new Vector2(260, 8), new Vector2(0, 0), "Range", new Dictionary<int,string> {
                    { 2, "Crit" }, { 4, "Name" }})},
                { "Crit", new Help_Node_Data(new Vector2(260, 24), new Vector2(0, 0), "Crit", new Dictionary<int,string> {
                    { 2, "Avoid" }, { 4, "Atk" }, { 8, "Range" }})},
                { "Avoid", new Help_Node_Data(new Vector2(260, 40), new Vector2(0, 0), "Avoid", new Dictionary<int,string> {
                    { 2, "AS" }, { 4, "Hit" }, { 8, "Crit" }})},
                { "AS", new Help_Node_Data(new Vector2(260, 56), new Vector2(0, 0), "AS", new Dictionary<int,string> {
                    { 2, "Affin" }, { 4, "Dodge" }, { 8, "Avoid" }})},
                    
                { "Cond", new Help_Node_Data(new Vector2(32, 88), new Vector2(0, 0), "Cond", new Dictionary<int,string> {
                    { 2, "Bond" }, { 6, "Status1" }, { 8, "Hp" }})},
                { "Status1", new Help_Node_Data(new Vector2(64, 88), new Vector2(0, 0), "Status", new Dictionary<int,string> {
                    { 2, "Bond" }, { 4, "Cond" }, { 6, "Status2" }, { 8, "Hp" }})},
                { "Status2", new Help_Node_Data(new Vector2(80, 88), new Vector2(0, 0), "Status", new Dictionary<int,string> {
                    { 2, "Bond" }, { 4, "Status1" }, { 6, "Status3" }, { 8, "Hp" }})},
                { "Status3", new Help_Node_Data(new Vector2(96, 88), new Vector2(0, 0), "Status", new Dictionary<int,string> {
                    { 2, "Bond" }, { 4, "Status2" }, { 6, "Affin" }, { 8, "Hp" }})},
                { "Bond", new Help_Node_Data(new Vector2(36, 116), new Vector2(0, 0), "Bond", new Dictionary<int,string> {
                    { 2, "BAtk" }, { 6, "Affin" }, { 8, "Cond" }})},
                { "BAtk", new Help_Node_Data(new Vector2(28, 136), new Vector2(0, 0), "BAtk", new Dictionary<int,string> {
                    { 2, "BHit" }, { 6, "BDef" }, { 8, "Bond" }})},
                { "BHit", new Help_Node_Data(new Vector2(28, 152), new Vector2(0, 0), "BHit", new Dictionary<int,string> {
                    { 2, "BCrt" }, { 6, "BAvo" }, { 8, "BAtk" }})},
                { "BCrt", new Help_Node_Data(new Vector2(28, 168), new Vector2(0, 0), "BCrt", new Dictionary<int,string> {
                    { 6, "BDod" }, { 8, "BHit" }})},
                { "BDef", new Help_Node_Data(new Vector2(84, 136), new Vector2(0, 0), "BDef", new Dictionary<int,string> {
                    { 2, "BAvo" }, { 4, "BAtk" }, { 6, "Affin" }, { 8, "Bond" }})},
                { "BAvo", new Help_Node_Data(new Vector2(84, 152), new Vector2(0, 0), "BAvo", new Dictionary<int,string> {
                    { 2, "BDod" }, { 4, "BHit" }, { 6, "Affin" }, { 8, "BDef" }})},
                { "BDod", new Help_Node_Data(new Vector2(84, 168), new Vector2(0, 0), "BDod", new Dictionary<int,string> {
                    { 4, "BCrt" }, { 6, "Affin" }, { 8, "BAvo" }})},

                { "Affin", new Help_Node_Data(new Vector2(176 + 32, 88), new Vector2(0, 0), "Affin", new Dictionary<int,string> {
                    { 4, "Status3" }, { 8, "Dodge" }})},
                #endregion
            }
        };
        */
    }
}
