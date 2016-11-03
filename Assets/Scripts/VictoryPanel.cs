using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VictoryPanel : GamePanel
{
    private AudioSource _audioSource;
    public List<AudioClip> MusicClips;
    private float _musicVolume = 0.5f;

    public GameObject LeftPlayerObject;
    public GameObject RightPlayerObject;

    // Use this for initialization
    void Awake()
    {
        _audioSource = gameObject.GetComponent<AudioSource>();
    }

    public void Show(float dMusicVolume)
    {
        _musicVolume = dMusicVolume;

        this.Show();
    }

    public override void Show()
    {
        base.Show();

        PlayMusic();
        CommenceTheJiggling();
    }

    public override void Hide()
    {
        StopMusic();
        base.Hide();
    }

    public void CommenceTheJiggling()
    {
        Animator leftAnimator = LeftPlayerObject.GetComponent<Animator>();
        if (leftAnimator != null)
            leftAnimator.SetTrigger("Win");

        Animator rightAnimator = RightPlayerObject.GetComponent<Animator>();
        if (rightAnimator != null)
            rightAnimator.SetTrigger("Win");
    }

    void PlayMusic()
    {
        if (MusicClips != null && MusicClips.Count > 0)
        {
            _audioSource.loop = true;
            _audioSource.volume = _musicVolume;
            _audioSource.clip = MusicClips[UnityEngine.Random.Range(0, MusicClips.Count - 1)];
            _audioSource.Play();
        }
    }

    void StopMusic()
    {
        if (_audioSource != null
            && _audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
    }
}
