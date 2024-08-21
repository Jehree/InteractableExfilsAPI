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
                    ToggleEnabled();
                }
            });
        }

        private void Enable()
        {
            gameObject.GetComponent<BoxCollider>().enabled = true;
            Enabled = true;
        }

        private void Disable()
        {
            gameObject.GetComponent<BoxCollider>().enabled = false;
            Enabled = false;
        }

        public void SetEnabled(bool enabled)
        {
            if (enabled)
            {
                Disable();
            }
            else
            {
                Enable();
            }
        }

        public void ToggleEnabled()
        {
            if (Enabled)
            {
                Disable();
            }
            else
            {
                Enable();
            }
        }
    }
}
