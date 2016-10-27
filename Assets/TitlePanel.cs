using System;
using UnityEngine;
using System.Collections;

public class TitlePanel : GamePanel
{
    private CharacterCard _characterCard;
    private TitleScreenGraphic _titleScreen;
    private Action EndTitleNotification;

    private float _timeLeft;
    public float _characterCardDuration = 10;
    public float _titleScreenDuration = 10;
    private CurrentScreen _currentScreen;

    // Use this for initialization
    void Awake ()
    {
        _currentScreen = CurrentScreen.None;
        _characterCard = GetComponentInChildren<CharacterCard>(true);
        _titleScreen = GetComponentInChildren<TitleScreenGraphic>(true);
        _timeLeft = _titleScreenDuration;
	}

    public void StartTitleScreen(Action endTitleNotification)
    {
        base.Show();
        EndTitleNotification = endTitleNotification;

        ShowTitleScreen();
    }
	
	// Update is called once per frame
	void Update ()
    {
	    if (ButtonMaster.IsStartKey())
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
        switch(_currentScreen)
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
}
