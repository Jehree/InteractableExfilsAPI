using EFT;
using EFT.Interactive;
using EFT.UI;
using InteractableExfilsAPI.Components;
using InteractableExfilsAPI.Singletons;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace InteractableExfilsAPI.Patches
{
    internal class GetAvailableActionsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // Delegate cast resolves at compile time, build errors rather than runtime surprises.
            return ((Func<GamePlayerOwner, IInteractive, AvailableInteractionState>)
                InteractionContextHelper.GetAvailableActions).Method;
        }

        [PatchPrefix]
        protected static bool PatchPrefix(GamePlayerOwner owner, IInteractive interactive, ref AvailableInteractionState __result)
        {
            if (IsInteractableExfil(interactive))
            {
                ExfiltrationPoint exfil = GetExfilPointFromInteractive(interactive);
                if (exfil == null)
                {
                    Plugin.LogSource.LogError("Cannot retrieve exfil point from interactive");
                    return true;
                }

                List<InteractionAction> vanillaActions = GetVanillaInteractionActions(owner, interactive);
                CustomExfilTrigger customTrigger = CreateCustomExfilTrigger(exfil, vanillaActions);
                AvailableInteractionState prompt = customTrigger.CreateExfilPrompt();

                __result = prompt;
                return false;
            }

            return true;
        }

        // vanilla interactable exfils (elevator exfils and saferoom exfil)
        private static bool IsInteractableExfil(IInteractive interactive)
        {
            // 1. check for car exfils
            if (interactive is ExfiltrationPoint point)
            {
                return InteractableExfilsService.IsExfilShared(point);
            }

            // 2. check for other exfils (based on a switch)
            if (interactive is Switch @switch)
            {
                if (InteractableExfilsService.IsExfilSwitchLabElevator(@switch)) return true;
                if (InteractableExfilsService.IsExfilSwitchInterchangeSafeRoom(@switch)) return true;
            }

            return false;
        }

        private static ExfiltrationPoint GetExfilPointFromInteractive(IInteractive interactive)
        {
            if (interactive is Switch @switch) return @switch.ExfiltrationPoint;
            if (interactive is ExfiltrationPoint point) return point;

            return null;
        }

        private static List<InteractionAction> GetVanillaInteractionActions(GamePlayerOwner gamePlayerOwner, IInteractive interactive)
        {
            if (InteractableExfilsService.Instance().DisableVanillaActions)
            {
                return [];
            }

            AvailableInteractionState vanillaActions = interactive switch
            {
                ExfiltrationPoint point => InteractionContextHelper.GetAvailableActions(gamePlayerOwner, point),
                Switch @switch => InteractionContextHelper.GetAvailableActions(gamePlayerOwner, @switch),
                _ => null,
            };

            return vanillaActions?.Actions ?? [];
        }

        private static CustomExfilTrigger CreateCustomExfilTrigger(ExfiltrationPoint exfil, List<InteractionAction> vanillaActions)
        {
            // Create a new GameObject to attach the MonoBehaviour
            GameObject customTriggerObject = new GameObject("CustomExfilTrigger");

            // Add the CustomExfilTrigger component
            CustomExfilTrigger customTrigger = customTriggerObject.AddComponent<CustomExfilTrigger>();

            bool exfilIsActiveToPlayer = true;
            customTrigger.Init(exfil, exfilIsActiveToPlayer, vanillaActions);

            string message = $"InteractionContextHelper.GetAvailableActions patched for exfil {exfil.Settings.Name}!\n";
            ConsoleScreen.Log(message);
            Plugin.LogSource.LogInfo(message);

            return customTrigger;
        }
    }
}
