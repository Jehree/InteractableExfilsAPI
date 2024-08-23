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

        protected override MethodBase GetTargetMethod()
        {
            _getExfiltrationActions = AccessTools.FirstMethod(
                typeof(GetActionsClass),
                method =>
                method.GetParameters()[0].Name == "owner" &&
                method.GetParameters()[1].ParameterType == typeof(ExfiltrationPoint)
            );

            return AccessTools.FirstMethod(typeof(GetActionsClass), method => method.Name == nameof(GetActionsClass.GetAvailableActions) && method.GetParameters()[0].Name == "owner");
        }

        [PatchPrefix]
        public static bool PatchPrefix(object[] __args, ref ActionsReturnClass __result)
        {
            var owner = __args[0] as GamePlayerOwner;
            var interactive = __args[1]; // as GInterface114 as of SPT 3.9.5

            if (interactive is ExfiltrationPoint && InteractableExfilsService.ExfilHasRequirement((ExfiltrationPoint)interactive, ERequirementState.TransferItem))
            {
                var exfil = interactive as ExfiltrationPoint;

                ActionsReturnClass vanillaExfilActions = _getExfiltrationActions.Invoke(null, __args) as ActionsReturnClass;
                var player = Singleton<GameWorld>.Instance.MainPlayer;
                OnActionsAppliedResult eventResult = Singleton<InteractableExfilsService>.Instance.OnActionsApplied(exfil, player.Side);

                List<ActionsTypesClass> actions = new List<ActionsTypesClass>();
                if (vanillaExfilActions != null)
                {
                    actions.AddRange(vanillaExfilActions.Actions);
                }
                actions.AddRange(CustomExfilAction.GetActionsTypesClassList(eventResult.Actions));

                __result = new ActionsReturnClass { Actions = actions };

                return false;
            }
            return true;
        }
    }
}
