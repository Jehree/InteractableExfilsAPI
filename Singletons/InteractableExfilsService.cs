using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.UI;
using InteractableExfilsAPI.Common;
using InteractableExfilsAPI.Components;
using InteractableExfilsAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace InteractableExfilsAPI.Singletons
{
    /// <summary>
    /// Event result containing actions, etc.<br/>
    /// Return <see cref="null"/> to skip adding actions
    /// </summary>
    public class OnActionsAppliedResult
    {
        public bool ExtractionToggleAvailable;
        public List<CustomExfilAction> Actions { get; private set; }

        public OnActionsAppliedResult()
        {
            Actions = new List<CustomExfilAction>();
            ExtractionToggleAvailable = true;
        }

        public OnActionsAppliedResult(CustomExfilAction action, bool extractionToggleAvailable = true)
        {
            Actions = new List<CustomExfilAction>();
            ExtractionToggleAvailable = extractionToggleAvailable;
            if (action != null)
            {
                Actions.Add(action);
            }
        }

        public OnActionsAppliedResult(List<CustomExfilAction> actions, bool extractionToggleAvailable = true)
        {
            Actions = new List<CustomExfilAction>();
            ExtractionToggleAvailable = extractionToggleAvailable;
            Actions.AddRange(actions);
        }
    }

    public class InteractableExfilsService
    {
        public delegate OnActionsAppliedResult ActionsAppliedEventHandler(ExfiltrationPoint exfil, EPlayerSide side);

        // other mods can subscribe to this event and optionally pass ActionsTypesClass(es) back to be added to the interactable objects
        public event ActionsAppliedEventHandler OnActionsAppliedEvent;

        public virtual OnActionsAppliedResult OnActionsApplied(ExfiltrationPoint exfil, EPlayerSide side)
        {
            OnActionsAppliedResult result = new OnActionsAppliedResult();
            if (OnActionsAppliedEvent == null) return result;

            foreach (ActionsAppliedEventHandler handler in OnActionsAppliedEvent.GetInvocationList())
            {
                OnActionsAppliedResult handlerResult = handler(exfil, side);
                if (handlerResult == null) continue;

                result.Actions.AddRange(handlerResult.Actions);
                if (handlerResult.ExtractionToggleAvailable == false)
                {
                    result.ExtractionToggleAvailable = false;
                }
            }
            return result;
        }

        public static InteractableExfilsSession GetSession()
        {
            if (!Singleton<GameWorld>.Instantiated)
            {
                string errorMsg = "Tried to get InteractableExfilsSession while GameWorld singleton was not instantiated.";
                Plugin.LogSource.LogError(errorMsg);
                ConsoleScreen.LogError(errorMsg);
                return null;
            }

            var session = Singleton<GameWorld>.Instance.MainPlayer.gameObject.GetComponent<InteractableExfilsSession>();

            if (session == null)
            {
                string errorMsg = "Failed to get InteractableExfilsSession component from player, was it added correctly?";
                Plugin.LogSource.LogError(errorMsg);
                ConsoleScreen.LogError(errorMsg);
                return null;
            }

            return session;
        }

        public static bool ExfilHasRequirement(ExfiltrationPoint exfil, ERequirementState requirement)
        {
            foreach (var req in exfil.Requirements)
            {
                if (req.Requirement == requirement) return true;
            }
            return false;
        }
    }
}
