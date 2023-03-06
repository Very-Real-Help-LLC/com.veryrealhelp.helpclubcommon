using UnityEngine;

namespace VeryRealHelp.HelpClubCommon.Prop
{
    [CreateAssetMenu(fileName = "Prop Definition", menuName = "Very Real Help/Prop/PropDefinition")]
    public class PropDefinition : ScriptableObject
    {
        public int Id;

        public enum PropType
        {
            Default = 1,
            UnstealableTest = 2,
            UsableTest = 3,
            TextProp = 4,
            HelperProp = 5,
            TextHelperProp = 6,
            DeckProp = 7,
            CardProp = 8
        }
        public enum PropFollowType
        {
            FollowOrientation = 1,
            FixedOrientation = 2 
        }
        public enum PropCategory
        {
            Game = 1,
            HelperOnly = 2,
            SMART = 3,
            DBT = 4,
            CBT = 5,
            Cards = 6
        }
        public enum PropPhysicsType
        {
            Floating = 1,
            PhysicsEnabled = 2
        }
        public PropType Type;
        public PropFollowType FollowType;
        public PropCategory Category;
        public PropPhysicsType PhysicsType;
        public GameObject Prefab;
        public CardDefinition CardSet;
        public Sprite PropPreviewImage;
        public bool isPremium;
    }
}
