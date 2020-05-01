using System;
using UnityEngine;
using UnityEngine.Events;

namespace VeryRealHelp.HelpClubCommon.Actions
{
    [Flags]
    public enum ActionType
    {
        Activate = 1,
        Destroy = 2
    }

    public interface IActionHandler
    {
        void HandleAction(ActionType actionType);
    }
    
    public abstract class HandlerConfig
    {
        public abstract ActionType ActionTypeMask { get; }
        public UnityEvent beforeHandling;
        public UnityEvent afterHandling;
    }

    public class DestroyHandlerConfig : HandlerConfig
    {
        public override ActionType ActionTypeMask => ActionType.Destroy;

        public bool destroySelf = true;
        public GameObject[] objectsToDestroy;
    }

    public class ActivateHandlerConfig : HandlerConfig
    {
        public override ActionType ActionTypeMask => ActionType.Activate;

        public UnityEvent onActivate;
     }
}
