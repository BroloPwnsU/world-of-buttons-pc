﻿using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameBrain : MonoBehaviour
{
    #region Private Members

    private float _timeLeft;

    private ButtonMaster _buttonMaster;

    private GameState _gameState;
    private bool _bossFight = false;
    private float _buffParty1CritChance = 0;
    private float _buffParty2CritChance = 0;
    private float _buffIncreaseActiveTimeMultiplier = 1;
    private float _buffIncreaseActiveTimePercent = 0;
    private float _buffDecreaseBossHealthPercent = 0;
    private float _buffIncreasePlayerHealthPercent = 0;

    private int _victoriesNeededToWin;
    private int _party1VictoryCount = 0;
    private int _party2VictoryCount = 0;

    private float _pvpStartHealth;
    private float _pveStartHealth;
    private float _bossStartHealth;

    private List<GamePanel> _gamePanels = new List<GamePanel>();
    
    #endregion

    #region Public Properties

    public bool DuplicateButtons = true;
    public bool FullSpread = false;
    public bool DebugMode = true;

    public GameObject MenuSelectorPrefab;

    public Text TitleText;
    public Text BuffText;
    public Text GetReadyText;
    public Text YouLoseText;
    public Text YouWinText;

    public Text GameStateText;

    public GameObject BattlePanel;
    public GameObject OptionsPanel;
    public GameObject Party1VictoryPanel;
    public GameObject Party2VictoryPanel;

    public float InitialActiveScreenSeconds;
    public float MinimumActiveScreenSeconds;
    public float ActiveScreenScalingFactor;
    public float GetReadyScreenSeconds;
    public float RoundVictoryScreenSeconds;
    public float FailScreenSeconds;
    public float SuccessScreenSeconds;

    public float BossStartHealth;
    public float PVPStartHealth;
    public float PVEStartHealth;

    public int DefaultRoundCount = 5;
    public float BossMaximumDamagePerAttack;
    public float BossMinimumDamagePerAttack;
    public float PlayerMinimumDamagePerAttack;
    public float PlayerMaximumDamagePerAttack;
    public int PlayerAttacksPerBossAttack;
    public float BuffPlayerTimeIncreasePercentPerTier;
    public float BuffBossHealthDecreasePercentPerTier;
    public float BuffPlayerHealthIncreasePercentPerTier;
    public float BuffCritChancePerTier;
    public float BuffNonsense;

    public float MusicVolume = 0.3f;

    private OptionsPanel _optionsPanel;
    private AttackPanel _attackPanel;

    #endregion

    #region Unity Built-In Functions

    // Use this for initialization
    void Awake()
    {
        _joystickMapping = JoystickMapping.LoadFromFile("joystick-mapping.json");

        _optionsPanel = this.OptionsPanel.GetComponent<OptionsPanel>();
        _buttonMaster = new ButtonMaster(_bossFight, _joystickMapping, DuplicateButtons, FullSpread);
        AssembleGamePanelsAndScripts();
    }

    private JoystickMapping _joystickMapping;

    void Start()
    {
        ApplyDefaults();

        //_gameButtonList = GetGameButtonList();
        RevertToTitleScreen();

    }

    void AssembleGamePanelsAndScripts()
    {
        GameObject mainCanvas = GameObject.Find("UITextCanvas");
        _gamePanels = new List<GamePanel>();
        foreach (MonoBehaviour gamePanel in mainCanvas.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (gamePanel is GamePanel)
            {
                _gamePanels.Add((GamePanel)gamePanel);
            }
        }

        _attackPanel = mainCanvas.GetComponentInChildren<AttackPanel>(true);
        //_buffPanel = mainCanvas.GetComponentInChildren<OptionsPanel>(true);
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
            //This button press will be handled by the title panel.

            #endregion
        }
        else if (_gameState == GameState.OptionsScreen)
        {
            #region Options - Admin choses game mode

            #endregion
        }
        else if (_gameState == GameState.Loading)
        {
            //Don't do anything. Let the loading panel handle it all.
        }
        else if (_gameState == GameState.GetReady)
        {
            #region Prep - Intro animation/instructions. Can't skip.

            // Only attendant can reset the game from here.
            //We will wait for the time period to end.
            _timeLeft -= Time.deltaTime;
            if (_timeLeft < 0)
            {
                //After the time expires, switch to a player cycle.
                EndGetReadyScreen();
            }

            #endregion
        }
        else if (_gameState == GameState.AttackScreen)
        {
            //Handled completely in the AttackPanel object
        }
        else if (_gameState == GameState.RoundVictoryScreen)
        {
            #region Prep - Intro animation/instructions. Can't skip.
            
            #endregion
        }
        else if (_gameState == GameState.OutcomeScreen)
        {
            #region Showing the victory screen. Press F1 or Esc to go back to title screen.

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
        _timeLeft = 0.0f;

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
        foreach (GamePanel gamePanel in _gamePanels)
        {
            if (gamePanel is TitlePanel)
            {
                ((TitlePanel)gamePanel).StartTitleScreen(_buttonMaster, _musicVolume, StartGame);
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

        OptionPanelSettings ops = GetOptionsSettings();
         
        _optionsPanel.StartOptions(ops, _buttonMaster, EndOptionsScreen);

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
        foreach (GamePanel gamePanel in _gamePanels)
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
    private string OPTION_MENU_ROUNDS = "ROUNDS";
    private string OPTION_MENU_CRIT_CHANCE = "CRIT CHANCE";
    private string OPTION_MENU_PLAYER_HEALTH = "HEALTH";
    private string OPTION_MENU_MUSIC_VOLUME = "MUSIC VOL";

    private int _roundCount;
    private float _musicVolume;

    void ApplyDefaults()
    {
        _roundCount = _joystickMapping.RoundCount;
        _pvpStartHealth = _joystickMapping.PVPStartHealth;
        _musicVolume = _joystickMapping.MusicVolume;
    }

    OptionPanelSettings GetOptionsSettings()
    {
        List<OptionMenu> bmList = new List<OptionMenu>();

        /*
        List<string> modeValues = new List<string>()
        {
            OPTION_MENU_MODE_VALUE_PVP,
            OPTION_MENU_MODE_VALUE_BOSS,
        };
        OptionMenu bmMode = new OptionMenu(
            OPTION_MENU_MODE,
            modeValues.ToArray(),
            modeValues[0]
            );

        bmList.Add(bmMode);
        */

        List<string> roundValues = new List<string>()
        {
            "1",
            "3",
            "5",
            "7"
        };
        
        OptionMenu omRounds = new OptionMenu(
            OPTION_MENU_ROUNDS,
            roundValues.ToArray(),
            _roundCount.ToString()
            );

        bmList.Add(omRounds);

        List<string> healthValues = new List<string>();
        for (int i = 1; i <= 20; i++)
        {
            healthValues.Add(
                Mathf.FloorToInt(PlayerMaximumDamagePerAttack * i).ToString()
                );
        }

        //Populate default from currently selected value.

        OptionMenu omHealth = new OptionMenu(
            OPTION_MENU_PLAYER_HEALTH,
            healthValues.ToArray(),
            Mathf.FloorToInt(_pvpStartHealth).ToString()
            );

        bmList.Add(omHealth);

        List<string> volumeValues = new List<string>();
        for (int i = 1; i <= 10; i++)
        {
            volumeValues.Add(i.ToString());
        }

        //Populate default from currently selected value.

        OptionMenu omMusicVolume = new OptionMenu(
            OPTION_MENU_MUSIC_VOLUME,
            volumeValues.ToArray(),
            Mathf.FloorToInt(_musicVolume * 10).ToString()
            );

        bmList.Add(omMusicVolume);

        /*
        List<string> critValues = new List<string>()
        {
            "0%",
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
        */

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
        Dictionary<string, string> selectedOptions = _optionsPanel.GetSelectedOptions();
        foreach (string sKey in selectedOptions.Keys)
        {
            Debug.Log("Option: " + sKey + " - " + selectedOptions[sKey]);
        }

        _bossFight = false;
        if (selectedOptions.ContainsKey(OPTION_MENU_MODE))
        {
            if (selectedOptions[OPTION_MENU_MODE] == OPTION_MENU_MODE_VALUE_BOSS)
            {
                _bossFight = true;
            }
            else
            {
                _bossFight = false;
            }
        }

        //Determine victories needed
        _victoriesNeededToWin = 1;
        _party1VictoryCount = 0;
        _party2VictoryCount = 0;
        if (selectedOptions[OPTION_MENU_ROUNDS] == "1")
        {
            _victoriesNeededToWin = 1;
            _roundCount = 1;
        }
        else if (selectedOptions[OPTION_MENU_ROUNDS] == "3")
        {
            _victoriesNeededToWin = 2;
            _roundCount = 3;
        }
        else if (selectedOptions[OPTION_MENU_ROUNDS] == "5")
        {
            _victoriesNeededToWin = 3;
            _roundCount = 5;
        }
        else if (selectedOptions[OPTION_MENU_ROUNDS] == "7")
        {
            _victoriesNeededToWin = 4;
            _roundCount = 7;
        }

        //Determine crit chance, if any
        float dCritChance = 0;
        if (selectedOptions.ContainsKey(OPTION_MENU_CRIT_CHANCE))
        {
            string sCritChance = selectedOptions[OPTION_MENU_CRIT_CHANCE];
            if (!String.IsNullOrEmpty(sCritChance))
            {
                sCritChance = sCritChance.Remove(sCritChance.Length - 1, 1);
                dCritChance = float.Parse(sCritChance) * .01f;
            }
        }

        if (_bossFight)
        {
            _buffParty1CritChance = dCritChance;
            _buffParty2CritChance = 0;
        }
        else
        {
            _buffParty1CritChance = dCritChance;
            _buffParty2CritChance = dCritChance;
        }

        //Determine Player Health
        float dPlayerHealth = _pvpStartHealth;
        if (selectedOptions.ContainsKey(OPTION_MENU_PLAYER_HEALTH))
        {
            string sPlayerHealth = selectedOptions[OPTION_MENU_PLAYER_HEALTH];
            if (!String.IsNullOrEmpty(sPlayerHealth))
            {
                dPlayerHealth = float.Parse(sPlayerHealth);
            }
        }

        //Apply health buffs/debuffs to starting health meters

        //If it's a PVP fight then player 1 uses the boss's stats. Otherwise it copies player 1.
        //We do this so the PVP fight lasts a good long while.

        //Don't apply debuffs/buffs during PVP.
        _pvpStartHealth = dPlayerHealth;

        //It's PVE... apply the debuffs to boss health.
        _bossStartHealth = BossStartHealth * (1 - _buffDecreaseBossHealthPercent);
        _pveStartHealth = PVEStartHealth * (1 + _buffIncreasePlayerHealthPercent);

        //Determine Music Volume
        if (selectedOptions.ContainsKey(OPTION_MENU_MUSIC_VOLUME))
        {
            string sMusicVolume = selectedOptions[OPTION_MENU_MUSIC_VOLUME];
            if (!String.IsNullOrEmpty(sMusicVolume))
            {
                _musicVolume = float.Parse(sMusicVolume) * 0.1f;
            }
        }

        //Re-initialize the buttons for the current fight.
        _buttonMaster.SetupButtons(_bossFight);

        StartGetReadyScreen();
        //StartLoadingScreen();
    }

    #endregion

    #region State - Prep / Get Ready

    /// <summary>
    /// Prep screen is the countdown before the game starts, after the options and loading screens, before the active screen.
    /// </summary>
    void StartGetReadyScreen()
    {
        _gameState = GameState.GetReady;
        RenderGetReadyScreen();

        _timeLeft = GetReadyScreenSeconds;
    }

    void RenderGetReadyScreen()
    {
        foreach (GamePanel gamePanel in _gamePanels)
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
    void EndGetReadyScreen()
    {
        StartLoadingScreen();
    }

    #endregion
    
    #region State - Loading Screen

    /// <summary>
    /// Like pressing Reset on the game. Goes back to title screen and clears out the current game.
    /// </summary>
    void StartLoadingScreen()
    {
        _gameState = GameState.Loading;
        RenderLoadingScreen();
    }

    /// <summary>
    /// Show the title, hide everything else.
    /// </summary>
    void RenderLoadingScreen()
    {
        //Iterate through the panels to hide them, unless they are on the acceptable list.
        foreach (GamePanel gamePanel in _gamePanels)
        {
            if (gamePanel is LoadingPanel)
            {
                ((LoadingPanel)gamePanel).StartLoadingPanel(_joystickMapping.LoadingScreenSeconds, EndLoadingScreen);
            }
            else
            {
                gamePanel.Hide();
            }
        }
    }

    void EndLoadingScreen()
    {
        StartAttack();
    }

    #endregion
    
    #region State - Attack

    void StartAttack()
    {
        _gameState = GameState.AttackScreen;
        RenderAttackScreen();

        BattleSettings battleSettings = new BattleSettings()
        {
            VictoriesNeededToWin = _victoriesNeededToWin,
            Party1CurrentVictories = _party1VictoryCount,
            Party2CurrentVictories = _party2VictoryCount,
            BossFight = _bossFight,
            PVEStartHealth = _pveStartHealth,
            PVPStartHealth = _pvpStartHealth,
            BossStartHealth = _bossStartHealth,
            InitialActiveScreenSeconds = InitialActiveScreenSeconds,
            MinimumActiveScreenSeconds = MinimumActiveScreenSeconds,
            ActiveScreenScalingFactor = ActiveScreenScalingFactor,
            BossMaximumDamagePerAttack = BossMaximumDamagePerAttack,
            BossMinimumDamagePerAttack = BossMinimumDamagePerAttack,
            PlayerMinimumDamagePerAttack = PlayerMinimumDamagePerAttack,
            PlayerMaximumDamagePerAttack = PlayerMaximumDamagePerAttack,
            BuffIncreaseActiveTimeMultiplier = _buffIncreaseActiveTimeMultiplier,
            BuffParty1CritChance = _buffParty1CritChance,
            BuffParty2CritChance = _buffParty2CritChance,
            MusicVolume = _musicVolume,
            TestMode = _joystickMapping.TestMode,
            EndRoundNotification = EndRoundNotification
        };

        _attackPanel.StartBattle(_buttonMaster, battleSettings);
    }

    void EndRoundNotification(RoundResult roundResult)
    {
        //Increment the appropirate counter to show who is winning
        if (roundResult.Victor == BattleVictor.Party1
            || roundResult.Victor == BattleVictor.PVE)
        {
            _party1VictoryCount++;
        }
        else if (roundResult.Victor == BattleVictor.Party2
            || roundResult.Victor == BattleVictor.Boss)
        {
            _party2VictoryCount++;
        }
        else
        {
            //Fuck you
        }

        //We've updated the victory count for these fucks. Now hide the battle panel.
        // Then, depending on the number of current victories, either show a TRIUMPH! screen of some sort,
        // or trigger a new combat sequence.
        BattlePanel.SetActive(false);

        if (_party1VictoryCount >= _victoriesNeededToWin)
        {
            if (_bossFight)
            {
                StartPVEVictory();
            }
            else
            {
                StartParty1Victory();
            }
        }
        else if (_party2VictoryCount >= _victoriesNeededToWin)
        {
            if (_bossFight)
            {
                StartBossVictory();
            }
            else
            {
                StartParty2Victory();
            }
        }
        else
        {
            //Nobody won yet. Need to fight another round.
            StartLoadingScreen();
        }
    }

    void RenderAttackScreen()
    {
        foreach (GamePanel gamePanel in _gamePanels)
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

    #endregion

    #region State - Victory Panels

    void StartParty1Victory()
    {
        _gameState = GameState.OutcomeScreen;
        RenderParty1Victory();
    }

    void StartParty2Victory()
    {
        _gameState = GameState.OutcomeScreen;
        RenderParty2Victory();
    }

    void StartPVEVictory()
    {
        _gameState = GameState.OutcomeScreen;
        RenderPVEVictory();
    }

    void StartBossVictory()
    {
        _gameState = GameState.OutcomeScreen;
        RenderBossVictory();
    }

    void RenderParty1Victory()
    {
        foreach (GamePanel gamePanel in _gamePanels)
        {
            if (gamePanel is Party1VictoryPanel)
            {
                ((Party1VictoryPanel)gamePanel).Show(_musicVolume);
            }
            else
            {
                gamePanel.Hide();
            }
        }
    }

    void RenderParty2Victory()
    {
        foreach (GamePanel gamePanel in _gamePanels)
        {
            if (gamePanel is Party2VictoryPanel)
            {
                ((Party2VictoryPanel)gamePanel).Show(_musicVolume);
            }
            else
            {
                gamePanel.Hide();
            }
        }
    }

    void RenderPVEVictory()
    {
        foreach (GamePanel gamePanel in _gamePanels)
        {
            if (gamePanel is PVEVictoryPanel)
            {
                gamePanel.Show();
            }
            else
            {
                gamePanel.Hide();
            }
        }
    }

    void RenderBossVictory()
    {
        foreach (GamePanel gamePanel in _gamePanels)
        {
            if (gamePanel is BossVictoryPanel)
            {
                gamePanel.Show();
            }
            else
            {
                gamePanel.Hide();
            }
        }
    }

    #endregion

    #region Rendering Game State

    void RenderGameState()
    {
        TextUtility.ShowText(true, GameStateText);
        GameStateText.text = _gameState.ToString();
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

    #endregion

    #region Utility Classes, Functions and Enums
    
    // Update is called once per frame
    IEnumerator UnthreadedDelay(float fSeconds, Action thingToExecute)
    {
        yield return new WaitForSeconds(fSeconds);
        thingToExecute();
    }

    void LogInputKeyPress()
    {
        /*
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
        */
    }
    
    enum GameState
    {
        TitleScreen,
        OptionsScreen,
        GetReady,
        Loading,
        AttackScreen,
        RoundVictoryScreen,
        OutcomeScreen
    }

    #endregion
}
