using BepInEx;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.UI;
using InteractableExfilsAPI.Common;
using InteractableExfilsAPI.Components;
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
            new GetAvailableActionsPatch().Enable();

            // Singleton<InteractableExfilsService>.Instance.OnActionsAppliedEvent += Test;
        }

        public OnActionsAppliedResult Test(ExfiltrationPoint exfil, EPlayerSide side)
        {
            if (exfil.Settings.Name != "Gate 3") return null;
            if (side == EPlayerSide.Savage) return null;

            CustomExfilAction customExfilAction = new CustomExfilAction(
                "Get Exfil Tips",
                false,
                () =>
                {
                    var tips = exfil.GetTips(Singleton<GameWorld>.Instance.MainPlayer.ProfileId);
                    foreach (var tip in tips)
                    {
                        ConsoleScreen.Log(tip);
                    }
                }
            );

            var resultList = new List<CustomExfilAction>();
            resultList.Add(customExfilAction);
            var result = new OnActionsAppliedResult(resultList);
            return result;
        }
    }
}
