using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class AttackPanel : GamePanel
{
    public bool UseTimer;
    public float TimeBetweenAttackCycles;

    public GameObject Party1Success;
    public GameObject Party2Success;
    public GameObject Party1FailPanel;
    public GameObject Party2FailPanel;

    //public Text SuccessText;
    //public Text FailText;
    //public Text PlayerHealthText;
    //public Text BossHealthText;

    public GameObject BossPanel;
    public GameObject PlayerPanel;
    public GameObject TimerPanel;
    public GameObject PanicModePanel;

    public AudioClip[] RoundVictorySounds;
    public float RoundVictoryVolume = 1.0f;

    public float RoundVictoryTimeSeconds = 2;

    //private bool AutoInputDualActionMode = true;

    private ButtonMaster _buttonMaster;
    private BattleSettings _battleSettings;

    private PartyGroup _party1;
    private PartyGroup _party2;
    private TimerPanel _timerPanel;
    private TheBigButton _theBigButton;
    private ProjectileCannon _cannon;

    private float _party1Health;
    private float _party2Health;
    private float _party1StartHealth;
    private float _party2StartHealth;
    private float _currentActivePeriodSeconds;
    private int _completedCycles;
    private float _timeLeft;
    private float _currentTimeLeftSeconds;

    private ButtonNamePanel _buttonNamePanel;

    private RoundResult _roundResult = null;
    private ResolutionPanel _resolutionPanel;

    //private SuccessSprite _party1SuccessSprite;
    //private SuccessSprite _party2SuccessSprite;
    //private FailSprite _party1FailSprite;
    //private FailSprite _party2FailSprite;

    private bool _battleRaging = false;
    private bool _freezeTime = false;
    //private BattleMode _battleMode = BattleMode.Timed;
    private Action<RoundResult> EndRoundNotification;

    private AudioSource _audioSource;

    void Awake()
    {
        _battleRaging = false;

        _party1 = PlayerPanel.GetComponent<PartyGroup>();
        //_party1SuccessSprite = PlayerPanel.GetComponentInChildren<SuccessSprite>(true);
        //_party1FailSprite = PlayerPanel.GetComponentInChildren<FailSprite>(true);

        _party2 = BossPanel.GetComponent<PartyGroup>();
        //_party2SuccessSprite = BossPanel.GetComponentInChildren<SuccessSprite>(true);
        //_party2FailSprite = BossPanel.GetComponentInChildren<FailSprite>(true);

        _timerPanel = TimerPanel.GetComponent<TimerPanel>();

        _resolutionPanel = GetComponentInChildren<ResolutionPanel>(true);
        _theBigButton = GetComponentInChildren<TheBigButton>(true);
        _buttonNamePanel = GetComponentInChildren<ButtonNamePanel>(true);
        _cannon = GetComponentInChildren<ProjectileCannon>(true);

        _audioSource = GetComponent<AudioSource>();
    }

    public void StartBattle(ButtonMaster buttonMaster, BattleSettings battleSettings)
    {
        _buttonMaster = buttonMaster;
        _battleSettings = battleSettings;


        if (_battleSettings.BossFight)
        {
            _party1StartHealth = _battleSettings.PVEStartHealth;
            _party2StartHealth = _battleSettings.BossStartHealth;
        }
        else
        {
            _party2StartHealth = _battleSettings.PVPStartHealth;
            _party1StartHealth = _battleSettings.PVPStartHealth;
        }

        _completedCycles = 0;
        _party1Health = _party1StartHealth;
        _party2Health = _party2StartHealth;

        //Want to clean up old rendering of things from previous games
        _party1.RefreshHealth(_party1StartHealth, _party1StartHealth);
        _party2.RefreshHealth(_party2StartHealth, _party2StartHealth);

        //Prep the player/boss panels for battle.
        _party1.PrepForBattle(
            _battleSettings.BossFight, 
            _party1StartHealth,
            _battleSettings.VictoriesNeededToWin,
            _battleSettings.Party1CurrentVictories
            );
        _party2.PrepForBattle(
            _battleSettings.BossFight, 
            _party2StartHealth,
            _battleSettings.VictoriesNeededToWin,
            _battleSettings.Party2CurrentVictories
            );

        _battleRaging = true;
        EndRoundNotification = _battleSettings.EndRoundNotification;

        //Start on normal mode, move to Panic Mode later
        PanicModePanel.SetActive(false);

        StartAttackCycle(AttackMode.Normal);
    }

    void Update()
    {
        //Only handle user input when the battle is raging (yar!) and we are not freezing time waiting for something interesting to happen.
        if (_battleRaging && !_freezeTime)
        {
            if (!HandleAttackInput())
            {
                //If we have not received a keypress, then we make sure they are within the time limit.

                //Unless time is frozen, which happens during the resolution phase after a keypress

                _timeLeft -= Time.deltaTime;
                UpdateTimeLeftText();

                if (_timeLeft < 0)
                {
                    FailByTimeElapsed();
                    EndAttackCycle();
                }
            }
        }
    }
    
    #region State - Attack Cycle

    /// <summary>
    /// A single attack cycle is the timed period where the players are prompted to press a button. Upon
    /// pressing a button this cycle ends and a new one is prompted.
    /// </summary>
    /// <param name="attackMode"></param>
    void StartAttackCycle(AttackMode attackMode)
    {
        //Debug.Log("Start attack cycle");

        #region Determine Timer

        //The attack timer starts as a high duration and slowly gets smaller as the game drags on.
        //We want to make the active screen longer in PVP. It's a race against each other, not the clock.
        float fMaximumActiveScreenSeconds;
        if (UseTimer)
        {
            fMaximumActiveScreenSeconds = _battleSettings.InitialActiveScreenSeconds;
        }
        else
        {
            fMaximumActiveScreenSeconds = 9999;
        }

        float fMinimumActiveScreenSeconds;
        if (UseTimer)
        {
            fMinimumActiveScreenSeconds = _battleSettings.MinimumActiveScreenSeconds;
        }
        else
        {
            fMinimumActiveScreenSeconds = 9998;
        }

        //Set the time span based on the current cycle.
        //_currentActivePeriodSeconds = InitialActiveScreenSeconds - (ActiveSecondsDecreasePerCycle * _completedCycles);
        _currentActivePeriodSeconds = ((fMaximumActiveScreenSeconds - fMinimumActiveScreenSeconds) * (1 / Mathf.Pow(_battleSettings.ActiveScreenScalingFactor * _completedCycles + 1, 2)) + fMinimumActiveScreenSeconds) * _battleSettings.BuffIncreaseActiveTimeMultiplier;

        //Time Left is the timer that controls the current cycle.
        //We set current time left and time left to guage the width of the timer bar.
        _currentTimeLeftSeconds = _currentActivePeriodSeconds;
        _timeLeft = _currentTimeLeftSeconds;

        //Unfreeze time so the timer starts counting again next cycle.
        _freezeTime = false;

        #endregion

        #region New Button Selection

        //If this is a boss fight, only pick a button for the first team.
        // If it's a PVP battle, need to pick a button for each.

        //Start with Party 1, they will need a button regardless
        _buttonMaster.SelectNewButtons();
        
        UpdateButtonNameText();

        #endregion

        //Track number of attack cycles to determine the speed of the timer.
        _completedCycles++;
    }

    /// <summary>
    /// Sudden Death gives both foes one last chance to kill each other.
    /// </summary>
    void StartSuddenDeath()
    {
        //For sudden death, each team gets 1 health. First person to fuck up loses.
        _party1Health = 1;
        _party2Health = 1;
        _party1.RefreshHealth(_party1Health, _party1StartHealth);
        _party2.RefreshHealth(_party2Health, _party2StartHealth);

        StartAttackCycle(AttackMode.SuddenDeath);
    }

    void EndRound(BattleVictor victor)
    {
        //The battle is ended. Make sure everything user-related freezes.
        _battleRaging = false;
        _freezeTime = false;

        if (victor == BattleVictor.Party1
            || victor == BattleVictor.PVE)
        {
            _party1.ShowVictoryDance();
        }
        else
        {
            _party2.ShowVictoryDance();
        }


        //How do we share the end of the battle with the parent object?
        RoundResult roundResult = new RoundResult()
        {
            Victor = victor
        };
        ShowRoundVictoryPanel(roundResult);
        //The attack panel will reset after the victory panel.
    }

    void EndAttackCycle()
    {
        _freezeTime = true;
        StartCoroutine(UnthreadedDelay(TimeBetweenAttackCycles, EvaluateAttackOutcome));
    }

    void EvaluateAttackOutcome()
    {
        if (_party1Health <= 0 && _party2Health <= 0)
        {
            //float fail!
            //Go into sudden death.
            StartSuddenDeath();
        }
        else if (_party1Health <= 0)
        {
            //Party 1 Died. Was it a boss fight?
            if (_battleSettings.BossFight)
            {
                EndRound(BattleVictor.Boss);
            }
            else
            {
                EndRound(BattleVictor.Party2);
            }
        }
        else if (_party2Health <= 0)
        {
            //Party 2 Died. Was it a boss fight?
            if (_battleSettings.BossFight)
            {
                EndRound(BattleVictor.PVE);
            }
            else
            {
                EndRound(BattleVictor.Party1);
            }
        }
        else
        {
            //Nobody died. What a pity. Let's just go back and do it again, shall we?
            StartAttackCycle(AttackMode.Normal);
        }
    }

    bool HandleAttackInput()
    {
        if (ReadAttackInput())
        {
            //Somebody pressed something. 

            EndAttackCycle();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Reads keystrokes during the attack cycle, determines outcome of keystrokes, and dispatches actions.
    /// </summary>
    /// <returns>Returns true if the players pressed a key.</returns>
    bool ReadAttackInput()
    {
        //First thing's first: read all the active button presses. Each joystick button will be represented
        // by TWO or MORE unique keypress events. We want to register two of them and cross reference to figure
        // out exactly which key belonging to which player was actually pressed.
        //If any duplicate letters or numbers, it's a tie. If the letters are on opposite teams, then no
        // damage is assigned. If they are on the same team, then damage is assigned.
        // Ties between presses are very unlikely given the fact that hundreds of updates happen per second.

        List<KeyCode> numberKeysPressed = new List<KeyCode>();
        //List<KeyCode> letterKeysPressed = new List<KeyCode>();

        string sGrabEmAll = "";
        foreach (KeyCode numberKey in _buttonMaster.GetAllActiveKeys())
        {
            sGrabEmAll += numberKey + " - ";

            //Check all possible joystick number keys to look for multiple presses.
            if (Input.GetKeyDown(numberKey))
            {
                Debug.Log("Key strike: " + numberKey);

                //Someone pressed a joystick key!
                numberKeysPressed.Add(numberKey);
            }
        }

        if (numberKeysPressed.Count > 0)
        {
            //Somebody pressed at least one joystick key. Oh joy!
            //First check to see if TWO buttons were pressed
            #region OLD
            /*if (letterKeysPressed.Count >= 2)
            {
                #region Two buttons hit in different sections.

                //If it's a boss fight, then this is an automatic failure, since all players are on the same team.
                if (_battleSettings.BossFight)
                {
                    Party1InputFailByTie();
                }
                else
                {
                    //Two different players pressed a key. Were they on the same team?
                    //How many from each team pressed a button?
                    int wParty1Count = 0;
                    int wParty2Count = 0;
                    foreach (KeyCode letterKey in letterKeysPressed)
                    {
                        if (_buttonMaster.IsKeyParty1(number))
                            wParty1Count++;
                        else if (_buttonMaster.IsKeyParty2(letterKey))
                            wParty2Count++;
                    }

                    //Deal damage to whichever team was the bigger assholes.
                    if (wParty1Count == wParty2Count)
                    {
                        //Tie!
                        //No damage dealt to either team.
                        BothPartyInputTie();
                    }
                    else if (wParty2Count > wParty1Count)
                    {
                        Party2InputFailByTie();
                    }
                    else if (wParty1Count > wParty2Count)
                    {
                        Party1InputFailByTie();
                    }
                }
                #endregion
                return true;
            }
            else */
            #endregion

            if (numberKeysPressed.Count >= 2)
            {
                Debug.Log("Duplicate press");
                #region Two buttons hit at the same time

                //If it's a boss fight, then this is an automatic failure, since all players are on the same team.
                if (_battleSettings.BossFight)
                {
                    Party1InputFailByTie();
                }
                else
                {
                    bool bParty1Success = false;
                    bool bParty1Failed = false;
                    bool bParty2Success = false;
                    bool bParty2Failed = false;

                    foreach (KeyCode key in numberKeysPressed)
                    {
                        if (_buttonMaster.IsKeyParty1(key))
                        {
                            Party1InputFailByTie();
                            break;
                        }
                    }

                    foreach (KeyCode key in numberKeysPressed)
                    {
                        if (_buttonMaster.IsKeyParty2(key))
                        {
                            if (_buttonMaster.IsCurrentButtonParty2(key))
                            {
                                bParty2Success = true;
                            }
                            else
                            {
                                bParty2Failed = true;
                            }
                            break;
                        }
                    }


                    if (bParty2Failed && bParty1Failed)
                    {
                        FailByTimeElapsed();
                    }
                    else if (bParty1Failed || bParty2Failed)
                    {
                        if (bParty1Failed)
                        {
                            if (bParty2Success)
                            {
                                Party2InputSuccess();
                            }
                            else
                            {
                                Party1InputFailByButtonPress();
                            }
                        }
                        else
                        {
                            if (bParty1Success)
                            {
                                Party1InputSuccess();
                            }
                            else
                            {
                                Party2InputFailByButtonPress();
                            }
                        }
                    }
                    else if (bParty2Success && bParty1Success)
                    {
                        BothPartyInputTie();
                    }
                    else if (bParty2Success)
                    {
                        Party2InputSuccess();
                    }
                    else if (bParty1Success)
                    {
                        Party1InputSuccess();
                    }

                }
                #endregion
                return true;
            }
            else
            {
                #region Single button pressed. Was it correct?

                if (_battleSettings.BossFight)
                {
                    if (_buttonMaster.IsCurrentButtonParty1(numberKeysPressed[0]))
                    {
                        //Correct key pressed!
                        Party1InputSuccess();
                    }
                    else
                    {
                        Party1InputFailByButtonPress();
                    }
                }
                else
                {
                    KeyCode numberKey = numberKeysPressed[0];

                    Debug.Log("Single press " + numberKey);
                    if (_buttonMaster.IsKeyParty1(numberKey))
                    {
                        //Pressed by party 1. Was it correct?
                        if (_buttonMaster.IsCurrentButtonParty1(numberKey))
                        {
                            //Success!
                            Debug.Log("Single press succ 1 " + numberKey);
                            Party1InputSuccess();
                        }
                        else
                        {
                            //They pressed the wrong button
                            Debug.Log("Single press fail 1 " + numberKey);
                            Party1InputFailByButtonPress();
                        }
                    }
                    else if (_buttonMaster.IsKeyParty2(numberKey))
                    {
                        Debug.Log("Single press party 2 " + numberKey);
                        //Pressed by party 2. Was it correct?
                        if (_buttonMaster.IsCurrentButtonParty2(numberKey))
                        {
                            //Success!
                            Party2InputSuccess();
                        }
                        else
                        {
                            //They pressed the wrong button
                            Party2InputFailByButtonPress();
                        }
                    }
                    else
                    {
                        //Somebody pressed a key, but it wasn't either of the parties.
                        //Ignore it. Pretend nothing happened.
                    }
                }
                #endregion
                return true;
            }
        }

        //No key was pressed, so return false;
        return false;
    }

    #region Respond to Player Input

    void FailByTimeElapsed()
    {
        if (_battleSettings.BossFight)
        {
            //If it's a boss fight, only punish team one... because they are the only team!
            ApplyDamageToParty1(true);
            ShowParty1Fail();
        }
        else
        {
            //What?! They BOTH failed!
            ApplyDamageToParty1(true);
            ApplyDamageToParty2(true);
            ShowParty1Fail();
            ShowParty2Fail();
        }
    }

    void Party1InputFailByButtonPress()
    {
        ApplyDamageToParty1(true);
        ShowParty1Fail();
    }

    void Party1InputFailByTie()
    {
        //TODO: Support for TIE screen.
        ApplyDamageToParty1(true);
        ShowParty1Fail();
    }

    void Party2InputFailByButtonPress()
    {
        ApplyDamageToParty2(true);
        ShowParty2Fail();
    }

    void Party2InputFailByTie()
    {
        ApplyDamageToParty2(true);
        ShowParty2Fail();
    }

    //If both players pressed at the same time... wow! It's a tie! Do something?
    void BothPartyInputTie()
    {
        //TODO: What do we do?
        //Fuck it, for now assume party 1 wins
        ShowParty1Fail();
        ShowParty2Fail();
    }

    void Party1InputSuccess()
    {
        //Party 1 pressed their button. Apply damage to party 2/boss
        ApplyDamageToParty2(false);
        ShowParty1Success();
    }

    //They pressed the correct button. That means success.
    void Party2InputSuccess()
    {
        ApplyDamageToParty1(false);
        ShowParty2Success();
    }

    void ApplyDamageToParty1(bool bSelfDamage)
    {
        if (_battleSettings.BossFight)
        {
            _party1Health = ApplyDamage(
                _party1Health,
                _party1StartHealth,
                _battleSettings.BossMinimumDamagePerAttack,
                _battleSettings.BossMaximumDamagePerAttack,
                0.0f,
                _party2,
                _party1,
                bSelfDamage
                );
        }
        else
        {
            _party1Health = ApplyDamage(
                _party1Health,
                _party1StartHealth,
                _battleSettings.PlayerMinimumDamagePerAttack,
                _battleSettings.PlayerMaximumDamagePerAttack,
                _battleSettings.BuffParty2CritChance,
                _party2,
                _party1,
                bSelfDamage
                );
        }
    }

    void ApplyDamageToParty2(bool bSelfDamage)
    {
        _party2Health = ApplyDamage(
            _party2Health,
            _party2StartHealth,
            _battleSettings.PlayerMinimumDamagePerAttack,
            _battleSettings.PlayerMaximumDamagePerAttack,
            _battleSettings.BuffParty1CritChance,
            _party1,
            _party2,
            bSelfDamage
            );
    }

    void PressButtonNotification()
    {
        //At the end of the jump movement we want the button to depress.
        _theBigButton.PressMeBaby();

        //Upon pressing the button, a weapon should shoot down from the sky.
        _attackStats.partyDefend.ChooseNewVictim();
        _cannon.Fire(
            _attackStats.partyDefend.GetVictimPosition(),
            ProjectileHitNotification
            );
    }

    void ProjectileHitNotification()
    {
        _attackStats.partyDefend.TakeDamage(
            _attackStats.damage,
            _attackStats.newHealth,
            _attackStats.startHealth,
            _attackStats.bCrit,
            _attackStats.bSelfDamage
            );
    }

    private AttackStats _attackStats;

    private float ApplyDamage(float currentHealth, float startHealth, float minimumDamage, float maximumDamage, float critPercent, PartyGroup partyAttack, PartyGroup partyDefend, bool bSelfDamage)
    {
        //Get a random damage value from within the specific player attack range
        float damage = Mathf.FloorToInt(UnityEngine.Random.Range(
            minimumDamage,
            maximumDamage
            ));

        //Determine if it's a crit.
        bool bCrit = false;
        //Can't crit yourself. That's just mean.
        if (!bSelfDamage)
        {
            bCrit = UnityEngine.Random.Range(0.0f, 1.0f) < critPercent;
            if (bCrit)
                damage *= 2.0f;
        }

        float newHealth = currentHealth;
        if (damage >= currentHealth)
        {
            damage = currentHealth;
            newHealth = 0;
        }
        else
        {
            newHealth -= damage;
        }

        _attackStats = new AttackStats()
        {
            currentHealth = currentHealth,
            damage = damage,
            newHealth = newHealth,
            startHealth = startHealth,
            bCrit = bCrit,
            bSelfDamage = bSelfDamage,
            partyAttack = partyAttack,
            partyDefend = partyDefend
        };

        if (!bSelfDamage)
        {
            //Do a big fancy attack animation
            partyAttack.MakeAttack(_attackStats, PressButtonNotification);
            /*partyAttack.MakeAttack(bCrit, () =>
            {
                partyDefend.TakeDamage(
                    damage,
                    newHealth,
                    startHealth,
                    bCrit,
                    bSelfDamage
                    );
            }, PressButtonNotification);*/
        }
        else
        {
            //Damage thyself without a big attack animationa
            partyDefend.TakeDamage(
                damage,
                newHealth,
                startHealth,
                bCrit,
                bSelfDamage
                );
        }
        
        return newHealth;
    }

    #endregion

    #endregion

    #region Rendering and Utility

    void ShowParty1Success()
    {
        //_party1SuccessSprite.Show();
    }

    void ShowParty2Success()
    {
       // _party2SuccessSprite.Show();
    }

    void ShowParty1Fail()
    {
        //_party1FailSprite.Show();
    }

    void ShowParty2Fail()
    {
        //_party2FailSprite.Show();
    }

    void UpdateTimeLeftText()
    {
        if (UseTimer)
        {
            _timerPanel.SetTime(_timeLeft, _currentTimeLeftSeconds);
        }
    }

    void UpdateButtonNameText()
    {
        //Always show player 1 button commands

        //ButtonNameText.text = _buttonMaster.GetCurrentParty1ActiveButton().Name + " - " + _buttonMaster.GetCurrentParty1ActiveButton().NumberKey.ToString();
        //ButtonNameText.text = _buttonMaster.GetCurrentParty1ActiveButton().Name;
        _buttonNamePanel.Show();
        _buttonNamePanel.SetText(
            _buttonMaster.GetCurrentParty1ActiveButton().Name + " - " + _buttonMaster.GetCurrentParty1ActiveButton().NumberKey.ToString()
            );
    }

    IEnumerator UnthreadedDelay(float fSeconds, Action thingToExecute)
    {
        yield return new WaitForSeconds(fSeconds);
        thingToExecute();
    }

    void ShowText(bool bShow, Text text)
    {
        CanvasGroup canvasGroup = text.rectTransform.GetComponent<CanvasGroup>();
        if (bShow)
        {
            if (canvasGroup.interactable != true)
                canvasGroup.interactable = true;
            if (canvasGroup.alpha != 1.0f)
                canvasGroup.alpha = 1.0f;
        }
        else
        {
            if (canvasGroup.interactable != false)
                canvasGroup.interactable = false;
            if (canvasGroup.alpha != 0.0f)
                canvasGroup.alpha = 0.0f;
        }
    }

    #endregion
    
    #region State - Round Victory

    void ShowRoundVictoryPanel(RoundResult roundResult)
    {
        _roundResult = roundResult;
        _buttonNamePanel.Hide();

        PlayVictorySound();

        _resolutionPanel.SetVictor(roundResult);
        _resolutionPanel.Show();
        StartCoroutine(UnthreadedDelay(
            RoundVictoryTimeSeconds,
            EndRoundVictoryScreen
            ));
    }

    void EndRoundVictoryScreen()
    {
        _resolutionPanel.Hide();
        EndRoundNotification(_roundResult);
    }

    #endregion

    #region Noises

    void PlayVictorySound()
    {
        if (RoundVictorySounds != null && RoundVictorySounds.Length > 0)
        {
            _audioSource.PlayOneShot(
            RoundVictorySounds[UnityEngine.Random.Range(0, RoundVictorySounds.Length - 1)],
            RoundVictoryVolume
            );
        }
    }

    #endregion
}


public enum AttackSource
{
    Party1Success,
    Party1Failed,
    Party2Success,
    Party2Failed,
    Boss
}

/// <summary>
/// Attack Mode changes the behavior of the attack cycle, including special animations and other visuals.
/// Functionality does not change.
/// </summary>
public enum AttackMode
{
    Normal,
    SuddenDeath
}

public enum BattleVictor
{
    Boss,
    PVE,
    Party1,
    Party2
}

public enum BattleMode
{
    Timed,
    Panic
}
public struct AttackStats
{
    public float currentHealth;
    public float startHealth;
    public float minimumDamage;
    public float maximumDamage;
    public float damage;
    public float critPercent;
    public bool bCrit;
    public float newHealth;
    public PartyGroup partyAttack;
    public PartyGroup partyDefend;
    public bool bSelfDamage;
}
