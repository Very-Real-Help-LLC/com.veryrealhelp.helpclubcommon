using System;
using UnityEngine;
using UnityEngine.Events;

namespace VeryRealHelp.HelpClubCommon.Actions
{
    [Flags]
    public enum ActionType
    {
        Activate = 1 << 0,
        Destroy = 1 << 1,
        Modify = 1 << 2,
        LockToggle = 1 << 3,
        Everything = 1 << 4,
        Report = 1 << 5,
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

    public class ModifyHandlerConfig : HandlerConfig
    {
        public override ActionType ActionTypeMask => ActionType.Modify;
    }
    public class LockToggleHandlerConfig : HandlerConfig
    {
        public override ActionType ActionTypeMask => ActionType.LockToggle;
    }
}
