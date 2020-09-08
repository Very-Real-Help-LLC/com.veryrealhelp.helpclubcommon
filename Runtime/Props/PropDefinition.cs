using UnityEngine;

namespace VeryRealHelp.HelpClubCommon.Prop
{
    [CreateAssetMenu(fileName = "Prop Definition", menuName = "Very Real Help/Prop/PropDefinition")]
    public class PropDefinition : ScriptableObject
    {
        public PropType Type;
        public PropFollowType FollowType;
        public PropComplexity Complexity;
        public GameObject Prefab;
        public Vector3 Offset;
    }
}
