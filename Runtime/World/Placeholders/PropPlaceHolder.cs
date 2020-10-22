using UnityEngine;
using System.Linq;
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

        public void OnDrawGizmos()
        {
            Gizmos.color = new Color(0, 1, 1, 1);
            PropPrefab.GetComponentsInChildren<MeshFilter>()
                .ToList()
                .ForEach(x => {
                    Gizmos.matrix = transform.localToWorldMatrix * x.transform.localToWorldMatrix;
                    Gizmos.DrawMesh(x.sharedMesh);
                });
        }
    }
}
