﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractableExfilsAPI.Common
{
    public class CustomExfilAction
    {
        public Action Action;
        public Func<string> GetName { get; private set; }
        public Func<bool> GetDisabled { get; private set; }

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

        public ActionsTypesClass GetActionsTypesClass()
        {
            return new ActionsTypesClass
            {
                Action = Action,
                Name = GetName(),
                Disabled = GetDisabled(),
            };
        }

        public static List<ActionsTypesClass> GetActionsTypesClassList(List<CustomExfilAction> CustomExfilActionList)
        {
            List<ActionsTypesClass> actionsTypesClassList = new List<ActionsTypesClass>();

            foreach (CustomExfilAction CustomExfilAction in CustomExfilActionList)
            {
                actionsTypesClassList.Add(CustomExfilAction.GetActionsTypesClass());
            }

            return actionsTypesClassList;
        }
    }

}
