using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.UI;
using InteractableExfilsAPI.Helpers;
using InteractableExfilsAPI.Singletons;
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
        public MapData MapData { get; private set; }
        public Dictionary<string, ExfiltrationPoint> ActiveExfils { get; private set; } = new Dictionary<string, ExfiltrationPoint>();

        public InteractableExfilsSession()
        {
            InitMapData();
            if (MapData == null)
            {
                string err = $"{Plugin.MOD_NAME}: No map data found for this map!";
                ConsoleScreen.Log(err);
                Plugin.LogSource.LogError(err);
                return;
            }

            FillActiveExtractList();
            CreateAllInteractableObjects();
        }

        private void CreateAllInteractableObjects()
        {
            foreach (var obj in MapData.Objects)
            {
                if (!ActiveExfils.ContainsKey(obj.Name)) continue;
                ExfiltrationPoint exfil = ActiveExfils[obj.Name];
                CreateInteractableObject(obj, exfil);
            }
        }

        private void FillActiveExtractList()
        {
            foreach (var point in Singleton<GameWorld>.Instance.ExfiltrationController.ExfiltrationPoints)
            {
                ActiveExfils.Add(point.Settings.Name, point);
            }
        }

        private void CreateInteractableObject(ObjectData obj, ExfiltrationPoint exfil)
        {
            foreach (var req in exfil.Requirements)
            {
                if (req.Requirement == ERequirementState.TransferItem) return;
            }

            GameObject gameObject = new GameObject();
            gameObject.AddComponent<BoxCollider>();
            var interactable = gameObject.AddComponent<ExfilInteractable>();
            interactable.SetEnabled(!Settings.ExtractAreaStartsDisabled.Value);
            interactable.SetExfil(exfil);

            gameObject.transform.position = obj.Position;
            gameObject.transform.rotation = obj.Rotation;
            gameObject.transform.localScale = obj.Scale;
            gameObject.layer = LayerMask.NameToLayer("Interactive");

            OnActionsAppliedResult eventResult = Singleton<InteractableExfilsService>.Instance.OnActionsApplied(exfil);

            if (eventResult.ExtractionToggleAvailable)
            {
                interactable.AddExtractToggleAction();
            }

            interactable.Actions.AddRange(eventResult.Actions);
        }

        private void InitMapData()
        {
            if (!Singleton<GameWorld>.Instantiated)
            {
                string errorMessage = "InteractableExfilsController tried to fetch map data when GameWorld singleton was not instantiated!";
                Plugin.LogSource.LogError(errorMessage);
                ConsoleScreen.LogError(errorMessage);
                Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ErrorMessage);
                return;
            }

            string locId = Singleton<GameWorld>.Instance.LocationId;
            MapData = MapData.Get(locId);
        }
    }
}
