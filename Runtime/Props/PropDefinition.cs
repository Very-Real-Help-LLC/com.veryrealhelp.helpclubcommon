using UnityEngine;

namespace VeryRealHelp.HelpClubCommon.Prop
{
    [CreateAssetMenu(fileName = "Prop Definition", menuName = "Very Real Help/Prop/PropDefinition")]
    public class PropDefinition : ScriptableObject
    {
        public bool IsUsable;
        public GameObject Prefab;
    }
}
