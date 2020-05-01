using System;
using UnityEngine;
using UnityEngine.Events;

namespace VeryRealHelp.HelpClubCommon.Actions
{
    public abstract class ActionHandlerSpec : MonoBehaviour
    {
        public abstract Type Type { get; }
        public abstract ActionType ActionTypeMask { get; }
        public UnityEvent beforeHandling;
        public UnityEvent afterHandling;
    }
}
