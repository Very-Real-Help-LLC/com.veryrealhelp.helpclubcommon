using UnityEngine;
using System.Collections;

namespace VeryRealHelp.HelpClubCommon.Actions
{
    public class Targetable : MonoBehaviour
    {
        
        public ActionType validActions;

        [Header("Optional")]
        public string label;

        [SerializeField]
        private GameObject handler;

        public GameObject Handler => handler != null ? handler : gameObject;
        public string Label => label != null && label != "" ? label : Handler.name;

        public void Reset()
        {
            handler = gameObject;
        }

        public void Start()
        {
            HelpClubCommon.targetableCreated.Invoke(this);
        }
    }
}
