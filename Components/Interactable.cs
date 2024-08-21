using EFT.Interactive;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace InteractableExfilsAPI.Components
{

    public class CustomInteractable : InteractableObject
    {
        public List<ActionsTypesClass> Actions = new List<ActionsTypesClass>();
    }

    public class ExfilInteractable : CustomInteractable
    {
        public ExfiltrationPoint Exfil { get; private set; }
        public bool Enabled { get; private set; }

        public void SetExfil(ExfiltrationPoint exfil)
        {
            Exfil = exfil;
        }
        public void AddExtractToggleAction()
        {
            Actions.Insert(0, new ActionsTypesClass
            {
                Name = "Toggle Extraction Zone",
                Action = () =>
                {
                    ToggleExfilZoneEnabled();
                }
            });
        }

        private void EnableExfilZone()
        {
            Exfil.gameObject.GetComponent<BoxCollider>().enabled = true;
            Enabled = true;
        }

        private void DisableExfilZone()
        {
            Exfil.gameObject.GetComponent<BoxCollider>().enabled = false;
            Enabled = false;
        }

        public void SetExfilZoneEnabled(bool enabled)
        {
            if (enabled)
            {
                DisableExfilZone();
            }
            else
            {
                EnableExfilZone();
            }
        }

        public void ToggleExfilZoneEnabled()
        {
            if (Enabled)
            {
                DisableExfilZone();
            }
            else
            {
                EnableExfilZone();
            }
        }
    }
}
