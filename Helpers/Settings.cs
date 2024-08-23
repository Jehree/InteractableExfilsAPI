using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractableExfilsAPI.Helpers
{
    internal class Settings
    {
        public static ConfigEntry<bool> ExtractAreaStartsEnabled;
        public static ConfigEntry<bool> DebugMode;
        
        public static void Init (ConfigFile config)
        {
            ExtractAreaStartsEnabled = config.Bind(
                "1: Settings",
                "Extract Area Defaults To Enabled",
                true
            );

            DebugMode = config.Bind(
                "2: Debug",
                "Enable Debug Actions",
                false
            );
        }
    }
}
