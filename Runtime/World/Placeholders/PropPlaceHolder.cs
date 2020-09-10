using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VeryRealHelp.HelpClubCommon.World
{
    public class PropPlaceHolder : Placeholder
    {
        public GameObject PropPrefab;
        //TO DO: Ensure that is a unqiue PROP ID by the tool
        public int PropID;
    }
}
