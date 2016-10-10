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
    private GameButton _currentButton;

    private List<GameButton> _gameButtonList;

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
    public Text ButtonNameText;
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

    public GameObject BuffPanel;
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
    #endregion

    // Use this for initialization
    void Start()
    {
        _gameButtonList = GetGameButtonList();
        _partyScript1 = PlayerPanel.GetComponent<PartyGroupBrain>();
        _partyScript2 = BossPanel.GetComponent<PartyGroupBrain>();
        _timerPanelScript = TimerPanel.GetComponent<TimerPanelBrain>();

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
            if (Input.GetKeyDown(_currentButton.Key))
            {
                //They pressed the correct button. That means success.
                Player1InputSuccess();
            }
            else if (Input.anyKeyDown)
            {
                //Somebody is pressing a button but it's not the right button.
                Player1InputFailByButtonPress();
            }
            else
            {
                //If we're currently active, and we have not received a keypress, then we make sure they are within the time limit.
                _timeLeft -= Time.deltaTime;
                if (_timeLeft < 0)
                {
                    Player1InputFailByTime();
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

        //Assign some random values for now
        _bossFight = true;
        _buffIncreaseActiveTimePercent = Random.Range(0, 5) * (0.01f * BuffCritChancePerTier);
        _buffIncreaseActiveTimeMultiplier = 1 + _buffIncreaseActiveTimePercent;
        _buffDecreaseBossHealthPercent = Random.Range(0, 5) * (0.01f * BuffBossHealthDecreasePercentPerTier);
        _buffIncreasePlayerHealthPercent = Random.Range(0, 5) * (0.01f * BuffPlayerHealthIncreasePercentPerTier);
        _buffParty1CritChance = Random.Range(0, 5) * (0.01f * BuffCritChancePerTier);
        _buffParty2CritChance = Random.Range(0, 5) * (0.01f * BuffCritChancePerTier);
    }

    void EndOptionsScreen()
    {
        //Apply health buffs/debuffs to starting health meters
        _party1StartHealth = PlayerStartHealth * (1 + _buffIncreasePlayerHealthPercent);
        _party1Health = _party1StartHealth;
        
        //If it's a boss fight then player 2 uses the boss's stats. Otherwise it copies player 1.
        if (_bossFight)
        {
            _party2StartHealth = _party1StartHealth;
        }
        else
        {
            _party2StartHealth = BossStartHealth * (1 - _buffDecreaseBossHealthPercent);
        }
        _party2Health = _party2StartHealth;

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

        int wGameButtonCount = _gameButtonList.Count;

        //Grab a new key
        if (_currentButton == null)
        {
            _currentButton = _gameButtonList[Random.Range(0, wGameButtonCount)];
        }
        else
        {
            //If we've already got a current button then we need to randomly select a new one... but not the same one.
            GameButton newButton = _currentButton;
            int wCount = 0;
            while (newButton.Key == _currentButton.Key && wCount < wGameButtonCount)
            {
                int wRandom = Random.Range(0, wGameButtonCount);
                //Keep cycling until we get a different random key than the one 
                newButton = _gameButtonList[wRandom];
                Debug.Log(wRandom);
                wCount++;
            }

            //NewButton should not be the same as current button, so replace it.
            _currentButton = newButton;
        }

        _completedCycles++;
    }

    void Player1InputFailByButtonPress()
    {
        //Incorrect button presses result in damage
        ApplyDamageToParty1();
        BeginFailScreen();
    }

    void Player1InputFailByTime()
    {
        //TEMPORARY: Later in development we will not be assigning damage for timeouts.
        ApplyDamageToParty1();
        BeginFailScreen();
    }

    void Player1InputSuccess()
    {
        ApplyDamageToParty2();
        BeginSuccessScreen();
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

            ShowText(false, ButtonNameText);
            ShowText(false, PlayerHealthText);
            ShowText(false, BossHealthText);
            ShowText(false, FailText);
            ShowText(false, SuccessText);
            ShowText(false, YouLoseText);
            ShowText(false, YouWinText);

            BuffPanel.SetActive(false);
            BossPanel.SetActive(false);
            PlayerPanel.SetActive(false);
        }
        else if (_gameState == GameState.OptionsScreen)
        {
            BuffPanel.SetActive(true);

            ShowText(false, TitleText);
            ShowText(true, BuffText);
            UpdateBuffText();

            ShowText(false, GetReadyText);

            TimerPanel.SetActive(false);

            ShowText(false, ButtonNameText);
            ShowText(false, PlayerHealthText);
            ShowText(false, BossHealthText);
            ShowText(false, FailText);
            ShowText(false, SuccessText);
            ShowText(false, YouLoseText);
            ShowText(false, YouWinText);

            BuffPanel.SetActive(true);
            BossPanel.SetActive(false);
            PlayerPanel.SetActive(false);
        }
        else if (_gameState == GameState.PrepScreen)
        {
            BuffPanel.SetActive(false);

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

            ShowText(false, ButtonNameText);
            ShowText(false, FailText);
            ShowText(false, SuccessText);
            ShowText(false, YouLoseText);
            ShowText(false, YouWinText);

            BuffPanel.SetActive(false);
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
            ShowText(true, ButtonNameText);

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

            BuffPanel.SetActive(false);
            BossPanel.SetActive(true);
            PlayerPanel.SetActive(true);
        }
        else if (_gameState == GameState.FailScreen)
        {
            ShowText(false, TitleText);
            ShowText(false, BuffText);
            ShowText(false, GetReadyText);
            ShowText(false, ButtonNameText);
            ShowText(false, ButtonNameText);

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

            BuffPanel.SetActive(false);
            BossPanel.SetActive(true);
            PlayerPanel.SetActive(true);
        }
        else if (_gameState == GameState.SuccessScreen)
        {
            ShowText(false, TitleText);
            ShowText(false, BuffText);
            ShowText(false, GetReadyText);
            ShowText(false, ButtonNameText);
            ShowText(false, ButtonNameText);

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

            BuffPanel.SetActive(false);
            BossPanel.SetActive(true);
            PlayerPanel.SetActive(true);
        }
        else if (_gameState == GameState.DefeatScreen)
        {
            ShowText(false, TitleText);
            ShowText(false, BuffText);
            ShowText(false, GetReadyText);
            TimerPanel.SetActive(false);
            ShowText(false, ButtonNameText);
            ShowText(false, PlayerHealthText);
            ShowText(false, BossHealthText);
            ShowText(false, FailText);
            ShowText(false, SuccessText);
            ShowText(true, YouLoseText);
            ShowText(false, YouWinText);

            BuffPanel.SetActive(false);
            BossPanel.SetActive(false);
            PlayerPanel.SetActive(false);
        }
        else if (_gameState == GameState.VictoryScreen)
        {
            ShowText(false, TitleText);
            ShowText(false, BuffText);
            ShowText(false, GetReadyText);
            TimerPanel.SetActive(false);
            ShowText(false, ButtonNameText);
            ShowText(false, PlayerHealthText);
            ShowText(false, BossHealthText);
            ShowText(false, FailText);
            ShowText(false, SuccessText);
            ShowText(false, YouLoseText);
            ShowText(true, YouWinText);

            BuffPanel.SetActive(false);
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
        ButtonNameText.text = _currentButton.Name + " - " + _currentButton.Key.ToString();
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

    List<GameButton> GetGameButtonList()
    {
        List<GameButton> gbList = new List<GameButton>();

        gbList.Add(new GameButton(KeyCode.A, "Kill With Fire"));
        gbList.Add(new GameButton(KeyCode.B, "Run Like Hell"));
        gbList.Add(new GameButton(KeyCode.C, "360 No Scope"));
        gbList.Add(new GameButton(KeyCode.D, "Tank and Spank"));
        gbList.Add(new GameButton(KeyCode.E, "Spray and Pray"));
        gbList.Add(new GameButton(KeyCode.F, "Slay"));
        gbList.Add(new GameButton(KeyCode.G, "Geld"));
        gbList.Add(new GameButton(KeyCode.H, "Invite To Tea"));
        gbList.Add(new GameButton(KeyCode.I, "Remove Splinter"));
        gbList.Add(new GameButton(KeyCode.J, "Aggrevate Old Groin Injury"));

        gbList.Add(new GameButton(KeyCode.K, "Get Good"));
        gbList.Add(new GameButton(KeyCode.L, "Wreck Shop"));
        gbList.Add(new GameButton(KeyCode.M, "Invite Criticism"));
        gbList.Add(new GameButton(KeyCode.N, "Count Beans"));
        gbList.Add(new GameButton(KeyCode.O, "420Blazem"));
        gbList.Add(new GameButton(KeyCode.P, "Kappa Kappa Kappa"));
        gbList.Add(new GameButton(KeyCode.Q, "Eat Chips on Voice Chat"));
        gbList.Add(new GameButton(KeyCode.R, "Spill Soda on Keyboard"));
        gbList.Add(new GameButton(KeyCode.S, "Press Alt + F4"));
        gbList.Add(new GameButton(KeyCode.T, "Heal Through The Damage"));

        gbList.Add(new GameButton(KeyCode.U, "Rage Quietly"));
        gbList.Add(new GameButton(KeyCode.V, "Download DLC"));
        gbList.Add(new GameButton(KeyCode.W, "Cheat"));
        gbList.Add(new GameButton(KeyCode.X, "Kill the Messenger"));
        gbList.Add(new GameButton(KeyCode.Y, "Grab Pitchfork"));
        gbList.Add(new GameButton(KeyCode.Z, "Jump on the Bandwagon"));
        gbList.Add(new GameButton(KeyCode.LeftBracket, "Smoke (If you got em)"));
        gbList.Add(new GameButton(KeyCode.RightBracket, "Fire da Lazzzooor"));
        gbList.Add(new GameButton(KeyCode.LeftParen, "Gank Top"));
        gbList.Add(new GameButton(KeyCode.RightParen, "Roll Need"));

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
