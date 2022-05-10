using System.Collections.Generic;
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

        public AvatarComponentSlot slot;
        public SlotTransformOverride[] slotTransformOverrides;
        public Sprite thumbnail;

        private Renderer[] renderers;
        private Dictionary<Renderer, Material> cachedMaterials;

        public Renderer[] Renderers()
        {
            if(renderers == null)
            {
                renderers = GetComponentsInChildren<Renderer>();
                if (cachedMaterials == null)
                {
                    cachedMaterials = new Dictionary<Renderer, Material>();
                    for(int i = 0; i < renderers.Length; i++)
                    {
                        cachedMaterials.Add(renderers[i], renderers[i].material);
                    }
                }
            }
            return renderers;
        }
        public void ApplyCachedMaterial()
        {
            if(cachedMaterials == null || cachedMaterials.Count == 0)
            {
                return;
            }
            foreach(KeyValuePair<Renderer, Material> cachedRendMaterials in cachedMaterials)
            {
                cachedRendMaterials.Key.material = cachedRendMaterials.Value;
            }
        }
    }
}