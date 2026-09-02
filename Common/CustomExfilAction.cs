using EFT.UI;
using System;
using System.Collections.Generic;

namespace InteractableExfilsAPI.Common
{
    public class CustomExfilAction
    {
        public Action Action { get; set; }
        public Func<string> GetName { get; set; }
        public Func<bool> GetDisabled { get; set; }

        /// <summary>
        /// Custom Exfil Action to be added to an interactable Exfil interaction prompt<br/>
        /// </summary>
        /// <remarks>
        /// name can be a <see cref="string"/> or a <see cref="Func{}"/> that returns <see cref="string"/><br/>
        /// disabled can be a <see cref="bool"/> or a <see cref="Func{}"/> that returns <see cref="bool"/><br/>
        /// </remarks>
        public CustomExfilAction(string name, bool disabled, Action action) // simple static name and disabled
        {
            GetName = () => name;
            GetDisabled = () => disabled;
            Action = action;
        }

        public CustomExfilAction(Func<string> getName, Func<bool> getDisabled, Action action) // dynamic simple and static
        {
            GetName = getName;
            GetDisabled = getDisabled;
            Action = action;
        }
        
        public CustomExfilAction(string name, Func<bool> getDisabled, Action action) // simple name, dynamic disabled
        {
            GetName = () => name;
            GetDisabled = getDisabled;
            Action = action;
        }

        public CustomExfilAction(Func<string> getName, bool disabled, Action action) // dynamic name, simple disabled
        {
            GetName = getName;
            GetDisabled = () => disabled;
            Action = action;
        }

        public InteractionAction GetInteractionAction()
        {
            return new InteractionAction
            {
                Action = Action,
                Name = GetName(),
                Disabled = GetDisabled(),
            };
        }

        public static List<InteractionAction> GetInteractionActionList(List<CustomExfilAction> CustomExfilActionList)
        {
            List<InteractionAction> interactionActionList = new List<InteractionAction>();

            foreach (CustomExfilAction CustomExfilAction in CustomExfilActionList)
            {
                interactionActionList.Add(CustomExfilAction.GetInteractionAction());
            }

            return interactionActionList;
        }
    }

}
