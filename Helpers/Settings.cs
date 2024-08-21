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
        public static ConfigEntry<bool> ExtractAreaStartsDisabled;
        
        public static void Init (ConfigFile config)
        {
            ExtractAreaStartsDisabled = config.Bind(
                "1: Settings",
                "Extract Area Starts Disabled",
                false
            );
        }
    }
}
