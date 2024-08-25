using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.UI;
using InteractableExfilsAPI.Helpers;
using InteractableExfilsAPI.Singletons;
using Koenigz.PerfectCulling.EFT;
using SPT.Reflection.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static InteractableExfilsAPI.Singletons.InteractableExfilsService;
using static System.Collections.Specialized.BitVector32;

namespace InteractableExfilsAPI.Components
{
    public class InteractableExfilsSession : MonoBehaviour
    {
        public List<ExfiltrationPoint> ActiveExfils { get; private set; } = new List<ExfiltrationPoint>();
        public InteractableExfilsSession()
        {
            FillActiveExtractList();
            CreateAllCustomExfilTriggers();
        }

        private void CreateAllCustomExfilTriggers()
        {
            foreach (var exfil in ActiveExfils)
            {
                CreateCustomExfilTriggerObject(exfil);
            }
        }

        private void CreateCustomExfilTriggerObject(ExfiltrationPoint exfil)
        {
            if (ExfilIsCar(exfil) || InteractableExfilsService.ExfilIsElevator(exfil)) return; // these are handled in the GetAvailableActionsPatch

            BoxCollider sourceCollider = exfil.gameObject.GetComponent<BoxCollider>();

            GameObject customExfilTriggerObject = new GameObject();
            customExfilTriggerObject.transform.parent = exfil.gameObject.transform; // this makes sure we are destroyed before the exfil zone is
            customExfilTriggerObject.name = exfil.Settings.Name + "_custom_trigger";
            customExfilTriggerObject.layer = LayerMask.NameToLayer("Triggers");

            BoxCollider targetCollider = customExfilTriggerObject.AddComponent<BoxCollider>();
            targetCollider.center = sourceCollider.center;
            targetCollider.size = sourceCollider.size;
            targetCollider.isTrigger = sourceCollider.isTrigger;

            customExfilTriggerObject.transform.position = exfil.gameObject.transform.position;
            customExfilTriggerObject.transform.rotation = exfil.gameObject.transform.rotation;
            customExfilTriggerObject.transform.localScale = exfil.gameObject.transform.localScale;
            CustomExfilTrigger customExfilTrigger = customExfilTriggerObject.AddComponent<CustomExfilTrigger>();
            customExfilTrigger.SetExfil(exfil);

            var player = Singleton<GameWorld>.Instance.MainPlayer;
            OnActionsAppliedResult eventResult = Singleton<InteractableExfilsService>.Instance.OnActionsApplied(exfil, player.Side);

            if (eventResult.ExtractionToggleAvailable)
            {
                customExfilTrigger.AddExtractToggleAction();
            }

            customExfilTrigger.Actions.AddRange(eventResult.Actions);
        }

        private void FillActiveExtractList()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            var player = gameWorld.MainPlayer;

            ExfiltrationPoint[] exfils = player.Side == EPlayerSide.Savage
                ? gameWorld.ExfiltrationController.ScavExfiltrationPoints
                : gameWorld.ExfiltrationController.ExfiltrationPoints;

            foreach (var exfil in exfils)
            {
                if (!exfil.InfiltrationMatch(gameWorld.MainPlayer)) continue;
                ActiveExfils.Add(exfil);
            }
        }

        private bool ExfilIsCar(ExfiltrationPoint exfil)
        {
            if (InteractableExfilsService.ExfilHasRequirement(exfil, ERequirementState.TransferItem)) return true;
            return false;
        }
    }
}
