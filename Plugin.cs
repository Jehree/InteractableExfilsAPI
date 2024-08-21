using BepInEx;
using BepInEx.Logging;
using Comfort.Common;
using EFT.Interactive;
using EFT.UI;
using InteractableExfilsAPI.Helpers;
using InteractableExfilsAPI.Patches;
using InteractableExfilsAPI.Singletons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InteractableExfilsAPI
{
    [BepInPlugin("Jehree.InteractableExfilsAPI", "InteractableExfilsAPI", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;
        public static string AssemblyPath { get; private set; } = Assembly.GetExecutingAssembly().Location;
        public const string MOD_NAME = "Interactable Exfils API";

        private void Awake()
        {
            LogSource = Logger;
            Settings.Init(Config);
            Singleton<InteractableExfilsService>.Create(new InteractableExfilsService());

            new GameStartedPatch().Enable();
            //new GameEndedPatch().Enable();
            new GetAvailableActionsPatch().Enable();

            Singleton<InteractableExfilsService>.Instance.OnActionsAppliedEvent += Test;
        }

        public OnActionsAppliedResult Test(ExfiltrationPoint exfil)
        {
            var result = new OnActionsAppliedResult();
            ActionsTypesClass action = new ActionsTypesClass
            {
                Name = "TEST",
                Action = () => { ConsoleScreen.LogError($"TEST SUCCESS WOOOO {exfil.Settings.Name}"); }
            };
            result.Actions.Add(action);

            return result;
        }
    }
}
