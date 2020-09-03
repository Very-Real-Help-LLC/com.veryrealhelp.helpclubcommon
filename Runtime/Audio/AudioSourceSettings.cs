using UnityEngine;

namespace VeryRealHelp.HelpClubCommon.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioSourceSettings : MonoBehaviour
    {
        public AudioSourceType type;

        private void OnEnable()
        {
            AudioControl.AudioSourceAdded.Invoke(this);
        }

        private void OnDisable()
        {
            if (this != null)
                AudioControl.AudioSourceRemoved?.Invoke(this);
        }
    }
}
