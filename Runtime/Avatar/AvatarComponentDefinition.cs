using UnityEngine;

namespace VeryRealHelp.HelpClubCommon.Avatar
{
    [CreateAssetMenu(fileName = "Avatar Component", menuName = "Very Real Help/Avatar/Avatar Component Definition")]
    public class AvatarComponentDefinition : ScriptableObject
    {
        public AvatarComponentSlot slot;
        public GameObject prefab;
    }
}