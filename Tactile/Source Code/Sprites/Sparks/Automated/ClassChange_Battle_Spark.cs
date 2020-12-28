using Microsoft.Xna.Framework.Content;

namespace Tactile
{
    class ClassChange_Battle_Spark : LevelUp_Battle_Spark
    {
        protected override string filename { get { return "ClassChange_Battle"; } }

        public ClassChange_Battle_Spark(ContentManager content) : base(content) { }
    }
}
