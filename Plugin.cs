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
    [BepInPlugin("Jehree.InteractableExfilsAPI", "InteractableExfilsAPI", "1.2.0")]
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
            InteractableExfilsService service = Singleton<InteractableExfilsService>.Instance;
            service.OnActionsAppliedEvent += service.ApplyUnavailableExtractAction;
            service.OnActionsAppliedEvent += service.ApplyExtractToggleAction;
            service.OnActionsAppliedEvent += service.ApplyDebugAction;

            new GameStartedPatch().Enable();
            new GetAvailableActionsPatch().Enable();
        }

        private void Start()
        {
            //Examples examplesClass = new Examples();
            //Singleton<InteractableExfilsService>.Instance.OnActionsAppliedEvent += examplesClass.SimpleExample;
            //Singleton<InteractableExfilsService>.Instance.OnActionsAppliedEvent += examplesClass.GoneWhenDisabledExample;
            //Singleton<InteractableExfilsService>.Instance.OnActionsAppliedEvent += examplesClass.DynamicDisabledExample;
            //Singleton<InteractableExfilsService>.Instance.OnActionsAppliedEvent += examplesClass.SoftDynamicDisabledExample;
            //Singleton<InteractableExfilsService>.Instance.OnActionsAppliedEvent += examplesClass.ScavGate3OnlyExample;
        }


    }
}
