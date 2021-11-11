using UnityEngine;

public class PlayAudioClipBehavior : MonoBehaviour
{
    [Header("Optional")]
    public AudioSource audioSource;

    public void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    public void PlayAudioClip(AudioClip clip) => audioSource.PlayOneShot(clip);
}
