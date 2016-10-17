using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameBrain : MonoBehaviour
{
    #region Private Members

    private float _timeLeft;
    private float _currentTimeLeftSeconds;
    private float _currentActivePeriodSeconds;
    private int _completedCycles;
    
    private bool AutoInputDualActionMode = true;
    private ButtonMaster _buttonMaster = new ButtonMaster();

    private GameState _gameState;
    private bool _bossFight = true;
    private float _party1Health;
    private float _party1StartHealth;
    private float _party2Health;
    private float _party2StartHealth;
    private float _buffParty1CritChance = 0;
    private float _buffParty2CritChance = 0;
    private float _buffIncreaseActiveTimeMultiplier = 1;
    private float _buffIncreaseActiveTimePercent = 0;
    private float _buffDecreaseBossHealthPercent = 0;
    private float _buffIncreasePlayerHealthPercent = 0;

    private List<IGamePanel> _gamePanels = new List<IGamePanel>();

    #endregion

    #region Public Properties

    public bool DebugMode = true;

    public GameObject MenuSelectorPrefab;

    public Text Party1ButtonNameText;
    public Text Party2ButtonNameText;
    public Text SuccessText;
    public Text FailText;
    public Text TitleText;
    public Text BuffText;
    public Text GetReadyText;
    public Text PlayerHealthText;
    public Text BossHealthText;
    public Text YouLoseText;
    public Text YouWinText;

    public Text GameStateText;

    public GameObject OptionsPanel;
    public GameObject BossPanel;
    public GameObject PlayerPanel;
    public GameObject TimerPanel;
    public GameObject Party1VictoryPanel;
    public GameObject Party2VictoryPanel;

    public float InitialActiveScreenSeconds;
    public float MinimumActiveScreenSeconds;
    public float ActiveScreenScalingFactor;
    public float PrepScreenSeconds;
    public float FailScreenSeconds;
    public float SuccessScreenSeconds;

    public float BossStartHealth;
    public float BossMaximumDamagePerAttack;
    public float BossMinimumDamagePerAttack;
    public float PlayerStartHealth;
    public float PlayerMinimumDamagePerAttack;
    public float PlayerMaximumDamagePerAttack;
    public int PlayerAttacksPerBossAttack;
    public float BuffPlayerTimeIncreasePercentPerTier;
    public float BuffBossHealthDecreasePercentPerTier;
    public float BuffPlayerHealthIncreasePercentPerTier;
    public float BuffCritChancePerTier;
    public float BuffNonsense;

    private PartyGroup _party1;
    private PartyGroup _party2;
    private TimerPanel _timerPanel;
    private OptionsPanel _buffPanel;

    #endregion

    #region Unity Built-In Functions

    // Use this for initialization
    void Start()
    {
        //_gameButtonList = GetGameButtonList();
        _party1 = PlayerPanel.GetComponent<PartyGroup>();
        _party2 = BossPanel.GetComponent<PartyGroup>();
        _timerPanel = TimerPanel.GetComponent<TimerPanel>();
        _buffPanel = OptionsPanel.GetComponent<OptionsPanel>();

        RevertToTitleScreen();
    }

    /// <summary>
    /// Update will drive the game. It reads input then sends out instructions to change visuals and game state.
    /// Need to alter this and all subsequent functions to directly affect the visibility and activation of panels.
    /// </summary>
    void Update()
    {
        //Debugging.
        if (DebugMode)
        {
            LogInputKeyPress();
        }

        //If at any time they hit the escape key it goes back to the title screen.
        // This doesn't care about current game state.
        CheckResetButton();
        
        //All other key commands are dependent on the game state.
        if (_gameState == GameState.TitleScreen)
        {
            #region Title Screen. Must press Enter to start game.

            //From the title screen, only the attendant can actually start the game.
            //Attendant must press Enter to start.
            if (_buttonMaster.IsStartKey())
            {
                StartGame();
            }

            #endregion
        }
        else if (_gameState == GameState.OptionsScreen)
        {
            #region Options - Admin choses game mode

            //Players are not allowed to do anything during this period.
            //Since they don't have anything to do, we will just wait for the enter key to be pressed.
            if (_buttonMaster.IsStartKey())
            {
                EndOptionsScreen();
            }

            #endregion
        }
        else if (_gameState == GameState.PrepScreen)
        {
            #region Prep - Intro animation/instructions. Can't skip.

            // Only attendant can reset the game from here.
            //We will wait for the time period to end.
            _timeLeft -= Time.deltaTime;
            if (_timeLeft < 0)
            {
                //After the time expires, switch to a player cycle.
                EndPrepScreen();
            }

            #endregion
        }
        else if (_gameState == GameState.AttackScreen)
        {
            if (!ReadAttackInput())
            {
                //If we have not received a keypress, then we make sure they are within the time limit.
                _timeLeft -= Time.deltaTime;
                if (_timeLeft < 0)
                {
                    FailByTimeElapsed();
                }
            }
        }
        else if (_gameState == GameState.SuccessScreen)
        {
            #region Success - Player pressed a correct button.

            //They got the right button. Suspend input for a few seconds while an animation plays.
            _timeLeft -= Time.deltaTime;
            if (_timeLeft < 0)
            {
                EndResolutionScreen();
            }

            #endregion
        }
        else if (_gameState == GameState.FailScreen)
        {
            #region Fail - Player failed to press the right button.

            //They failed. They are bad and should feel bad. Suspend input for a few seconds while an animation plays.
            _timeLeft -= Time.deltaTime;
            if (_timeLeft < 0)
            {
                EndAttackCycle();
            }

            #endregion
        }
        else if (_gameState == GameState.VictoryScreen)
        {
            #region Showing the victory screen. Press F1 or Esc to go back to title screen.

            //Want to show some stats.
            if (Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.Escape))
            {
                RevertToTitleScreen();
            }

            #endregion
        }
        else if (_gameState == GameState.DefeatScreen)
        {
            #region Showing the defeat screen. Press F1 or Esc to go back to title screen.

            //Want to show some stats.
            if (Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.Escape))
            {
                RevertToTitleScreen();
            }

            #endregion
        }
        else
        {
            //If no game state is set, reset the app. It will take us back to the title screen.
            RevertToTitleScreen();
        }

        //We've altered the game state based on user input. Now render the game based on the new state.
        RenderGameState();
    }

    #endregion

    #region Startup

    void ResetGameValues()
    {
        _bossFight = true;
        _party1Health = 0.0f;
        _party2Health = 0.0f;
        _completedCycles = 0;
        _timeLeft = 0.0f;
        _currentTimeLeftSeconds = 0.0f;

        _buttonMaster.SetupButtons(_bossFight);
    }

    #endregion

    ///State Management

    #region State Agnostic

    /// <summary>
    /// Checks for the reset button and reverts to the title screen, regardless of game state.
    /// </summary>
    void CheckResetButton()
    {
        if (_buttonMaster.IsResetKey())
        {
            RevertToTitleScreen();
            RenderGameState();
            return;
        }
    }

    #endregion

    #region State - Title Screen
    
    /// <summary>
    /// Like pressing Reset on the game. Goes back to title screen and clears out the current game.
    /// </summary>
    void RevertToTitleScreen()
    {
        _gameState = GameState.TitleScreen;
        ResetGameValues();
        RenderTitleScreen();
    }

    /// <summary>
    /// Show the title, hide everything else.
    /// </summary>
    void RenderTitleScreen()
    {
        //Iterate through the panels to hide them, unless they are on the acceptable list.
        foreach (IGamePanel gamePanel in _gamePanels)
        {
            if (gamePanel is TitlePanel)
            {
                gamePanel.Show();
            }
            else
            {
                gamePanel.Hide();
            }
        }
    }

    /// <summary>
    /// Leave the title screen and go to options.
    /// </summary>
    void StartGame()
    {
        StartOptionsScreen();
    }

    #endregion

    #region State - Game Options

    void StartOptionsScreen()
    {
        _gameState = GameState.OptionsScreen;
        RenderGameOptionsScreen();

        _buffPanel.StartOptions(GetOptionsSettings());

        //Assign some random values for now
        _bossFight = true;
        _buffIncreaseActiveTimePercent = UnityEngine.Random.Range(0, 5) * (0.01f * BuffCritChancePerTier);
        _buffIncreaseActiveTimeMultiplier = 1 + _buffIncreaseActiveTimePercent;
        _buffDecreaseBossHealthPercent = UnityEngine.Random.Range(0, 5) * (0.01f * BuffBossHealthDecreasePercentPerTier);
        _buffIncreasePlayerHealthPercent = UnityEngine.Random.Range(0, 5) * (0.01f * BuffPlayerHealthIncreasePercentPerTier);
        _buffParty1CritChance = UnityEngine.Random.Range(0, 5) * (0.01f * BuffCritChancePerTier);
        _buffParty2CritChance = UnityEngine.Random.Range(0, 5) * (0.01f * BuffCritChancePerTier);
    }

    void RenderGameOptionsScreen()
    {
        foreach (IGamePanel gamePanel in _gamePanels)
        {
            if (gamePanel is OptionsPanel)
            {
                gamePanel.Show();
            }
            else
            {
                gamePanel.Hide();
            }
        }
    }

    private string OPTION_MENU_MODE = "MODE";
    private string OPTION_MENU_MODE_VALUE_BOSS = "BOSS BATTLE";
    private string OPTION_MENU_MODE_VALUE_PVP = "PVP";
    private string OPTION_MENU_CRIT_CHANCE = "CRIT CHANCE";

    OptionPanelSettings GetOptionsSettings()
    {
        List<OptionMenu> bmList = new List<OptionMenu>();

        List<string> modeValues = new List<string>()
        {
            OPTION_MENU_MODE_VALUE_BOSS,
            OPTION_MENU_MODE_VALUE_PVP
        };
        OptionMenu bmMode = new OptionMenu(
            OPTION_MENU_MODE,
            modeValues.ToArray(),
            modeValues[0]
            );

        bmList.Add(bmMode);

        List<string> critValues = new List<string>()
        {
            "5%",
            "10%",
            "15%",
            "20%",
            "25%"
        };
        OptionMenu omCrit = new OptionMenu(
            OPTION_MENU_CRIT_CHANCE,
            critValues.ToArray(),
            critValues[0]
            );

        bmList.Add(omCrit);

        OptionPanelSettings ops = new OptionPanelSettings(bmList);

        return ops;
    }

    void EndOptionsScreen()
    {
        StartCoroutine(UnthreadedDelay(1.1f, ApplyGameOptions));
    }

    /// <summary>
    /// Reads in selected options from the options panel.
    /// </summary>
    void ApplyGameOptions()
    {
        //First just figure out if it's a boss fight or not.
        Dictionary<string, string> selectedOptions = _buffPanel.GetSelectedOptions();
        foreach (string sKey in selectedOptions.Keys)
        {
            Debug.Log("Option: " + sKey + " - " + selectedOptions[sKey]);
        }

        if (selectedOptions[OPTION_MENU_MODE] == OPTION_MENU_MODE_VALUE_BOSS)
        {
            _bossFight = true;
        }
        else
        {
            _bossFight = false;
        }

        //Apply health buffs/debuffs to starting health meters

        //If it's a PVP fight then player 1 uses the boss's stats. Otherwise it copies player 1.
        //We do this so the PVP fight lasts a good long while.

        if (!_bossFight)
        {
            //Player 2 always has the boss's health level.
            //Don't apply debuffs/buffs during PVP.
            _party2StartHealth = BossStartHealth;
            _party1StartHealth = BossStartHealth;
        }
        else
        {
            //It's PVE... apply the debuffs to boss health.
            _party2StartHealth = BossStartHealth * (1 - _buffDecreaseBossHealthPercent);
            _party1StartHealth = PlayerStartHealth * (1 + _buffIncreasePlayerHealthPercent);
        }

        _party2Health = _party2StartHealth;
        _party1Health = _party1StartHealth;
        
        //Want to clean up old rendering of things from previous games
        _party1.RefreshHealth(_party1StartHealth, _party1StartHealth);
        _party2.RefreshHealth(_party2StartHealth, _party2StartHealth);

        //Re-initialize the buttons for the current fight.
        _buttonMaster.SetupButtons(_bossFight);

        //Prep the player/boss panels for battle.
        _party1.PrepForBattle(_bossFight, _party1StartHealth);
        _party2.PrepForBattle(_bossFight, _party2StartHealth);

        //Skip to prep screen
        BeginPrepScreen();
    }

    #endregion

    #region State - Prep / Get Ready

    /// <summary>
    /// Prep screen is the countdown before the game starts, after the options and loading screens, before the active screen.
    /// </summary>
    void BeginPrepScreen()
    {
        _gameState = GameState.PrepScreen;
        RenderPrepScreen();

        _timeLeft = PrepScreenSeconds;
        _currentTimeLeftSeconds = _timeLeft;
    }

    void RenderPrepScreen()
    {
        foreach (IGamePanel gamePanel in _gamePanels)
        {
            if (gamePanel is GetReadyPanel)
            {
                gamePanel.Show();
            }
            else
            {
                gamePanel.Hide();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void EndPrepScreen()
    {
        StartAttackCycle(AttackMode.Normal);
    }

    #endregion

    #region State - Attack Cycle

    enum AttackSource
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
    enum AttackMode
    {
        Normal,
        SuddenDeath
    }

    /// <summary>
    /// A single attack cycle is the timed period where the players are prompted to press a button. Upon
    /// pressing a button this cycle ends and a new one is prompted.
    /// </summary>
    /// <param name="attackMode"></param>
    void StartAttackCycle(AttackMode attackMode)
    {
        //\left(\left(5-1.5\right)\cdot \left(\frac{1}{\left(.15x+1\right)^2}\right)\ +\ 1.5\right)\cdot 1.05

        _gameState = GameState.AttackScreen;
        RenderAttackScreen();

        #region Determine Timer

        //The attack timer starts as a high duration and slowly gets smaller as the game drags on.
        //We want to make the active screen longer in PVP. It's a race against each other, not the clock.
        float fMaximumActiveScreenSeconds;
        if (_bossFight)
            fMaximumActiveScreenSeconds = InitialActiveScreenSeconds;
        else
            fMaximumActiveScreenSeconds = InitialActiveScreenSeconds * 5;

        float fMinimumActiveScreenSeconds;
        if (_bossFight)
            fMinimumActiveScreenSeconds = MinimumActiveScreenSeconds;
        else
            fMinimumActiveScreenSeconds = MinimumActiveScreenSeconds * 5;

        //Set the time span based on the current cycle.
        //_currentActivePeriodSeconds = InitialActiveScreenSeconds - (ActiveSecondsDecreasePerCycle * _completedCycles);
        _currentActivePeriodSeconds = ((fMaximumActiveScreenSeconds - fMinimumActiveScreenSeconds) * (1 / Mathf.Pow(ActiveScreenScalingFactor * _completedCycles + 1, 2)) + fMinimumActiveScreenSeconds) * _buffIncreaseActiveTimeMultiplier;

        //Time Left is the timer that controls the current cycle.
        //We set current time left and time left to guage the width of the timer bar.
        _currentTimeLeftSeconds = _currentActivePeriodSeconds;
        _timeLeft = _currentTimeLeftSeconds;

        #endregion

        #region New Button Selection

        //If this is a boss fight, only pick a button for the first team.
        // If it's a PVP battle, need to pick a button for each.

        //Start with Party 1, they will need a button regardless
        _buttonMaster.SelectNewButtonForParty1();

        //Let's check if the game is PVP, then select a button for party 2 (if necessary)
        if (!_bossFight)
        {
            _buttonMaster.SelectNewButtonForParty2();
        }

        #endregion

        //Track number of attack cycles to determine the speed of the timer.
        _completedCycles++;
    }

    void RenderAttackScreen()
    {
        foreach (IGamePanel gamePanel in _gamePanels)
        {
            if (gamePanel is AttackPanel)
            {
                gamePanel.Show();
            }
            else
            {
                gamePanel.Hide();
            }
        }
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

    void EndAttackCycle()
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
            if (_bossFight)
            {
                EndGameAsDefeat();
            }
            else
            {
                EndGameAsParty2Victory();
            }
        }
        else if (_party2Health <= 0)
        {
            //Party 2 Died. Was it a boss fight?
            if (_bossFight)
            {
                EndGameAsVictory();
            }
            else
            {
                EndGameAsParty1Victory();
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
                if (_bossFight)
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
                if (_bossFight)
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

                if (_bossFight)
                {
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
        if (_bossFight)
        {
            //If it's a boss fight, only punish team one... because they are the only team!
            ApplyDamageToParty1(true);
        }
        else
        {
            //What?! They BOTH failed!
            ApplyDamageToParty1(true);
            ApplyDamageToParty2(true);
        }

        BeginResolutionScreen();
    }

    void Party1InputFailByButtonPress()
    {
        ApplyDamageToParty1(true);
        BeginResolutionScreen();
    }

    void Party1InputFailByTie()
    {
        //TODO: Support for TIE screen.
        ApplyDamageToParty1(true);
        BeginResolutionScreen();
    }

    void Party2InputFailByButtonPress()
    {
        ApplyDamageToParty2(true);
        BeginResolutionScreen();
    }

    void Party2InputFailByTie()
    {
        ApplyDamageToParty2(true);
    }
    
    //If both players pressed at the same time... wow! It's a tie! Do something?
    void BothPartyInputTie()
    {
        //TODO: What do we do?
        //Fuck it, for now assume party 1 wins
        BeginResolutionScreen();
    }

    void Party1InputSuccess()
    {
        //Party 1 pressed their button. Apply damage to party 2/boss
        ApplyDamageToParty2(false);
        BeginResolutionScreen();
    }

    //They pressed the correct button. That means success.
    void Party2InputSuccess()
    {
        ApplyDamageToParty1(false);
        BeginResolutionScreen();
    }


    void EndGameAsDefeat()
    {
        _gameState = GameState.DefeatScreen;
    }

    void EndGameAsVictory()
    {
        _gameState = GameState.VictoryScreen;
    }

    void EndGameAsParty1Victory()
    {
        _gameState = GameState.Party1VictoryScreen;
    }

    void EndGameAsParty2Victory()
    {
        _gameState = GameState.Party2VictoryScreen;
    }

    void ApplyDamageToParty1(bool bSelfDamage)
    {
        if (_bossFight)
        {
            _party1Health = ApplyDamage(
                _party1Health,
                _party1StartHealth,
                BossMinimumDamagePerAttack,
                BossMaximumDamagePerAttack,
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
                PlayerMinimumDamagePerAttack,
                PlayerMaximumDamagePerAttack,
                _buffParty2CritChance,
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
            PlayerMinimumDamagePerAttack,
            PlayerMaximumDamagePerAttack,
            _buffParty1CritChance,
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
    
    #region Rendering Game State

    void RenderGameState()
    {
        ShowText(true, GameStateText);
        GameStateText.text = _gameState.ToString();

        if (_gameState == GameState.TitleScreen)
        {
            ShowText(true, TitleText);
            ShowText(false, BuffText);
            ShowText(false, GetReadyText);

            TimerPanel.SetActive(false);

            ShowText(false, Party1ButtonNameText);
            ShowText(false, Party2ButtonNameText);

            ShowText(false, PlayerHealthText);
            ShowText(false, BossHealthText);

            ShowText(false, YouLoseText);
            ShowText(false, YouWinText);

            Party1VictoryPanel.SetActive(false);
            Party2VictoryPanel.SetActive(false);

            OptionsPanel.SetActive(false);
            BossPanel.SetActive(false);
            PlayerPanel.SetActive(false);
        }
        else if (_gameState == GameState.OptionsScreen)
        {
            OptionsPanel.SetActive(true);

            ShowText(false, TitleText);
            ShowText(true, BuffText);
            UpdateBuffText();

            ShowText(false, GetReadyText);

            TimerPanel.SetActive(false);

            ShowText(false, Party1ButtonNameText);
            ShowText(false, Party2ButtonNameText);

            ShowText(false, PlayerHealthText);
            ShowText(false, BossHealthText);
            ShowText(false, FailText);
            ShowText(false, SuccessText);
            ShowText(false, YouLoseText);
            ShowText(false, YouWinText);

            Party1VictoryPanel.SetActive(false);
            Party2VictoryPanel.SetActive(false);

            OptionsPanel.SetActive(true);
            BossPanel.SetActive(false);
            PlayerPanel.SetActive(false);
        }
        else if (_gameState == GameState.PrepScreen)
        {
            OptionsPanel.SetActive(false);

            ShowText(false, TitleText);
            ShowText(false, BuffText);
            ShowText(true, GetReadyText);

            //Render Timer
            TimerPanel.SetActive(true);
            UpdateTimeLeftText();

            //Render Player Health
            UpdatePlayerHealthText();
            ShowText(true, PlayerHealthText);

            //Render Boss Health
            UpdateBossHealthText();
            ShowText(true, BossHealthText);

            ShowText(false, Party1ButtonNameText);
            ShowText(false, Party2ButtonNameText);

            ShowText(false, FailText);
            ShowText(false, SuccessText);
            ShowText(false, YouLoseText);
            ShowText(false, YouWinText);

            Party1VictoryPanel.SetActive(false);
            Party2VictoryPanel.SetActive(false);

            OptionsPanel.SetActive(false);
            BossPanel.SetActive(true);
            PlayerPanel.SetActive(true);
        }
        else if (_gameState == GameState.AttackScreen)
        {
            ShowText(false, TitleText);
            ShowText(false, BuffText);
            ShowText(false, GetReadyText);

            //Render Timer
            TimerPanel.SetActive(true);
            UpdateTimeLeftText();

            //Render Current Active Key
            UpdateButtonNameText();
            ShowText(true, Party1ButtonNameText);
            ShowText(!_bossFight, Party2ButtonNameText);


            //Render Player Health
            BossPanel.SetActive(true);
            PlayerPanel.SetActive(true);

            //Render Boss Health
            UpdateBossHealthText();
            ShowText(true, BossHealthText);

            //Hide Success and Failure Text
            ShowText(false, FailText);
            ShowText(false, SuccessText);
            ShowText(false, YouLoseText);
            ShowText(false, YouWinText);

            Party1VictoryPanel.SetActive(false);
            Party2VictoryPanel.SetActive(false);

            OptionsPanel.SetActive(false);
        }
        else if (_gameState == GameState.FailScreen)
        {
            ShowText(false, TitleText);
            ShowText(false, BuffText);
            ShowText(false, GetReadyText);
            ShowText(false, Party1ButtonNameText);
            ShowText(false, Party2ButtonNameText);

            //Render Timer
            TimerPanel.SetActive(false);

            //Render Player Health
            UpdatePlayerHealthText();
            ShowText(true, PlayerHealthText);

            //Render Boss Health
            UpdateBossHealthText();
            ShowText(true, BossHealthText);

            //Show Fail text
            ShowText(true, FailText);
            ShowText(false, SuccessText);
            ShowText(false, YouLoseText);
            ShowText(false, YouWinText);

            Party1VictoryPanel.SetActive(false);
            Party2VictoryPanel.SetActive(false);

            OptionsPanel.SetActive(false);
            BossPanel.SetActive(true);
            PlayerPanel.SetActive(true);
        }
        else if (_gameState == GameState.SuccessScreen)
        {
            ShowText(false, TitleText);
            ShowText(false, BuffText);
            ShowText(false, GetReadyText);
            ShowText(false, Party1ButtonNameText);
            ShowText(false, Party2ButtonNameText);

            //Render Timer
            TimerPanel.SetActive(false);

            //Render Player Health
            UpdatePlayerHealthText();
            ShowText(true, PlayerHealthText);

            //Render Boss Health
            UpdateBossHealthText();
            ShowText(true, BossHealthText);

            //Show Success text
            ShowText(false, FailText);
            ShowText(true, SuccessText);
            ShowText(false, YouLoseText);
            ShowText(false, YouWinText);

            Party1VictoryPanel.SetActive(false);
            Party2VictoryPanel.SetActive(false);

            OptionsPanel.SetActive(false);
            BossPanel.SetActive(true);
            PlayerPanel.SetActive(true);
        }
        else if (_gameState == GameState.DefeatScreen)
        {
            ShowText(false, TitleText);
            ShowText(false, BuffText);
            ShowText(false, GetReadyText);
            TimerPanel.SetActive(false);
            ShowText(false, Party1ButtonNameText);
            ShowText(false, Party2ButtonNameText);
            ShowText(false, PlayerHealthText);
            ShowText(false, BossHealthText);
            ShowText(false, FailText);
            ShowText(false, SuccessText);
            ShowText(true, YouLoseText);
            ShowText(false, YouWinText);

            Party1VictoryPanel.SetActive(false);
            Party2VictoryPanel.SetActive(false);

            OptionsPanel.SetActive(false);
            BossPanel.SetActive(false);
            PlayerPanel.SetActive(false);
        }
        else if (_gameState == GameState.VictoryScreen)
        {
            ShowText(false, TitleText);
            ShowText(false, BuffText);
            ShowText(false, GetReadyText);
            TimerPanel.SetActive(false);
            ShowText(false, Party1ButtonNameText);
            ShowText(false, Party2ButtonNameText);
            ShowText(false, PlayerHealthText);
            ShowText(false, BossHealthText);
            ShowText(false, FailText);
            ShowText(false, SuccessText);
            ShowText(false, YouLoseText);
            ShowText(true, YouWinText);

            Party1VictoryPanel.SetActive(false);
            Party2VictoryPanel.SetActive(false);

            OptionsPanel.SetActive(false);
            BossPanel.SetActive(false);
            PlayerPanel.SetActive(false);
        }
        else if (_gameState == GameState.Party1VictoryScreen)
        {
            ShowText(false, TitleText);
            ShowText(false, BuffText);
            ShowText(false, GetReadyText);
            TimerPanel.SetActive(false);
            ShowText(false, Party1ButtonNameText);
            ShowText(false, Party2ButtonNameText);
            ShowText(false, PlayerHealthText);
            ShowText(false, BossHealthText);
            ShowText(false, FailText);
            ShowText(false, SuccessText);
            ShowText(false, YouLoseText);
            ShowText(false, YouWinText);

            Party1VictoryPanel.SetActive(true);
            Party2VictoryPanel.SetActive(false);

            OptionsPanel.SetActive(false);
            BossPanel.SetActive(false);
            PlayerPanel.SetActive(false);
        }
        else if (_gameState == GameState.Party2VictoryScreen)
        {
            ShowText(false, TitleText);
            ShowText(false, BuffText);
            ShowText(false, GetReadyText);
            TimerPanel.SetActive(false);
            ShowText(false, Party1ButtonNameText);
            ShowText(false, Party2ButtonNameText);
            ShowText(false, PlayerHealthText);
            ShowText(false, BossHealthText);
            ShowText(false, FailText);
            ShowText(false, SuccessText);
            ShowText(false, YouLoseText);
            ShowText(false, YouWinText);

            Party1VictoryPanel.SetActive(false);
            Party2VictoryPanel.SetActive(true);

            OptionsPanel.SetActive(false);
            BossPanel.SetActive(false);
            PlayerPanel.SetActive(false);
        }
    }

    void UpdateBuffText()
    {
        BuffText.text = string.Format(
            "Active Buffs{0}{0}Boss Health: -{1}%{0}Player Health: +{2}%{0}Time Increase: +{3}%{0}Party 1 Crit: {4}%{0}Party 2 Crit: {4}%",
            "\r\n",
            ((int)(100 * _buffDecreaseBossHealthPercent)).ToString(),
            ((int)(100 * _buffIncreasePlayerHealthPercent)).ToString(),
            ((int)(100 * _buffIncreaseActiveTimePercent)).ToString(),
            ((int)(100 * _buffParty1CritChance)).ToString(),
            ((int)(100 * _buffParty2CritChance)).ToString()
            );
    }

    void UpdateTimeLeftText()
    {
        _timerPanel.SetTime(_timeLeft, _currentTimeLeftSeconds);
    }

    void UpdateButtonNameText()
    {
        //Always show player 1 button commands
        Party1ButtonNameText.text = _buttonMaster.GetCurrentParty1ActiveButton().Name + " - " + _buttonMaster.GetCurrentParty1ActiveButton().NumberKey.ToString();

        if (!_bossFight)
        {
            //If PVP, Show both button commands
            Party2ButtonNameText.text = _buttonMaster.GetCurrentParty2ActiveButton().Name + " - " + _buttonMaster.GetCurrentParty2ActiveButton().NumberKey.ToString();
        }
    }

    void UpdatePlayerHealthText()
    {
        PlayerHealthText.text = ((int)_party1Health).ToString();
    }

    void UpdateBossHealthText()
    {
        BossHealthText.text = ((int)_party2Health).ToString();
    }

    #endregion

    #region Show Text

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

    #region Utility Classes, Functions and Enums
    
    // Update is called once per frame
    IEnumerator UnthreadedDelay(float fSeconds, Action thingToExecute)
    {
        print(Time.time);
        yield return new WaitForSeconds(fSeconds);
        print(Time.time);
        thingToExecute();
    }

    void LogInputKeyPress()
    {
        if (Input.anyKeyDown)
        {
            string sCombinedKeyDown = "";
            foreach (KeyCode kcode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(kcode))
                {
                    sCombinedKeyDown += kcode + ", ";
                    sCombinedKeyDown += _buttonMaster.GetLetterKeyFromNumberKey(kcode) + ", ";
                }
            }
            Debug.Log("Key pressed: " + sCombinedKeyDown);

            if (_gameState == GameState.AttackScreen && _buttonMaster.GetCurrentParty1ActiveButton() != null)
            {
                GameButton party1Button = _buttonMaster.GetCurrentParty1ActiveButton();
                Debug.Log("Desired key: " + party1Button.LetterKey + ", " + party1Button.NumberKey);
            }
        }
    }
    
    enum GameState
    {
        TitleScreen,
        OptionsScreen,
        PrepScreen,
        FailScreen,
        SuccessScreen,
        AttackScreen,
        VictoryScreen,
        Party1VictoryScreen,
        Party2VictoryScreen,
        DefeatScreen
    }

    #endregion
}
