using Comfort.Common;
using EFT;
using EFT.Interactive;
using HarmonyLib;
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

            Plugin.LogSource.LogError(_getExfiltrationActions.Name);

            return AccessTools.FirstMethod(typeof(GetActionsClass), method => method.Name == nameof(GetActionsClass.GetAvailableActions) && method.GetParameters()[0].Name == "owner");
        }

        [PatchPrefix]
        public static bool PatchPrefix(object[] __args, ref ActionsReturnClass __result)
        {
            var owner = __args[0] as GamePlayerOwner;
            var interactive = __args[1]; // as GInterface114 as of SPT 3.9.5

            if (interactive is CustomInteractable)
            {
                var customInteractable = interactive as CustomInteractable;

                __result = new ActionsReturnClass()
                {
                    Actions = customInteractable.Actions
                };
                return false;
            }

            if (interactive is ExfiltrationPoint)
            {
                var exfil = interactive as ExfiltrationPoint;
                //if (!InteractableExfilsService.ExfilHasRequirement(exfil, ERequirementState.TransferItem)) return true;

                ActionsReturnClass vanillaExfilActions = _getExfiltrationActions.Invoke(null, __args) as ActionsReturnClass;
                OnActionsAppliedResult eventResult = Singleton<InteractableExfilsService>.Instance.OnActionsApplied(exfil);

                List<ActionsTypesClass> actions = vanillaExfilActions == null
                    ? eventResult.Actions
                    : vanillaExfilActions.Actions.Concat(eventResult.Actions).ToList();

                __result = new ActionsReturnClass
                {
                    Actions = actions
                };

                return false;
            }
            return true;
        }
    }
}
