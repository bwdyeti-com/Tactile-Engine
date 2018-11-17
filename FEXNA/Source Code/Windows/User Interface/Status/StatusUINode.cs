using System;
using System.Collections.Generic;
using FEXNA.Windows.UserInterface;

namespace FEXNA.Windows.UserInterface.Status
{
    abstract class StatusUINode : UINode
    {
        protected bool _enabled = true;

        internal string HelpLabel { get; private set; }

        protected override IEnumerable<Inputs> ValidInputs
        {
            get
            {
                yield break;
            }
        }
        protected override bool RightClickActive { get { return false; } }

        internal override bool Enabled { get { return _enabled; } }

        internal StatusUINode(string helpLabel)
        {
            HelpLabel = helpLabel;
        }

        internal abstract void refresh(Game_Unit unit);

#if DEBUG
        internal virtual void cheat(Game_Unit unit, DirectionFlags dir)
        {
            if (Cheat != null)
            {
                if (Cheat(unit, dir))
                    Global.game_system.play_se(System_Sounds.Menu_Move2);
            }
        }

        private Func<Game_Unit, DirectionFlags, bool> Cheat;
        internal void set_cheat(Func<Game_Unit, DirectionFlags, bool> cheat)
        {
            if (Cheat == null)
                Cheat = cheat;
        }
#endif
    }
}
