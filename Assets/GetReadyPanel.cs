using UnityEngine;
using System.Collections;

public class GetReadyPanel : GamePanel
{
    public AudioClip CoolClipBro;
    private AudioSource _audioSource;
    public float ClipVolume = 1.0f;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public override void Show()
    {
        base.Show();
        PlayMusic();
    }

    void PlayMusic()
    {
        if (_audioSource != null && CoolClipBro != null)
        {
            _audioSource.PlayOneShot(CoolClipBro, ClipVolume);
        }
    }
}
