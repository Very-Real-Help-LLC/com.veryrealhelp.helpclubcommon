using UnityEngine;

namespace VeryRealHelp.HelpClubCommon.Audio
{
    public enum AudioSourceType
    {
        Ambient,
        Music,
        Effects,
        Interface,
        Voice
    }

    public delegate void AudioSourceSettingsDelegate(AudioSourceSettings settings);
    public static class AudioControl
    {
        public static AudioSourceSettingsDelegate AudioSourceAdded;
        public static AudioSourceSettingsDelegate AudioSourceRemoved;
    }
}
