using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.UI;
using HarmonyLib;
using InteractableExfilsAPI.Common;
using InteractableExfilsAPI.Components;
using InteractableExfilsAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private FieldInfo _exfilPlayersMetAllRequirementsFieldInfo = AccessTools.Field(typeof(ExfiltrationPoint), "_playersMetAllRequirements");


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

                if (Settings.DebugMode.Value)
                {
                    result.Actions.Add(GetDebugAction(exfil));
                }
            }
            return result;
        }

        public CustomExfilAction GetDebugAction(ExfiltrationPoint exfil)
        {
            return new CustomExfilAction(
                "Print Debug Info To Console",
                false,
                () =>
                {
                    var gameWorld = Singleton<GameWorld>.Instance;
                    var player = gameWorld.MainPlayer;

                    foreach (var req in exfil.Requirements)
                    {
                        ConsoleScreen.Log($"... {req.Requirement.ToString()}");
                    }
                    ConsoleScreen.Log($"Requirements: ");
                    ConsoleScreen.Log($"Chance: {exfil.Settings.Chance}");
                    ConsoleScreen.Log($"Exfil Id: {exfil.Settings.Name}");
                    ConsoleScreen.Log($"EXFIL INFO:\n");

                    ConsoleScreen.Log($"Map Id: {gameWorld.LocationId}");
                    ConsoleScreen.Log($"WORLD INFO:\n");

                    List<string> exfilNames = new List<string>();
                    foreach (var activeExfil in player.gameObject.GetComponent<InteractableExfilsSession>().ActiveExfils)
                    {
                        exfilNames.Add(activeExfil.Settings.Name);
                    }
                    string combinedString = string.Join(", ", exfilNames);
                    ConsoleScreen.Log(combinedString);
                    ConsoleScreen.Log($"Active Exfils:");
                    ConsoleScreen.Log($"Player Rotation (Quaternion): {player.CameraPosition.rotation}");
                    ConsoleScreen.Log($"Player Rotation (Euler): {player.CameraPosition.rotation.eulerAngles}");
                    ConsoleScreen.Log($"Player Position: {player.gameObject.transform.position}");
                    ConsoleScreen.Log($"Profile Side: {player.Side.ToString()}");
                    ConsoleScreen.Log($"Profile Id: {player.ProfileId}");
                    ConsoleScreen.Log($"PLAYER INFO:\n");

                    Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.TradeOperationComplete);
                }
            );
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

        public static bool ExfilIsElevator(ExfiltrationPoint exfil)
        {
            GameWorld gameWorld = Singleton<GameWorld>.Instance;
            if (exfil.Settings.Name.Contains("Elevator") && gameWorld.LocationId == "laboratory") return true;
            return false;
        }

        public void AddPlayerToPlayersMetAllRequirements(ExfiltrationPoint exfil, string profileId)
        {
            List<string> playerIdList = this._exfilPlayersMetAllRequirementsFieldInfo.GetValue(exfil) as List<string>;
            if (playerIdList.Contains(profileId)) return;
            playerIdList.Add(profileId);
            _exfilPlayersMetAllRequirementsFieldInfo.SetValue(exfil, playerIdList);
        }
    }
}
