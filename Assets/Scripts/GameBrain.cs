using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameBrain : MonoBehaviour
{
    #region Private Members

    private float _timeLeft;
    
    private ButtonMaster _buttonMaster = new ButtonMaster();

    private GameState _gameState;
    private bool _bossFight = false;
    private float _buffParty1CritChance = 0;
    private float _buffParty2CritChance = 0;
    private float _buffIncreaseActiveTimeMultiplier = 1;
    private float _buffIncreaseActiveTimePercent = 0;
    private float _buffDecreaseBossHealthPercent = 0;
    private float _buffIncreasePlayerHealthPercent = 0;
    
    private float _pvpStartHealth;
    private float _pveStartHealth;
    private float _bossStartHealth;

    private List<GamePanel> _gamePanels = new List<GamePanel>();

    #endregion

    #region Public Properties

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
    public float PrepScreenSeconds;
    public float FailScreenSeconds;
    public float SuccessScreenSeconds;

    public float BossStartHealth;
    public float PVPStartHealth;
    public float PVEStartHealth;
    
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

    private OptionsPanel _buffPanel;
    private AttackPanel _attackPanel;

    #endregion

    #region Unity Built-In Functions

    // Use this for initialization
    void Start()
    {
        _buffPanel = this.OptionsPanel.GetComponent<OptionsPanel>();

        AssembleGamePanelsAndScripts();

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
                Debug.Log("Panel: " + gamePanel.name);
                _gamePanels.Add((GamePanel)gamePanel);
            }
        }

        _attackPanel = mainCanvas.GetComponentInChildren<AttackPanel>();
        //_buffPanel = mainCanvas.GetComponentInChildren<OptionsPanel>();
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
        _bossFight = true;
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
        
        //Don't apply debuffs/buffs during PVP.
        _pvpStartHealth = PVPStartHealth;

        //It's PVE... apply the debuffs to boss health.
        _bossStartHealth = BossStartHealth * (1 - _buffDecreaseBossHealthPercent);
        _pveStartHealth = PVEStartHealth * (1 + _buffIncreasePlayerHealthPercent);

        //Re-initialize the buttons for the current fight.
        _buttonMaster.SetupButtons(_bossFight);

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
    }

    void RenderPrepScreen()
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
    void EndPrepScreen()
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
        };

        _attackPanel.StartBattle(_buttonMaster, battleSettings);
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
                gamePanel.Show();
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
                gamePanel.Show();
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
        ShowText(true, GameStateText);
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
        return;
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
        AttackScreen,
        OutcomeScreen
    }

    #endregion
}
