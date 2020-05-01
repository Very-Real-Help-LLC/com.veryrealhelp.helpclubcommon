using System;
using UnityEngine;

namespace VeryRealHelp.HelpClubCommon.Actions
{
    public class DestroyActionHandlerSpec : ActionHandlerSpec
    {
        public override Type Type => typeof(DestroyActionHandlerSpec);
        public override ActionType ActionTypeMask => ActionType.Destroy;

        public bool destroySelf = true;
        public GameObject[] objectsToDestroy;
    }
}
