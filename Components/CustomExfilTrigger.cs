using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.UI;
using InteractableExfilsAPI.Common;
using InteractableExfilsAPI.Helpers;
using InteractableExfilsAPI.Singletons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InteractableExfilsAPI.Components
{
    public class CustomExfilTrigger : MonoBehaviour, IPhysicsTrigger
    {
        public List<CustomExfilAction> Actions = new List<CustomExfilAction>();
        public ExfiltrationPoint Exfil { get; private set; }
        public string Description { get; } = "Custom Trigger";
        private bool _playerInTriggerArea = false;
        public bool ExfilEnabled { get; private set; } = true;

        public void Update()
        {
            if (!_playerInTriggerArea) return;
            Player player = Singleton<GameWorld>.Instance.MainPlayer;
            GamePlayerOwner gamePlayerOwner = player.GetComponent<GamePlayerOwner>();
            if (gamePlayerOwner.AvailableInteractionState.Value != null) return;

            var returnClass = new ActionsReturnClass { Actions = CustomExfilAction.GetActionsTypesClassList(Actions) };
            if (Settings.DebugMode.Value)
            {
                returnClass.Actions.Add(GetDebugAction().GetActionsTypesClass());
            }
            returnClass.InitSelected();

            gamePlayerOwner.AvailableInteractionState.Value = returnClass;
        }

        public void OnTriggerEnter(Collider collider)
        {
            Player player = Singleton<GameWorld>.Instance.GetPlayerByCollider(collider);
            if (player == Singleton<GameWorld>.Instance.MainPlayer)
            {
                _playerInTriggerArea = true;
                SetExfilZoneEnabled(Settings.ExtractAreaStartsEnabled.Value);
            }
        }

        public void OnTriggerExit(Collider collider)
        {
            Player player = Singleton<GameWorld>.Instance.GetPlayerByCollider(collider);
            if (player == Singleton<GameWorld>.Instance.MainPlayer)
            {
                _playerInTriggerArea = false;
                GamePlayerOwner gamePlayerOwner = player.GetComponent<GamePlayerOwner>();
                gamePlayerOwner.ClearInteractionState();
                SetExfilZoneEnabled(true);
            }
        }

        public void SetExfil(ExfiltrationPoint exfil)
        {
            Exfil = exfil;
        }

        public void AddExtractToggleAction()
        {
            Actions.Insert(0, new CustomExfilAction(
                "Extract",
                Exfil.UnmetRequirements(Singleton<GameWorld>.Instance.MainPlayer).ToArray<ExfiltrationRequirement>().Any<ExfiltrationRequirement>,
                ToggleExfilZoneEnabled
            ));
        }

        public CustomExfilAction GetDebugAction()
        {
            return new CustomExfilAction(
                "Print Debug Info To Console",
                false,
                () =>
                {
                    var gameWorld = Singleton<GameWorld>.Instance;
                    var player = gameWorld.MainPlayer;

                    foreach (var req in Exfil.Requirements)
                    {
                        ConsoleScreen.Log($"... {req.Requirement.ToString()}");
                    }
                    ConsoleScreen.Log($"Requirements: ");
                    ConsoleScreen.Log($"Chance: {Exfil.Settings.Chance}");
                    ConsoleScreen.Log($"Exfil Id: {Exfil.Settings.Name}");
                    ConsoleScreen.Log($"EXFIL INFO:\n");

                    ConsoleScreen.Log($"Map Id: {gameWorld.LocationId}");
                    ConsoleScreen.Log($"WORLD INFO:\n");

                    List<string> exfilNames = new List<string>();
                    foreach ( var exfil in player.gameObject.GetComponent<InteractableExfilsSession>().ActiveExfils)
                    {
                        exfilNames.Add(exfil.Settings.Name);
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

        private void EnableExfilZone()
        {
            Exfil.gameObject.GetComponent<BoxCollider>().enabled = true;
            ExfilEnabled = true;
            
        }

        private void DisableExfilZone(bool mute = false)
        {
            Exfil.gameObject.GetComponent<BoxCollider>().enabled = false;
            ExfilEnabled = false;
            
        }

        public void SetExfilZoneEnabled(bool enabled)
        {
            if (enabled)
            {
                EnableExfilZone();
            }
            else
            {
                DisableExfilZone();
            }
        }

        public void ToggleExfilZoneEnabled()
        {
            if (ExfilEnabled)
            {
                DisableExfilZone();
                Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.GeneratorTurnOff);
            }
            else
            {
                EnableExfilZone();
                Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.GeneratorTurnOn);
            }
        }
    }
}
