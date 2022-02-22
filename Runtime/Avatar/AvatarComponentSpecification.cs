using UnityEngine;

namespace VeryRealHelp.HelpClubCommon.Avatar
{
    public class AvatarComponentSpecification : MonoBehaviour
    {
        [System.Serializable]
        public class SlotTransformOverride
        {
            public AvatarComponentSlot slot;
            public Transform transform;
        }

        public AvatarComponentSlot slot;
        public SlotTransformOverride[] slotTransformOverrides;
        public Sprite thumbnail;
    }
}