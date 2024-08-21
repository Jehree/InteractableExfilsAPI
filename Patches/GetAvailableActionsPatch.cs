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

            /*
            if (interactive is ExfiltrationPoint)
            {
                Plugin.LogSource.LogError("1");
                ActionsReturnClass vanillaExfilActions = _getExfiltrationActions.Invoke(null, __args) as ActionsReturnClass;
                Plugin.LogSource.LogError(vanillaExfilActions.Actions[0].Name);
                Plugin.LogSource.LogError("2");
                OnActionsAppliedResult eventResult = Singleton<InteractableExfilsService>.Instance.OnActionsApplied((ExfiltrationPoint)interactive);

                Plugin.LogSource.LogError("3");
                __result = new ActionsReturnClass
                {
                    Actions = vanillaExfilActions.Actions.Concat(eventResult.Actions).ToList()
                };
                Plugin.LogSource.LogError("");
                return false;
            }
            
            */
            return true;
        }
    }
}
