using System;
using UnityEngine;

namespace VeryRealHelp.HelpClubCommon.Actions
{
    public class ModifyActionHandlerSpec : ActionHandlerSpec
    {
        public override Type Type => typeof(ModifyActionHandlerSpec);
        public override ActionType ActionTypeMask => ActionType.Modify;
    }
}
