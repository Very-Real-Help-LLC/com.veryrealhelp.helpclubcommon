using System;
using UnityEngine;

namespace VeryRealHelp.HelpClubCommon.Actions
{
    public class LockToggleActionHandlerSpec : ActionHandlerSpec
    {
        public override Type Type => typeof(LockToggleActionHandlerSpec);
        public override ActionType ActionTypeMask => ActionType.LockToggle;
    }
}
