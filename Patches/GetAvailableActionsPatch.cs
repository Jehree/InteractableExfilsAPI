using Comfort.Common;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using InteractableExfilsAPI.Common;
using InteractableExfilsAPI.Components;
using InteractableExfilsAPI.Singletons;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace InteractableExfilsAPI.Patches
{
    internal class GetAvailableActionsPatch : ModulePatch
    {
        private static MethodInfo _getExfiltrationActions;
        private static MethodInfo _getSwitchActions;

        protected override MethodBase GetTargetMethod()
        {
            _getExfiltrationActions = AccessTools.FirstMethod(
                typeof(GetActionsClass),
                method =>
                method.GetParameters()[0].Name == "owner" &&
                method.GetParameters()[1].ParameterType == typeof(ExfiltrationPoint)
            );

            _getSwitchActions = AccessTools.FirstMethod(
                typeof(GetActionsClass),
                method =>
                method.GetParameters()[0].Name == "owner" &&
                method.GetParameters()[1].ParameterType == typeof(Switch)
            );

            return AccessTools.FirstMethod(typeof(GetActionsClass), method => method.Name == nameof(GetActionsClass.GetAvailableActions) && method.GetParameters()[0].Name == "owner");
        }

        [PatchPrefix]
        public static bool PatchPrefix(object[] __args, ref ActionsReturnClass __result)
        {
            var player = Singleton<GameWorld>.Instance.MainPlayer;
            var owner = __args[0] as GamePlayerOwner;
            var interactive = __args[1]; // as GInterface114 as of SPT 3.9.5

            bool isCarExtract = InteractiveIsCarExtract(interactive);
            bool isElevatorSwitch = InteractiveIsElevatorSwitch(interactive);

            if (isCarExtract || isElevatorSwitch)
            {
                ExfiltrationPoint exfil = GetExfilPointFromInteractive(interactive);

                List<ActionsTypesClass> actions = new List<ActionsTypesClass>();
                actions.AddRange(GetVanillaInteractionActions(owner, interactive));
                actions.AddRange(GetCustomExfilActions(exfil));

                __result = new ActionsReturnClass { Actions = actions };

                return false;
            }

            return true;
        }

        private static bool InteractiveIsCarExtract(object interactive)
        {
            if (!(interactive is ExfiltrationPoint)) return false;
            if (InteractableExfilsService.ExfilIsCar((ExfiltrationPoint)interactive)) return true;
            return false;
        }

        private static bool InteractiveIsElevatorSwitch(object interactive)
        {
            if (!(interactive is Switch)) return false;
            Switch switcheroo = interactive as Switch;
            if (switcheroo.ExfiltrationPoint == null) return false;
            if (!InteractableExfilsService.ExfilIsElevator(switcheroo.ExfiltrationPoint)) return false;
            return true;
        }

        private static ExfiltrationPoint GetExfilPointFromInteractive(object interactive)
        {
            if (interactive is Switch) return ((Switch)interactive).ExfiltrationPoint;
            return interactive as ExfiltrationPoint;
        }

        private static List<ActionsTypesClass> GetVanillaInteractionActions(GamePlayerOwner gamePlayerOwner, object interactive)
        {
            object[] args = new object[2];
            args[0] = gamePlayerOwner;
            args[1] = interactive;

            MethodInfo methodInfo = null;
            if (interactive is ExfiltrationPoint)
            {
                methodInfo = _getExfiltrationActions;
            }
            if (interactive is Switch)
            {
                methodInfo = _getSwitchActions;
            }

            List<ActionsTypesClass> vanillaExfilActions = ((ActionsReturnClass)methodInfo.Invoke(null, args))?.Actions;
            return vanillaExfilActions ?? new List<ActionsTypesClass>();
        }

        private static List<ActionsTypesClass> GetCustomExfilActions(ExfiltrationPoint exfil)
        {
            var player = Singleton<GameWorld>.Instance.MainPlayer;
            OnActionsAppliedResult eventResult = Singleton<InteractableExfilsService>.Instance.OnActionsApplied(exfil, null, true);

            return CustomExfilAction.GetActionsTypesClassList(eventResult.Actions);
        }
    }
}
