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

        [System.Serializable]
        public class ColorReceiver
        {
            public AvatarComponentColorSlot slot;
            public Renderer renderer;
            public string shaderPropertyName = "_Color";
        }

        public AvatarComponentSlot slot;
        public SlotTransformOverride[] slotTransformOverrides;
        public ColorReceiver[] colorReceivers;
    }
}