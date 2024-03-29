﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tactile.Menus.Options
{
    class GameplaySettingsTopMenu : SettingsTopMenu
    {
        public bool SoloAnimAllowed { get; private set; }

        public GameplaySettingsTopMenu(bool soloAnimAllowed) : base()
        {
            SoloAnimAllowed = soloAnimAllowed;
        }

        protected override List<string> GetSettingsStrings()
        {
            List<string> settings = new List<string> { "Gameplay", "Graphics", "Audio", "Controls" };
            if (Global.gameSettings.General.AnyValidSettings)
                settings.Insert(1, "General");
            return settings;
        }

        #region StandardMenu Abstract
        public override int Index
        {
            get
            {
                int index = Window.index;
                if (!Global.gameSettings.General.AnyValidSettings && index >= 1)
                    index++;
                return index;
            }
        }
        #endregion
    }
}
