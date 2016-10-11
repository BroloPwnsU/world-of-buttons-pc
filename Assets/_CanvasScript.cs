using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class _CanvasScript : MonoBehaviour
{
    #region Private Members
    private float _timeLeft;
    private float _currentTimeLeftSeconds;
    private float _currentActivePeriodSeconds;
    private int _completedCycles;


    private List<GameButton> _player1Buttons;
    private List<GameButton> _player2Buttons;
    private List<GameButton> _player3Buttons;
    private List<GameButton> _player4Buttons;
    private GameButton _party1DummyButton;
    private GameButton _party2DummyButton;
    private GameButton _party1CurrentButton;
    private GameButton _party1PreviousButton;
    private GameButton _party2CurrentButton;
    private GameButton _party2PreviousButton;
    private List<GameButton> _party1ActiveButtons;
    private List<GameButton> _party2ActiveButtons;

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
    #endregion

    #region Public Properties
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

    private PartyGroupBrain _partyScript1;
    private PartyGroupBrain _partyScript2;
    private TimerPanelBrain _timerPanelScript;
    private BuffPanel _buffPanelScript;
    #endregion

    // Use this for initialization
    void Start()
    {
        //_gameButtonList = GetGameButtonList();
        _partyScript1 = PlayerPanel.GetComponent<PartyGroupBrain>();
        _partyScript2 = BossPanel.GetComponent<PartyGroupBrain>();
        _timerPanelScript = TimerPanel.GetComponent<TimerPanelBrain>();
        _buffPanelScript = OptionsPanel.GetComponent<BuffPanel>();

        RevertToTitleScreen();
    }

    // Update is called once per frame

    void Update()
    {
        //If at any time they hit the escape key it goes back to the title screen.
        // This doesn't care about current game state.
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            RevertToTitleScreen();
            RenderGameState();
            return;
        }

        //All other key commands are dependent on the game state.
        if (_gameState == GameState.TitleScreen)
        {
            #region Title Screen. Must press Enter to start game.

            //From the title screen, only the attendant can actually start the game.
            //Attendant must press Enter to start.
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
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
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                EndOptionsScreen();
            }

            ///NOT IMPLEMENTED

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
        else if (_gameState == GameState.ActiveScreen)
        {
            #region Active - Players are prompted for a button press.

            ///Active state means we're waiting for the player to press a button. They are under a time limit.
            if (!_bossFight && Input.GetKeyDown(_party1CurrentButton.Key) && Input.GetKeyDown(_party2CurrentButton.Key))
            {
                //If both players pressed at the same time... wow! It's a tie! Do something?
                PartyInputTie();
            }
            else if (Input.GetKeyDown(_party1CurrentButton.Key))
            {
                //They pressed the correct button. That means success.
                Party1InputSuccess();
            }
            else if (!_bossFight && Input.GetKeyDown(_party2CurrentButton.Key))
            {
                //They pressed the correct button. That means success.
                Party2InputSuccess();
            }
            else if (Input.anyKeyDown)
            {
                //Somebody is pressing a button but it's not the right button.
                //It could be either player, but we gotta figure out who screwed up.
                FailByButtonPress();
            }
            else
            {
                //If we're currently active, and we have not received a keypress, then we make sure they are within the time limit.
                _timeLeft -= Time.deltaTime;
                if (_timeLeft < 0)
                {
                    FailByTimeElapsed();
                }
            }

            #endregion
        }
        else if (_gameState == GameState.SuccessScreen)
        {
            #region Success - Player pressed a correct button.

            //They got the right button. Suspend input for a few seconds while an animation plays.
            _timeLeft -= Time.deltaTime;
            if (_timeLeft < 0)
            {
                EndSuccessScreen();
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
                EndFailScreen();
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

    #region Startup

    void ResetGameValues()
    {
        _party1Health = 0.0f;
        _party2Health = 0.0f;
        _completedCycles = 0;
        _timeLeft = 0.0f;
        _currentTimeLeftSeconds = 0.0f;
        
        _player1Buttons = null;
        _player2Buttons = null;
        _player3Buttons = null;
        _player4Buttons = null;
        _party1DummyButton = null;
        _party2DummyButton = null;
        _party1CurrentButton = null;
        _party1PreviousButton = null;
        _party2CurrentButton = null;
        _party2PreviousButton = null;
        _party1ActiveButtons = null;
        _party2ActiveButtons = null;

        SetupButtons();
    }

    #endregion
    
    #region Game Logic

    void RevertToTitleScreen()
    {
        _gameState = GameState.TitleScreen;
        ResetGameValues();
    }

    void StartGame()
    {
        StartOptionsScreen();
    }

    void StartOptionsScreen()
    {
        _gameState = GameState.OptionsScreen;

        _buffPanelScript.StartOptions(GetOptionsSettings());

        //Assign some random values for now
        _bossFight = true;
        _buffIncreaseActiveTimePercent = Random.Range(0, 5) * (0.01f * BuffCritChancePerTier);
        _buffIncreaseActiveTimeMultiplier = 1 + _buffIncreaseActiveTimePercent;
        _buffDecreaseBossHealthPercent = Random.Range(0, 5) * (0.01f * BuffBossHealthDecreasePercentPerTier);
        _buffIncreasePlayerHealthPercent = Random.Range(0, 5) * (0.01f * BuffPlayerHealthIncreasePercentPerTier);
        _buffParty1CritChance = Random.Range(0, 5) * (0.01f * BuffCritChancePerTier);
        _buffParty2CritChance = Random.Range(0, 5) * (0.01f * BuffCritChancePerTier);
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
        //First just figure out if it's a boss fight or not.
        Dictionary<string, string> selectedOptions = _buffPanelScript.GetSelectedOptions();
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
        _party1StartHealth = PlayerStartHealth * (1 + _buffIncreasePlayerHealthPercent);
        _party1Health = _party1StartHealth;
        
        //If it's a boss fight then player 2 uses the boss's stats. Otherwise it copies player 1.
        if (_bossFight)
        {
            _party2StartHealth = BossStartHealth * (1 - _buffDecreaseBossHealthPercent);
        }
        else
        {
            _party2StartHealth = _party1StartHealth;
        }
        _party2Health = _party2StartHealth;


        //Want to clean up old rendering of things from previous games
        _partyScript1.RefreshHealth(_party1StartHealth, _party1StartHealth);
        _partyScript2.RefreshHealth(_party2StartHealth, _party2StartHealth);

        //Skip to prep screen
        BeginPrepScreen();
    }

    void BeginPrepScreen()
    {
        _gameState = GameState.PrepScreen;
        _timeLeft = PrepScreenSeconds;
        _currentTimeLeftSeconds = _timeLeft;
    }

    void EndPrepScreen()
    {
        StartActiveCycle();
    }

    void StartActiveCycle()
    {
        //\left(\left(5-1.5\right)\cdot \left(\frac{1}{\left(.15x+1\right)^2}\right)\ +\ 1.5\right)\cdot 1.05

        _gameState = GameState.ActiveScreen;

        //Set the time span based on the current cycle.
        //_currentActivePeriodSeconds = InitialActiveScreenSeconds - (ActiveSecondsDecreasePerCycle * _completedCycles);
        _currentActivePeriodSeconds = ((InitialActiveScreenSeconds - MinimumActiveScreenSeconds) * (1 / Mathf.Pow(ActiveScreenScalingFactor * _completedCycles + 1, 2)) + MinimumActiveScreenSeconds) * _buffIncreaseActiveTimeMultiplier;

        _timeLeft = _currentActivePeriodSeconds;
        _currentTimeLeftSeconds = _timeLeft;

        //If this is a boss fight, only pick a button for the first team.
        // If it's a PVP battle, need to pick a button for each.

        //Start with Party 1, they will need a button regardless
        _party1CurrentButton = GetRandomButton(_party1ActiveButtons, _party1CurrentButton, _party1PreviousButton);

        //Let's check if the game is PVP, then select a button for party 2 (if necessary)
        if (!_bossFight)
        {
            _party2CurrentButton = GetRandomButton(_party2ActiveButtons, _party2CurrentButton, _party2PreviousButton);
        }

        _completedCycles++;
    }

    static GameButton GetRandomButton(List<GameButton> gameButtons, GameButton currentButton, GameButton previousButton)
    {
        int wButtonCount = gameButtons.Count;
        if (currentButton == null)
        {
            //The first button will be truly random because we don't have to worry about copying a previous button.
            return gameButtons[Random.Range(0, wButtonCount)];
        }
        else
        {
            //If we've already got a current button then we need to randomly select a new one... but not the same one.
            GameButton newButton = currentButton;
            int wCount = 0;
            while ((newButton.Key == currentButton.Key || (previousButton != null && newButton.Key == previousButton.Key)) && wCount < wButtonCount)
            {
                int wRandom = Random.Range(0, wButtonCount);
                //Keep cycling until we get a different random key than the one 
                newButton = gameButtons[wRandom];
                wCount++;
            }

            //NewButton should not be the same as current button, so replace it.
            return newButton;
        } 
    }

    void FailByButtonPress()
    {
        //First need to figure out which party fucked up
        if (_bossFight)
        {
            //If it's a boss fight, then we know it was party one, cuz there's only one party one.
            Party1InputFailByButtonPress();
        }
        else
        {
            //Iterate through the button list and find the fail button.
            if (Input.GetKeyDown(_party1DummyButton.Key))
            {
                Party1InputFailByButtonPress();
            }
            else if (Input.GetKeyDown(_party2DummyButton.Key))
            {
                Party2InputFailByButtonPress();
            }
            else
            {
                //Have to iterate through both team's buttons until we find the one that sucks.
                bool bButtonFound = true;
                foreach (GameButton gb in _party1ActiveButtons)
                {
                    if (Input.GetKeyDown(gb.Key))
                    {
                        Party1InputFailByButtonPress();
                        bButtonFound = true;
                        break;
                    }
                }

                //Actually, it could be both parties failing at the same time. Punish them both!
                foreach (GameButton gb in _party2ActiveButtons)
                {
                    if (Input.GetKeyDown(gb.Key))
                    {
                        Party2InputFailByButtonPress();
                        bButtonFound = true;
                        break;
                    }
                }

                //Do we care if we found a button press or not?
            }
        }

        BeginFailScreen();
    }

    void FailByTimeElapsed()
    {
        if (_bossFight)
        {
            //If it's a boss fight, only punish team one... because they are the only team!
            Party1InputFailByTime();
        }
        else
        {
            //What?! They BOTH failed!
            Party1InputFailByTime();
            Party2InputFailByTime();
        }

        BeginFailScreen();
    }

    void Party1InputFailByButtonPress()
    {
        ApplyDamageToParty1();
    }

    void Party2InputFailByButtonPress()
    {
        ApplyDamageToParty2();
    }

    void Party1InputFailByTime()
    {
        //TEMPORARY: Later in development we will not be assigning damage for timeouts.
        ApplyDamageToParty1();
    }

    void Party2InputFailByTime()
    {
        //TEMPORARY: Later in development we will not be assigning damage for timeouts.
        ApplyDamageToParty2();
    }
    
    //If both players pressed at the same time... wow! It's a tie! Do something?
    void PartyInputTie()
    {
        //What do we do?
        //Fuck it, for now assume party 1 wins
    }

    void Party1InputSuccess()
    {
        //Party 1 pressed their button. Apply damage to party 2/boss
        ApplyDamageToParty2();
        BeginSuccessScreen();
    }

    //They pressed the correct button. That means success.
    void Party2InputSuccess()
    {

    }

    void BeginSuccessScreen()
    {
        //Set the pause duration.
        _timeLeft = SuccessScreenSeconds;
        _currentTimeLeftSeconds = _timeLeft;
        _gameState = GameState.SuccessScreen;
    }

    void EndSuccessScreen()
    {
        //Some damage may have occurred against the boss during a success action.
        //Check to see if the boss died.
        if (_party2Health <= 0)
        {
            EndGameAsVictory();
        }
        else
        {
            //There is still some health. Go back to the active screen.
            StartActiveCycle();
        }
    }

    void BeginFailScreen()
    {
        _timeLeft = FailScreenSeconds;
        _currentTimeLeftSeconds = _timeLeft;
        _gameState = GameState.FailScreen;
    }

    void EndFailScreen()
    {
        //Some damage may have occurred against the user during a fail action.
        //Check to see if the player died.
        if (_party1Health <= 0)
        {
            EndGameAsDefeat();
        }
        else
        {
            //There is still some health. Go back to a keypress.
            StartActiveCycle();
        }
    }

    void EndGameAsDefeat()
    {
        _gameState = GameState.DefeatScreen;
    }

    void EndGameAsVictory()
    {
        _gameState = GameState.VictoryScreen;
    }

    void ApplyDamageToParty1()
    {
        if (_bossFight)
        {
            _party1Health = ApplyDamage(
                _party1Health,
                _party1StartHealth,
                BossMinimumDamagePerAttack,
                BossMaximumDamagePerAttack,
                0.0f,
                _partyScript2,
                _partyScript1
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
                _partyScript2,
                _partyScript1
                );
        }
    }

    void ApplyDamageToParty2()
    {
        _party2Health = ApplyDamage(
            _party2Health,
            _party2StartHealth,
            PlayerMinimumDamagePerAttack,
            PlayerMaximumDamagePerAttack,
            _buffParty1CritChance,
            _partyScript1,
            _partyScript2
            );
    }

    private float ApplyDamage(float currentHealth, float startHealth, float minimumDamage, float maximumDamage, float critPercent, PartyGroupBrain partyScriptAttack, PartyGroupBrain partyScriptDefend)
    {
        //Get a random damage value from within the specific player attack range
        float damage = Mathf.FloorToInt(Random.Range(
            minimumDamage,
            maximumDamage
            ));

        //Determine if it's a crit.
        bool bCrit = Random.Range(0.0f, 1.0f) < critPercent;
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

        partyScriptAttack.MakeAttack(bCrit);

        partyScriptDefend.TakeDamage(
            damage,
            newHealth,
            startHealth,
            bCrit
            );

        return newHealth;
    }

    #endregion

    #region Rendering Methods

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
            ShowText(false, FailText);
            ShowText(false, SuccessText);
            ShowText(false, YouLoseText);
            ShowText(false, YouWinText);

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

            OptionsPanel.SetActive(false);
            BossPanel.SetActive(true);
            PlayerPanel.SetActive(true);
        }
        else if (_gameState == GameState.ActiveScreen)
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
            UpdatePlayerHealthText();
            ShowText(true, PlayerHealthText);

            //Render Boss Health
            UpdateBossHealthText();
            ShowText(true, BossHealthText);

            //Hide Success and Failure Text
            ShowText(false, FailText);
            ShowText(false, SuccessText);
            ShowText(false, YouLoseText);
            ShowText(false, YouWinText);

            OptionsPanel.SetActive(false);
            BossPanel.SetActive(true);
            PlayerPanel.SetActive(true);
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
        _timerPanelScript.SetTime(_timeLeft, _currentTimeLeftSeconds);
    }

    void UpdateButtonNameText()
    {
        //Always show player 1 button commands
        Party1ButtonNameText.text = _party1CurrentButton.Name + " - " + _party1CurrentButton.Key.ToString();

        if (!_bossFight)
        {
            //If PVP, Show both button commands
            Party2ButtonNameText.text = _party2CurrentButton.Name + " - " + _party2CurrentButton.Key.ToString();
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

    #region Game Button Definition

    void SetupButtons()
    {
        _player1Buttons = GetPlayer1Buttons();
        _player2Buttons = GetPlayer2Buttons();
        _player3Buttons = GetPlayer3Buttons();
        _player4Buttons = GetPlayer4Buttons();
        _party1DummyButton = GetParty1DummyButton();
        _party2DummyButton = GetParty2DummyButton();

        //The party buttons are the list of buttons we can use for the next random button press.
        _party1ActiveButtons = new List<GameButton>();
        _party2ActiveButtons = new List<GameButton>();
        if (_bossFight)
        {
            //Boss fight combines all the buttons.
            _party1ActiveButtons.AddRange(_player1Buttons);
            _party1ActiveButtons.AddRange(_player2Buttons);
            _party1ActiveButtons.AddRange(_player3Buttons);
            _party1ActiveButtons.AddRange(_player4Buttons);

            //Leave Party2ActiveButtons empty, because there is no party 2. Boss is AI!
        }
        else
        {
            //PVP
            //Player 1 and 2 on party 1
            _party1ActiveButtons.AddRange(_player1Buttons);
            _party1ActiveButtons.AddRange(_player2Buttons);

            //player 3 and 4 on party 2
            _party2ActiveButtons.AddRange(_player3Buttons);
            _party2ActiveButtons.AddRange(_player4Buttons);
        }
    }

    List<GameButton> GetPlayer1Buttons()
    {
        List<GameButton> gbList = new List<GameButton>();

        gbList.Add(new GameButton(KeyCode.A, "Kill With Fire"));
        gbList.Add(new GameButton(KeyCode.B, "Turn Off and Back On"));
        gbList.Add(new GameButton(KeyCode.C, "360 No Scope"));
        gbList.Add(new GameButton(KeyCode.D, "Tank and Spank"));
        gbList.Add(new GameButton(KeyCode.E, "Falcon Punch!"));
        gbList.Add(new GameButton(KeyCode.F, "Overcook the Roast"));
        gbList.Add(new GameButton(KeyCode.G, "Put Gum in Hair"));
        gbList.Add(new GameButton(KeyCode.H, "Spray and Pray"));
        gbList.Add(new GameButton(KeyCode.I, "Aggrevate Old Tap-Dancing Injury"));
        gbList.Add(new GameButton(KeyCode.J, "Flying Groin Stomp"));

        return gbList;
    }

    List<GameButton> GetPlayer2Buttons()
    {
        List<GameButton> gbList = new List<GameButton>();

        gbList.Add(new GameButton(KeyCode.K, "Flail Wildly"));
        gbList.Add(new GameButton(KeyCode.L, "Charge-a Your Laser"));
        gbList.Add(new GameButton(KeyCode.M, "Don't Send Christmas Card"));
        gbList.Add(new GameButton(KeyCode.N, "Spin to Win"));
        gbList.Add(new GameButton(KeyCode.O, "420 Blazem"));
        gbList.Add(new GameButton(KeyCode.P, "Kill with Kindness"));
        gbList.Add(new GameButton(KeyCode.Q, "Give Just One Chip"));
        gbList.Add(new GameButton(KeyCode.R, "Run in Circles"));
        gbList.Add(new GameButton(KeyCode.S, "Press Alt + F4"));
        gbList.Add(new GameButton(KeyCode.T, "Fap Quietly to Food Network"));

        return gbList;
    }

    List<GameButton> GetPlayer3Buttons()
    {
        List<GameButton> gbList = new List<GameButton>();

        gbList.Add(new GameButton(KeyCode.U, "Rage Quietly"));
        gbList.Add(new GameButton(KeyCode.V, "Download DLC"));
        gbList.Add(new GameButton(KeyCode.W, "Cheat"));
        gbList.Add(new GameButton(KeyCode.X, "Kill the Messenger"));
        gbList.Add(new GameButton(KeyCode.Y, "Grab Pitchfork"));
        gbList.Add(new GameButton(KeyCode.Z, "Jump on the Bandwagon"));
        gbList.Add(new GameButton(KeyCode.LeftBracket, "Smoke (If you got em)"));
        gbList.Add(new GameButton(KeyCode.RightBracket, "Fire da Lazzzooor"));
        gbList.Add(new GameButton(KeyCode.Comma, "Gank Top"));
        gbList.Add(new GameButton(KeyCode.Period, "Roll Need"));

        return gbList;
    }

    List<GameButton> GetPlayer4Buttons()
    {
        List<GameButton> gbList = new List<GameButton>();

        gbList.Add(new GameButton(KeyCode.Alpha0, "Uninstall (Noob)"));
        gbList.Add(new GameButton(KeyCode.Alpha1, "Spend Real Money in the Item Shop"));
        gbList.Add(new GameButton(KeyCode.Alpha2, "Crush Candy"));
        gbList.Add(new GameButton(KeyCode.Alpha3, "Play a Casual Game"));
        gbList.Add(new GameButton(KeyCode.Alpha4, "Disenchant Legendary Item"));
        gbList.Add(new GameButton(KeyCode.Alpha5, "Lightning Bolt!"));
        gbList.Add(new GameButton(KeyCode.Alpha6, "Steal the Schnitzel"));
        gbList.Add(new GameButton(KeyCode.Alpha7, "Do a Cock Pushup"));
        gbList.Add(new GameButton(KeyCode.Alpha8, "Walk Slowly on the Beach"));
        gbList.Add(new GameButton(KeyCode.Alpha9, "Turn it up to 11"));

        return gbList;
    }

    GameButton GetParty1DummyButton()
    {
        return new GameButton(KeyCode.Semicolon, "Party 1 Dummy");
    }

    GameButton GetParty2DummyButton()
    {
        return new GameButton(KeyCode.Slash, "Party 2 Dummy");
    }

    List<GameButton> GetParty1Buttons()
    {
        List<GameButton> combinedList = new List<GameButton>();

        combinedList.Add(GetParty1DummyButton());
        combinedList.AddRange(GetPlayer1Buttons());
        combinedList.AddRange(GetPlayer2Buttons());

        return combinedList;
    }

    List<GameButton> GetParty2Buttons()
    {
        List<GameButton> combinedList = new List<GameButton>();

        combinedList.Add(GetParty2DummyButton());
        combinedList.AddRange(GetPlayer3Buttons());
        combinedList.AddRange(GetPlayer4Buttons());

        return combinedList;
    }

    #endregion

    #region Utility Classes and Enums

    public class GameButton
    {
        public string Name;
        public KeyCode Key;

        public GameButton(KeyCode kcKey, string sName)
        {
            this.Key = kcKey;
            this.Name = sName;
        }
    }

    enum GameState
    {
        TitleScreen,
        OptionsScreen,
        PrepScreen,
        FailScreen,
        SuccessScreen,
        ActiveScreen,
        VictoryScreen,
        DefeatScreen
    }

    #endregion
}
