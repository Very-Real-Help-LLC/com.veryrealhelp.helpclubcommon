using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VeryRealHelp.HelpClubCommon.World
{
    public class BulletinboardPlaceholder : Placeholder
    {
        private Vector3 menuPanelOffset = new Vector3(-0.8226418f, 0.3027542f, 0.2661361f);
        private Vector3 bulletinboardBounds = new Vector3(3.5f, 1.8f, 0.05f);
#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            if (isActiveAndEnabled)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.color = Color.Lerp(Color.red, Color.black, 0.75f);
                Gizmos.DrawCube(menuPanelOffset, bulletinboardBounds);
            }
        }
#endif
    }
}