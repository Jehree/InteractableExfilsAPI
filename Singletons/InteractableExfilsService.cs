using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.UI;
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
    public class OnActionsAppliedResult
    {
        public bool ExtractionToggleAvailable;
        public List<ActionsTypesClass> Actions { get; private set; } = new List<ActionsTypesClass>();

        public OnActionsAppliedResult(bool extractionToggleAvailable = true)
        {
            ExtractionToggleAvailable = extractionToggleAvailable;
        }
    }

    public class InteractableExfilsService
    {
        public delegate OnActionsAppliedResult ActionsAppliedEventHandler(ExfiltrationPoint exfil);

        // other mods can subscribe to this event and optionally pass ActionsTypesClass(es) back to be added to the interactable objects
        public event ActionsAppliedEventHandler OnActionsAppliedEvent;

        public virtual OnActionsAppliedResult OnActionsApplied(ExfiltrationPoint exfil)
        {
            OnActionsAppliedResult result = new OnActionsAppliedResult();

            if (OnActionsAppliedEvent == null) return result;

            foreach (ActionsAppliedEventHandler handler in OnActionsAppliedEvent.GetInvocationList())
            {
                OnActionsAppliedResult handlerResult = handler(exfil);
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
            bool result = false;

            foreach (var req in exfil.Requirements)
            {
                if (req.Requirement == requirement) result = true;
                break;
            }

            return result;
        }
    }
}
