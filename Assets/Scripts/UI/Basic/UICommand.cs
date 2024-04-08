using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Client.UI
{
    [System.Serializable]
    public enum UIEventTypes
    {
        Open,
        Close,
        FireEvent,
        Custom
    }

    /// <summary>
    /// Inherit if need to make custom list
    /// </summary>
    [System.Serializable]
    public class UITypeToEvents
    {
        public UIEventTypes Type;
        // if its event, the things filled in should be scriptable objects
        public List<Object> ExecuteOn = new();

        public void Execute()
        {
            switch (Type)
            {
                //open
                case UIEventTypes.Open:
                    break;
                //transit out, set inactive etc
                case UIEventTypes.Close:
                    break;
                //fire whatever is inside
                case UIEventTypes.FireEvent:
                    foreach (var e in ExecuteOn)
                    {
                        if (e is UIEvent uiEvent)
                        {
                            uiEvent?.Execute();
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }

    [System.Serializable]
    public class UICommand
    {
        public List<UITypeToEvents> CommandList;

        void Execute()
        {
            foreach (var command in CommandList)
            {
                command?.Execute();
            }
        }
    }
}