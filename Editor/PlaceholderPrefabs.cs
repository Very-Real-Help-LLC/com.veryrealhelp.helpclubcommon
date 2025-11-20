using UnityEngine;

namespace VeryRealHelp.HelpClubCommon.Editor
{
    [CreateAssetMenu(fileName = "PlaceholderPrefabs", menuName = "VRH/Placeholder Prefabs")]
    public class PlaceholderPrefabs : ScriptableObject
    {
        [Header("Content Station")]
        public GameObject contentStationPrefab;        
        private static PlaceholderPrefabs _instance;
        public static PlaceholderPrefabs Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<PlaceholderPrefabs>("PlaceholderPrefabs");
                }
                return _instance;
            }
        }
    }
}