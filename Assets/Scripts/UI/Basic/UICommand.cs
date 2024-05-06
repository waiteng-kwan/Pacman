using NaughtyAttributes;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace Client.UI
{
    [System.Serializable]
    public enum UIEventTypes
    {
        Open,
        Close,
        Pop,       //back
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
        public List<Object> EventList = new();

        public void Execute()
        {
            switch (Type)
            {
                //open
                case UIEventTypes.Open:
                    foreach (var obj in EventList)
                    {
                        var transition = obj.GetComponent<UITransition>();

                        if (transition)
                            transition.DoTransitionIn();
                        else
                        {
                            GameObject go = obj as GameObject;
                            go.SetActive(true);
                        }
                    }
                    break;
                //transit out, set inactive etc
                case UIEventTypes.Close:
                    foreach (var obj in EventList)
                    {
                        var transition = obj.GetComponent<UITransition>();

                        if (transition)
                            transition.DoTransitionOut();
                        else
                        {
                            GameObject go = obj as GameObject;
                            go.SetActive(false);
                        }
                    }
                    break;
                case UIEventTypes.Pop:
                    foreach (var obj in EventList)
                    {
                        var transition = obj.GetComponent<UITransition>();

                        if (transition)
                            transition.DoTransitionOut();
                    }
                    break;
                //fire whatever is inside
                case UIEventTypes.FireEvent:
                    foreach (var e in EventList)
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

        public void Execute()
        {
            foreach (var command in CommandList)
            {
                command?.Execute();
            }
        }
    }
}