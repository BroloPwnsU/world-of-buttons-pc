using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class AttackPanel : GamePanel
{
    public GameObject Party1Success;
    public GameObject Party2Success;
    public GameObject Party1FailPanel;
    public GameObject Party2FailPanel;

    public Text Party1ButtonNameText;
    public Text Party2ButtonNameText;
    //public Text SuccessText;
    //public Text FailText;
    //public Text PlayerHealthText;
    //public Text BossHealthText;

    public GameObject BossPanel;
    public GameObject PlayerPanel;
    public GameObject TimerPanel;

    private bool AutoInputDualActionMode = true;

    private ButtonMaster _buttonMaster;
    private BattleSettings _battleSettings;

    private PartyGroup _party1;
    private PartyGroup _party2;
    private TimerPanel _timerPanel;

    private float _party1Health;
    private float _party2Health;
    private float _party1StartHealth;
    private float _party2StartHealth;
    private float _currentActivePeriodSeconds;
    private int _completedCycles;
    private float _timeLeft;
    private float _currentTimeLeftSeconds;

    private SuccessSprite _party1SuccessSprite;
    private SuccessSprite _party2SuccessSprite;
    private FailSprite _party1FailSprite;
    private FailSprite _party2FailSprite;

    private bool _battleRaging = false;
    private bool _freezeTime = false;

    void Start()
    {
        _battleRaging = false;

        _party1 = PlayerPanel.GetComponent<PartyGroup>();
        _party1SuccessSprite = PlayerPanel.GetComponentInChildren<SuccessSprite>();
        _party1FailSprite = PlayerPanel.GetComponentInChildren<FailSprite>();

        _party2 = BossPanel.GetComponent<PartyGroup>();
        _party2SuccessSprite = BossPanel.GetComponentInChildren<SuccessSprite>();
        _party2FailSprite = BossPanel.GetComponentInChildren<FailSprite>();

        _timerPanel = TimerPanel.GetComponent<TimerPanel>();
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
        _party1.PrepForBattle(_battleSettings.BossFight, _party1StartHealth);
        _party2.PrepForBattle(_battleSettings.BossFight, _party2StartHealth);

        _battleRaging = true;

        StartAttackCycle(AttackMode.Normal);
    }

    void Update()
    {
        if (_battleRaging)
        {
            if (ReadAttackInput())
            {
                EndAttackCycle();
            }
            else
            {
                //If we have not received a keypress, then we make sure they are within the time limit.

                //Unless time is frozen, which happens during the resolution phase after a keypress
                if (!_freezeTime)
                {
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
        if (_battleSettings.BossFight)
            fMaximumActiveScreenSeconds = _battleSettings.InitialActiveScreenSeconds;
        else
            fMaximumActiveScreenSeconds = _battleSettings.InitialActiveScreenSeconds * 5;

        float fMinimumActiveScreenSeconds;
        if (_battleSettings.BossFight)
            fMinimumActiveScreenSeconds = _battleSettings.MinimumActiveScreenSeconds;
        else
            fMinimumActiveScreenSeconds = _battleSettings.MinimumActiveScreenSeconds * 5;

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
        _buttonMaster.SelectNewButtonForParty1();

        //Let's check if the game is PVP, then select a button for party 2 (if necessary)
        if (!_battleSettings.BossFight)
        {
            _buttonMaster.SelectNewButtonForParty2();
        }

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

    void EndBattle(BattleVictor victor)
    {
        _battleRaging = false;
    }

    void EndAttackCycle()
    {
        _freezeTime = true;
        StartCoroutine(UnthreadedDelay(1.1f, EvaluateAttackOutcome));
    }

    void EvaluateAttackOutcome()
    {
        if (_party1Health <= 0 && _party2Health <= 0)
        {
            //Double fail!
            //Go into sudden death.
            StartSuddenDeath();
        }
        else if (_party1Health <= 0)
        {
            //Party 1 Died. Was it a boss fight?
            if (_battleSettings.BossFight)
            {
                Debug.Log("Battle Over. Boss won!");
                EndBattle(BattleVictor.Boss);
            }
            else
            {
                Debug.Log("Battle Over. Party 2 won!");
                EndBattle(BattleVictor.Party2);
            }
        }
        else if (_party2Health <= 0)
        {
            //Party 2 Died. Was it a boss fight?
            if (_battleSettings.BossFight)
            {
                Debug.Log("Battle Over. Humans won!");
                EndBattle(BattleVictor.PVE);
            }
            else
            {
                Debug.Log("Battle Over. Party 1 won!");
                EndBattle(BattleVictor.Party1);
            }
        }
        else
        {
            //Nobody died. What a pity. Let's just go back and do it again, shall we?
            StartAttackCycle(AttackMode.Normal);
        }
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
        List<KeyCode> letterKeysPressed = new List<KeyCode>();

        foreach (KeyCode numberKey in _buttonMaster.GetAllNumberKeys())
        {
            //Check all possible joystick number keys to look for multiple presses.
            if (Input.GetKeyDown(numberKey))
            {
                //Someone pressed a joystick key!
                numberKeysPressed.Add(numberKey);

                //If the AutoInput mode is active then we self-populate the letter keys
                if (AutoInputDualActionMode)
                {
                    KeyCode letterKey = _buttonMaster.GetLetterKeyFromNumberKey(numberKey);
                    if (!letterKeysPressed.Contains(letterKey))
                        letterKeysPressed.Add(letterKey);
                }
            }
        }

        foreach (KeyCode letterKey in _buttonMaster.GetAllLetterKeys())
        {
            //Check all possible letter keys to look for multiple presses.
            if (Input.GetKeyDown(letterKey))
            {
                //Someone pressed a joystick key!
                letterKeysPressed.Add(letterKey);
            }
        }

        if (letterKeysPressed.Count > 0 && numberKeysPressed.Count > 0)
        {
            //Somebody pressed at least one joystick key. Oh joy!
            //First check to see if TWO buttons were pressed
            if (letterKeysPressed.Count >= 2)
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
                        if (_buttonMaster.IsLetterKeyParty1(letterKey))
                            wParty1Count++;
                        else if (_buttonMaster.IsLetterKeyParty2(letterKey))
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
            else if (numberKeysPressed.Count >= 2)
            {
                #region Two buttons hit in the same section

                //If it's a boss fight, then this is an automatic failure, since all players are on the same team.
                if (_battleSettings.BossFight)
                {
                    Party1InputFailByTie();
                }
                else
                {
                    KeyCode letterKey = letterKeysPressed[0];

                    //One player pressed more than one key. Which team were they on?
                    if (_buttonMaster.IsLetterKeyParty1(letterKey))
                    {
                        Party1InputFailByTie();
                    }
                    else if (_buttonMaster.IsLetterKeyParty1(letterKey))
                    {
                        Party2InputFailByTie();
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
                    Debug.Log("something pressed?");
                    if (_buttonMaster.IsCurrentButtonParty1(letterKeysPressed[0], numberKeysPressed[0]))
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
                    KeyCode letterKey = letterKeysPressed[0];
                    KeyCode numberKey = numberKeysPressed[0];


                    if (_buttonMaster.IsLetterKeyParty1(letterKey))
                    {
                        //Pressed by party 1. Was it correct?
                        if (_buttonMaster.IsCurrentButtonParty1(letterKey, numberKey))
                        {
                            //Success!
                            Party1InputSuccess();
                        }
                        else
                        {
                            //They pressed the wrong button
                            Party1InputFailByButtonPress();
                        }
                    }
                    else if (_buttonMaster.IsLetterKeyParty2(letterKey))
                    {
                        //Pressed by party 2. Was it correct?
                        if (_buttonMaster.IsCurrentButtonParty2(letterKey, numberKey))
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

    private float ApplyDamage(float currentHealth, float startHealth, float minimumDamage, float maximumDamage, float critPercent, PartyGroup partyAttack, PartyGroup partyDefend, bool bSelfDamage)
    {
        //Get a random damage value from within the specific player attack range
        float damage = Mathf.FloorToInt(UnityEngine.Random.Range(
            minimumDamage,
            maximumDamage
            ));

        //Determine if it's a crit.
        bool bCrit = UnityEngine.Random.Range(0.0f, 1.0f) < critPercent;
        if (bCrit)
            damage *= 2.0f;

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

        if (!bSelfDamage)
            partyAttack.MakeAttack(bCrit);

        partyDefend.TakeDamage(
            damage,
            newHealth,
            startHealth,
            bCrit,
            bSelfDamage
            );

        return newHealth;
    }

    #endregion

    #endregion

    #region Rendering and Utility

    void ShowParty1Success()
    {
        _party1SuccessSprite.Show();
    }

    void ShowParty2Success()
    {
        _party2SuccessSprite.Show();
    }

    void ShowParty1Fail()
    {
        _party1FailSprite.Show();
    }

    void ShowParty2Fail()
    {
        _party2FailSprite.Show();
    }

    void UpdateTimeLeftText()
    {
        _timerPanel.SetTime(_timeLeft, _currentTimeLeftSeconds);
    }

    void UpdateButtonNameText()
    {
        //Always show player 1 button commands
        Party1ButtonNameText.text = _buttonMaster.GetCurrentParty1ActiveButton().Name + " - " + _buttonMaster.GetCurrentParty1ActiveButton().NumberKey.ToString();

        if (_battleSettings.BossFight)
        {
            ShowText(false, Party2ButtonNameText);
        }
        else
        {
            //If PVP, Show both button commands
            ShowText(true, Party2ButtonNameText);
            Party2ButtonNameText.text = _buttonMaster.GetCurrentParty2ActiveButton().Name + " - " + _buttonMaster.GetCurrentParty2ActiveButton().NumberKey.ToString();
        }
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