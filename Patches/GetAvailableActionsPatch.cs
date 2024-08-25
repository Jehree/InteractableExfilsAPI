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
            var owner = __args[0] as GamePlayerOwner;
            var interactive = __args[1]; // as GInterface114 as of SPT 3.9.5

            bool isCarExtract = InteractiveIsCarExtract(interactive);
            bool isElevatorSwitch = InteractiveIsElevatorSwitch(interactive);

            if (isCarExtract || isElevatorSwitch)
            {
                ExfiltrationPoint exfil = GetExfilPoint(interactive, isElevatorSwitch);
                List<ActionsTypesClass> actions = GetCustomExfilActions(exfil, owner, interactive);

                __result = new ActionsReturnClass { Actions = actions };

                return false;
            }

            return true;
        }

        private static bool InteractiveIsCarExtract(object interactive)
        {
            if (interactive is ExfiltrationPoint && InteractableExfilsService.ExfilHasRequirement((ExfiltrationPoint)interactive, ERequirementState.TransferItem)) return true;
            return false;
        }

        private static bool InteractiveIsElevatorSwitch(object interactive)
        {
            if (interactive is Switch)
            {
                Switch switcheroo = interactive as Switch;
                if (switcheroo.ExfiltrationPoint == null) return false;
                if (!InteractableExfilsService.ExfilIsElevator(switcheroo.ExfiltrationPoint)) return false;
                return true;
            }
            return false;
        }

        private static ExfiltrationPoint GetExfilPoint(object interactive, bool fromSwitch)
        {
            if (fromSwitch) return ((Switch)interactive).ExfiltrationPoint;
            return interactive as ExfiltrationPoint;
        }

        private static List<ActionsTypesClass> GetCustomExfilActions(ExfiltrationPoint exfil, GamePlayerOwner gamePlayerOwner, object interactive)
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

            ActionsReturnClass vanillaExfilActions = methodInfo.Invoke(null, args) as ActionsReturnClass;
            var player = Singleton<GameWorld>.Instance.MainPlayer;
            OnActionsAppliedResult eventResult = Singleton<InteractableExfilsService>.Instance.OnActionsApplied(exfil, player.Side);

            List<ActionsTypesClass> actions = new List<ActionsTypesClass>();
            if (vanillaExfilActions != null)
            {
                actions.AddRange(vanillaExfilActions.Actions);
            }
            actions.AddRange(CustomExfilAction.GetActionsTypesClassList(eventResult.Actions));

            return actions;
        }
    }
}
