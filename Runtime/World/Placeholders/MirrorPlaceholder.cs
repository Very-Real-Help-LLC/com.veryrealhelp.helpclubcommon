using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VeryRealHelp.HelpClubCommon.World
{
    [ExecuteInEditMode]
    public class MirrorPlaceholder : Placeholder
    {
        private static readonly Vector3 mirrorSize = new Vector3(2.56f, 2.56f, 0.025f);
        public Texture mask;

#if UNITY_EDITOR
        private GameObject maskPreviewGameObject;
        private RawImage maskPreviewImage;


        private void Update()
        {
            if (mask == null)
            {
                if(maskPreviewGameObject != null)
                {
                    DestroyImmediate(maskPreviewGameObject);
                }
            }
            else
            {
                if(maskPreviewGameObject == null)
                {
                    maskPreviewGameObject = new GameObject("MaskPreviewCanvas", typeof(Canvas), typeof(RawImage));
                    maskPreviewGameObject.transform.SetParent(transform);
                    Canvas maskPreviewCanvas = maskPreviewGameObject.GetComponent<Canvas>();
                    maskPreviewCanvas.renderMode = RenderMode.WorldSpace;
                    RectTransform maskPreviewCanvasRectTransform = maskPreviewCanvas.GetComponent<RectTransform>();
                    maskPreviewCanvasRectTransform.pivot = new Vector2(0.5f, 0f);
                    maskPreviewCanvasRectTransform.sizeDelta = new Vector2(512, 512);
                    maskPreviewImage = maskPreviewGameObject.GetComponent<RawImage>();
                    maskPreviewImage.texture = mask;
                    maskPreviewGameObject.transform.localPosition = Vector3.zero;
                    maskPreviewGameObject.transform.localRotation = Quaternion.identity;
                    maskPreviewGameObject.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
                }
                else if (maskPreviewImage.texture != mask)
                {
                    maskPreviewImage.texture = mask;
                }
            }
        }

        public void OnDrawGizmos()
        {
            if (isActiveAndEnabled)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Color col = Color.magenta;
                if(mask != null)
                {
                    col.a = 0.1f;
                }
                Gizmos.color = col;
                Gizmos.DrawCube(Vector3.zero + mirrorSize.y * 0.5f * Vector3.up, mirrorSize);

                Handles.Label(transform.position + mirrorSize.y * 1.1f * Vector3.up, "Mirror");
            }
        }
#endif
    }
}