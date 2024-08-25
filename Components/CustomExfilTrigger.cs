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

        public void FixedUpdate()
        {
            if (!_playerInTriggerArea) return;
            Player player = Singleton<GameWorld>.Instance.MainPlayer;
            GamePlayerOwner gamePlayerOwner = player.GetComponent<GamePlayerOwner>();
            if (gamePlayerOwner.AvailableInteractionState.Value != null) return;

            var returnClass = new ActionsReturnClass { Actions = CustomExfilAction.GetActionsTypesClassList(Actions) };
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
            /*
            Actions.Insert(0, new CustomExfilAction(
                "Extract",
                () =>
                {
                    var player = Singleton<GameWorld>.Instance.MainPlayer;
                    if (!Exfil.HasRequirements) return false;
                    if (Exfil.HasMetRequirements(player.ProfileId)) return false;
                    //if (Exfil.UnmetRequirements(Singleton<GameWorld>.Instance.MainPlayer).ToArray<ExfiltrationRequirement>().Any<ExfiltrationRequirement>())
                    //-116.9174 -18 169.2975
                    // labs ele switch -276.1301 -2.3366 -364.7404
                    // labs sewer -131.3852 -5.4804 -266.646
                    // labs cargo switch -122.9479 -4 -355.3093
                    // labs cargo ele -118.9453 4 -406.0783
                    return true;
                },
                ToggleExfilZoneEnabled
            ));
            */

            Actions.Insert(0, new CustomExfilAction(
                "Extract",
                false,
                ToggleExfilZoneEnabled
            ));
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
            var player = Singleton<GameWorld>.Instance.MainPlayer;



            if (Exfil.HasRequirements && !Exfil.HasMetRequirements(player.ProfileId))
            {
                if (!Exfil.UnmetRequirements(player).ToArray<ExfiltrationRequirement>().Any<ExfiltrationRequirement>())
                {
                    Singleton<InteractableExfilsService>.Instance.AddPlayerToPlayersMetAllRequirements(Exfil, player.ProfileId);
                    ToggleExfilZoneEnabled();
                    return;
                }

                string tips = string.Join(", ", Exfil.GetTips(player.ProfileId));
                ConsoleScreen.Log($"You have not met the extract requirements for {Exfil.Settings.Name}!");
                NotificationManagerClass.DisplayWarningNotification($"{tips}");
                Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ErrorMessage);
                return;
            }

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
