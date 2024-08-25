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
    [BepInPlugin("Jehree.InteractableExfilsAPI", "InteractableExfilsAPI", "1.1.0")]
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
        }

        private void Start()
        {
            //Singleton<InteractableExfilsService>.Instance.OnActionsAppliedEvent += ExampleMethod;
        }

        public OnActionsAppliedResult ExampleMethod(ExfiltrationPoint exfil, EPlayerSide side)
        {
            CustomExfilAction customExfilAction = new CustomExfilAction(
                "Another Example Action",
                false,
                () => { ConsoleScreen.Log($"Interaction with {exfil.Settings.Name} was a success!!"); }
            );

            var resultList = new List<CustomExfilAction>();
            resultList.Add(customExfilAction);
            var result = new OnActionsAppliedResult(resultList);
            return result;
        }
    }
}
