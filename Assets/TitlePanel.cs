using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TitlePanel : GamePanel
{
    private CharacterCard _characterCard;
    private TitleScreenGraphic _titleScreen;
    private Action EndTitleNotification;

    public float _characterCardDuration = 10;
    public float _titleScreenDuration = 10;

    private float _timeLeft;
    private CurrentScreen _currentScreen;
    private ButtonMaster _buttonMaster;

    private AudioSource _audioSource;
    public List<AudioClip> MusicClips;
    public float _musicVolume = 0.5f;

    // Use this for initialization
    void Awake()
    {
        _currentScreen = CurrentScreen.None;
        _characterCard = GetComponentInChildren<CharacterCard>(true);
        _titleScreen = GetComponentInChildren<TitleScreenGraphic>(true);
        _timeLeft = _titleScreenDuration;
        _audioSource = GetComponent<AudioSource>();
    }

    public void StartTitleScreen(ButtonMaster buttonMaster, float musicVolume, Action endTitleNotification)
    {
        _buttonMaster = buttonMaster;
        _musicVolume = musicVolume;

        base.Show();
        EndTitleNotification = endTitleNotification;

        ShowTitleScreen();
        PlayMusic();
    }

    public override void Hide()
    {
        StopMusic();
        base.Hide();
    }

    // Update is called once per frame
    void Update()
    {

        if (_buttonMaster.IsStartKey())
        {
            //They pressed start.
            //Call back to the game brain for the next action.
            EndTitleNotification();
        }
        else
        {
            //Check the timer. Should we move to the next option?
            _timeLeft -= Time.deltaTime;
            if (_timeLeft <= 0)
                ChangeScreen();
        }
    }

    void ChangeScreen()
    {
        switch (_currentScreen)
        {
            case CurrentScreen.TitleScreen:
                ShowCharacterCard();
                break;
            case CurrentScreen.CharacterCard:
            default:
                ShowTitleScreen();
                break;
        }
    }

    void ShowCharacterCard()
    {
        //Hide the title screen, show the character card.
        _titleScreen.Hide();
        _characterCard.Show();

        _currentScreen = CurrentScreen.CharacterCard;
        _timeLeft = _characterCardDuration;
    }

    void ShowTitleScreen()
    {
        //Hide the title screen, show the character card.
        _titleScreen.Show();
        _characterCard.Hide();

        _currentScreen = CurrentScreen.TitleScreen;
        _timeLeft = _titleScreenDuration;
    }

    enum CurrentScreen
    {
        None,
        TitleScreen,
        CharacterCard
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
        if (_audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
    }
}