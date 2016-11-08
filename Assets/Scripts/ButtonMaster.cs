using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ButtonMaster
{
    #region Properties


    private bool _testMode = false;
    private int _previousIndex = -1;

    private bool _bDuplicatedButtons = true;
    private bool _bFullSpread = true;

    private List<GameButton> Party1ActiveButtons;
    private List<GameButton> Party2ActiveButtons;

    private Dictionary<KeyCode, GameButton> Party1Dictionary;
    private Dictionary<KeyCode, GameButton> Party2Dictionary;

    private List<KeyCode> AllActiveKeys = new List<KeyCode>();
    
    //These can change based on settings and configuration that happens at runtime.
    private GameButton Party1CurrentButton;
    private GameButton Party1PreviousButton;
    private GameButton Party2CurrentButton;
    private GameButton Party2PreviousButton;

    //private Dictionary<KeyCode, KeyCode> NumberToLetterKeyMapping = new Dictionary<KeyCode, KeyCode>();
    
    private JoystickAssignment Party1PositonAJoystickAssignment = JoystickAssignment.Joystick1;
    private JoystickAssignment Party1PositonBJoystickAssignment = JoystickAssignment.Joystick2;
    private JoystickAssignment Party1PositonCJoystickAssignment = JoystickAssignment.Joystick3;
    private JoystickAssignment Party1PositonDJoystickAssignment = JoystickAssignment.Joystick4;

    private JoystickAssignment Party2PositonAJoystickAssignment = JoystickAssignment.Joystick5;
    private JoystickAssignment Party2PositonBJoystickAssignment = JoystickAssignment.Joystick6;
    private JoystickAssignment Party2PositonCJoystickAssignment = JoystickAssignment.Joystick7;
    private JoystickAssignment Party2PositonDJoystickAssignment = JoystickAssignment.Joystick8;

    private JoystickAssignment AdminPositionJoystickAssignment = JoystickAssignment.Joystick9;

    //private JoystickAssignment AccessiblePositionAJoystickAssignment = JoystickAssignment.Joystick10;
    //private JoystickAssignment AccessiblePositionBJoystickAssignment = JoystickAssignment.Joystick11;

    

    #endregion

    #region Constructors/Initializers

    public ButtonMaster(bool bIsBossFight, JoystickMapping joystickMapping, bool bDuplicatedButtons, bool bFullSpread)
    {
        _bDuplicatedButtons = bDuplicatedButtons;
        _bFullSpread = bFullSpread;
        
        this.Party1PositonAJoystickAssignment = (JoystickAssignment)joystickMapping.BlueA;
        this.Party1PositonBJoystickAssignment = (JoystickAssignment)joystickMapping.BlueB;
        this.Party1PositonCJoystickAssignment = (JoystickAssignment)joystickMapping.BlueC;
        this.Party1PositonDJoystickAssignment = (JoystickAssignment)joystickMapping.BlueD;

        this.Party2PositonAJoystickAssignment = (JoystickAssignment)joystickMapping.GreenA;
        this.Party2PositonBJoystickAssignment = (JoystickAssignment)joystickMapping.GreenB;
        this.Party2PositonCJoystickAssignment = (JoystickAssignment)joystickMapping.GreenC;
        this.Party2PositonDJoystickAssignment = (JoystickAssignment)joystickMapping.GreenD;

        this.AdminPositionJoystickAssignment = (JoystickAssignment)joystickMapping.Admin;

        this._testMode = joystickMapping.TestMode;
        this._bFullSpread = joystickMapping.FullSpread;

        SetupButtons(bIsBossFight);
    }

    public void SetupButtons(bool bIsBossFight)
    {
        AssignAdminButtons();

        Party1CurrentButton = null;
        Party1PreviousButton = null;
        Party2CurrentButton = null;
        Party2PreviousButton = null;

        //The party buttons are the list of buttons we can use for the next random button press.
        Party1ActiveButtons = new List<GameButton>();
        Party2ActiveButtons = new List<GameButton>();
        if (bIsBossFight)
        {
            //Boss fight combines all the buttons.
            Party1ActiveButtons.AddRange(GetParty1Buttons(_bFullSpread));
            Party1ActiveButtons.AddRange(GetParty2Buttons(_bFullSpread));

            //Leave Party2ActiveButtons empty, because there is no party 2. Boss is AI!
        }
        else
        {
            //PVP
            //Player 1 and 2 on party 1
            Party1ActiveButtons.AddRange(GetParty1Buttons(_bFullSpread));

            //player 3 and 4 on party 2
            Party2ActiveButtons.AddRange(GetParty2Buttons(_bFullSpread));
        }

        AllActiveKeys = new List<KeyCode>();
        Party1Dictionary = new Dictionary<KeyCode, GameButton>();
        Party2Dictionary = new Dictionary<KeyCode, GameButton>();

        foreach (GameButton gameButton in Party1ActiveButtons)
        {
            AllActiveKeys.Add(gameButton.NumberKey);

            if (Party1Dictionary.ContainsKey(gameButton.NumberKey))
                throw new System.Exception("Party 1 Duplicate key code found: " + gameButton.NumberKey + ", " + gameButton.Name);
            else
                Party1Dictionary[gameButton.NumberKey] = gameButton;
        }
        foreach (GameButton gameButton in Party2ActiveButtons)
        {
            AllActiveKeys.Add(gameButton.NumberKey);

            if (Party2Dictionary.ContainsKey(gameButton.NumberKey))
                throw new System.Exception("Party 2 Duplicate key code found: " + gameButton.NumberKey + ", " + gameButton.Name);
            else
                Party2Dictionary[gameButton.NumberKey] = gameButton;
        }
    }

    public void AssignAdminButtons()
    {
        List<KeyCode> keyCodeList = GetKeyCodeListByJoystickAssignment(AdminPositionJoystickAssignment);

        _startKey = keyCodeList[1];
        _resetKey = keyCodeList[0];
    }

    private KeyCode _resetKey;
    private KeyCode _startKey;

    public bool IsResetKey()
    {
        return Input.GetKeyDown(_resetKey);
    }

    public bool IsStartKey()
    {
        return Input.GetKeyDown(_startKey);
    }

    public List<GameButton> GetParty1Buttons(bool bFullSpread)
    {
        if (bFullSpread)
        {
            //Add position A
            List<GameButton> buttonList = BuildButtonList(
                BoardPositon.A,
                Party1PositonAJoystickAssignment
                );

            //Add position B
            buttonList.AddRange(BuildButtonList(
                BoardPositon.B,
                Party1PositonBJoystickAssignment
                ));

            //Add position C
            buttonList.AddRange(BuildButtonList(
                BoardPositon.C,
                Party1PositonCJoystickAssignment
                ));

            //Add position D
            buttonList.AddRange(BuildButtonList(
                BoardPositon.D,
                Party1PositonDJoystickAssignment
                ));

            return buttonList;
        }
        else
        {
            List<GameButton> buttonList = BuildButtonList(
                BoardPositon.B,
                Party1PositonBJoystickAssignment
                );
            return buttonList;
        }
    }

    public List<GameButton> GetParty2Buttons(bool bFullSpread)
    {
        if (bFullSpread)
        {
            //Add position A
            List<GameButton> buttonList = BuildButtonList(
                BoardPositon.A,
                Party2PositonAJoystickAssignment
                );

            //Add position B
            buttonList.AddRange(BuildButtonList(
                BoardPositon.B,
                Party2PositonBJoystickAssignment
                ));

            //Add position C
            buttonList.AddRange(BuildButtonList(
                BoardPositon.C,
                Party2PositonCJoystickAssignment
                ));

            //Add position D
            buttonList.AddRange(BuildButtonList(
                BoardPositon.D,
                Party2PositonDJoystickAssignment
                ));

            return buttonList;
        }
        else
        {
            List<GameButton> buttonList = BuildButtonList(
                BoardPositon.B,
                Party2PositonBJoystickAssignment
                );
            return buttonList;
        }
    }

    #endregion

    #region Button List Building

    public List<GameButton> BuildButtonList(BoardPositon boardPositon, JoystickAssignment joystickAssignment)
    {
        List<GameButton> gbList = new List<GameButton>();

        List<KeyCode> keyCodeList = GetKeyCodeListByJoystickAssignment(joystickAssignment);
        List<string> buttonNameList = GetButtonNameListByBoardPosition(boardPositon);

        //Button names must be provided in the exact order they appear on the panel.
        for (int i = 0; i < buttonNameList.Count; i++)
        {
            gbList.Add(new GameButton(
               keyCodeList[i],
                buttonNameList[i]
                ));
        }

        return gbList;
    }

    //When AutoInputDualActionMode is true then we simulate the Button Letter Key input during the active screen.
    List<KeyCode> GetKeyCodeListByJoystickAssignment(JoystickAssignment ja)
    {
        switch (ja)
        {
            case JoystickAssignment.Joystick1:
                return new List<KeyCode>()
                {
                        KeyCode.A,
                        KeyCode.B,
                        KeyCode.C,
                        KeyCode.D,
                        KeyCode.E,
                        KeyCode.F,
                        KeyCode.G,
                        KeyCode.H,
                        KeyCode.I,
                        KeyCode.J,
                };
            case JoystickAssignment.Joystick2:
                return new List<KeyCode>()
                {
                        KeyCode.K,
                        KeyCode.L,
                        KeyCode.M,
                        KeyCode.N,
                        KeyCode.O,
                        KeyCode.P,
                        KeyCode.Q,
                        KeyCode.R,
                        KeyCode.S,
                        KeyCode.T,
                };
            case JoystickAssignment.Joystick3:
                return new List<KeyCode>()
                {
                        KeyCode.U,
                        KeyCode.V,
                        KeyCode.W,
                        KeyCode.X,
                        KeyCode.Y,
                        KeyCode.Z,
                        KeyCode.Comma,
                        KeyCode.Period,
                        KeyCode.Semicolon,
                        KeyCode.Quote,
                };
            case JoystickAssignment.Joystick4:
                return new List<KeyCode>()
                {
                        KeyCode.Alpha1,
                        KeyCode.Alpha2,
                        KeyCode.Alpha3,
                        KeyCode.Alpha4,
                        KeyCode.Alpha5,
                        KeyCode.Alpha6,
                        KeyCode.Alpha7,
                        KeyCode.Alpha8,
                        KeyCode.Alpha9,
                        KeyCode.Alpha0,
                };
            case JoystickAssignment.Joystick5:
                return new List<KeyCode>()
                {
                        KeyCode.Keypad1,
                        KeyCode.Keypad2,
                        KeyCode.Keypad3,
                        KeyCode.Keypad4,
                        KeyCode.Keypad5,
                        KeyCode.Keypad6,
                        KeyCode.Keypad7,
                        KeyCode.Keypad8,
                        KeyCode.Keypad9,
                        KeyCode.Keypad0,
                };
            case JoystickAssignment.Joystick6:
                return new List<KeyCode>()
                {
                        KeyCode.F1,
                        KeyCode.F2,
                        KeyCode.F3,
                        KeyCode.F4,
                        KeyCode.F5,
                        KeyCode.F6,
                        KeyCode.F7,
                        KeyCode.F8,
                        KeyCode.F9,
                        KeyCode.F10,
                };
            case JoystickAssignment.Joystick7:
                return new List<KeyCode>()
                {
                        KeyCode.F11,
                        KeyCode.F12,
                        KeyCode.F13,
                        KeyCode.F14,
                        KeyCode.F15,
                        KeyCode.LeftBracket,
                        KeyCode.RightBracket,
                        KeyCode.KeypadMinus,
                        KeyCode.KeypadPlus,
                        KeyCode.Equals,
                };
            case JoystickAssignment.Joystick8:
                return new List<KeyCode>()
                {
                        KeyCode.Space,
                        KeyCode.PageUp,
                        KeyCode.PageDown,
                        KeyCode.End,
                        KeyCode.Home,
                        KeyCode.KeypadDivide,
                        KeyCode.Backslash,
                        KeyCode.Backspace,
                        KeyCode.Tab,
                        KeyCode.KeypadMultiply,
                };
            case JoystickAssignment.Joystick9:
                return new List<KeyCode>()
                {
                        KeyCode.LeftShift,
                        KeyCode.RightShift,
                        KeyCode.LeftControl,
                        KeyCode.Numlock,
                        KeyCode.LeftAlt,
                        KeyCode.ScrollLock,
                        KeyCode.KeypadPeriod,
                        KeyCode.Insert,
                        KeyCode.Delete,
                        KeyCode.BackQuote,
                };
            default:
                throw new System.Exception("No joystick that high.");
        }
    }

    List<string> GetButtonNameListByBoardPosition(BoardPositon boardPositon)
    {
        switch (boardPositon)
        {
            case BoardPositon.A:
                return new List<string>()
                {
                    { "Play an Advertisement" },
                    { "Holy Word: Annoy" },
                    { "Flying Groin Stomp" },
                    { "Cartwheeling Pile Driver" },
                    { "Thrusting Hip Strike" },

                    { "360 No Scope" },
                    { "420 Blaze 'Em" },
                    { "Spinning Neck Chop" },
                    { "Spinning Meat Cleaver" },
                    { "Splintering Mace Clout" },
                };
            case BoardPositon.B:
                return new List<string>()
                {
                    { "Vicious Chest Poke" },
                    { "Bedazzling Blade" },
                    { "Bewitching Blast" },
                    { "Befuddling Backhand" },
                    { "Liger's Paw Clasps the Wrist" },

                    { "Activate Hax" },
                    { "Super Intense Stare" },
                    { "Super Insensitive Slander" },
                    { "Tiger's Claw Grasps the Pearls" },
                    { "Viper's Fang Stabs the Kiwis" }
                };
            case BoardPositon.C:
                return new List<string>()
                {
                    { "Long Distance Expectoration" }, //Especially Good Expectoration
                    { "Short Distance Assassination" }, //Especially Good Expectoration
                    { "Fake High Five" },
                    { "Take Their Lives" },
                    { "Poetry Choke Slam" },

                    { "Sweep the Leg" },
                    { "Sweep the Floor" },
                    { "Spray and Pray" },
                    { "Tank and Spank" },
                    { "Spin to Win" },
                };
            case BoardPositon.D:
                return new List<string>()
                {
                    { "Blasphemous Belch" },
                    { "Pulverizing Punch" },
                    { "Taunt (A Second Time)" },
                    { "Inflict Flesh Wound" },

                    { "Rage Silently" },
                    { "Flail Wildly" },
                    { "Get Comfortable" },

                    { "Mildly Magic Missile" },
                    { "Mostly Magic Missile" },
                    { "Markedly Magic Missile" },

                };
            case BoardPositon.Admin:
                return new List<string>()
                {
                    { "Enter" },
                    { "Reset" },
                    { "None1" },
                    { "None2" },
                    { "None3" },
                    { "None4" },
                    { "None5" },
                    { "None6" },
                    { "None7" },
                    { "None8" }
                };
            default:
                throw new System.Exception("Invalid board positon.");
        }
    }

    #endregion

    #region Select New Active Buttons

    public GameButtonGroup SelectNewButtons()
    {
        GameButton team1Button = SelectNewButtonForParty1();
        GameButton team2Button;
        if (_bDuplicatedButtons)
        {
            //If we're duplicating buttons
            team2Button = SelectNewButtonForParty2(team1Button);
        }
        else
        {
            team2Button = SelectNewButtonForParty2();
        }

        return new GameButtonGroup(team1Button, team2Button);
    }

    private GameButton SelectNewButtonForParty1()
    {
        GameButton gb = GetRandomButton(Party1ActiveButtons, Party1CurrentButton, Party1PreviousButton);

        Party1PreviousButton = Party1CurrentButton;
        Party1CurrentButton = gb;

        return gb;
    }

    private GameButton SelectNewButtonForParty2()
    {
        GameButton gb = GetRandomButton(Party2ActiveButtons, Party2CurrentButton, Party2PreviousButton);

        Party2PreviousButton = Party2CurrentButton;
        Party2CurrentButton = gb;

        return gb;
    }

    private GameButton SelectNewButtonForParty2(GameButton team1Button)
    {
        GameButton gb = GetMatchingButton(Party2ActiveButtons, team1Button);

        Party2PreviousButton = Party2CurrentButton;
        Party2CurrentButton = gb;

        return gb;
    }

    private GameButton GetMatchingButton(List<GameButton> gameButtons, GameButton matchingButton)
    {
        int wButtonCount = gameButtons.Count;
        if (matchingButton == null)
        {
            //Can't match a null button. Just return a random.
            return gameButtons[Random.Range(0, wButtonCount)];
        }
        else
        {
            //Find a matching button in the list of active buttons.
            foreach (GameButton gameButton in gameButtons)
            {
                if (gameButton.Name == matchingButton.Name)
                {
                    return gameButton;
                }
            }

            //NewButton should not be the same as current button, so replace it.
            throw new System.Exception("Could not find matching button.");
        }
    }
    public GameButton GetRandomButton(List<GameButton> gameButtons, GameButton currentButton, GameButton previousButton)
    {
        int wButtonCount = gameButtons.Count;
        if (currentButton == null)
        {
            //The first button will be truly random because we don't have to worry about copying a previous button.

            if (!_testMode)
            {
                return gameButtons[Random.Range(0, wButtonCount)];
            }
            else
            {
                int index = 0;
                _previousIndex = index;
                return gameButtons[index];
            }
        }
        else
        {
            //If we've already got a current button then we need to randomly select a new one... but not the same one.
            GameButton newButton = currentButton;


            if (!_testMode)
            {
                int wCount = 0;
                while (((newButton.NumberKey == currentButton.NumberKey)
                            || (previousButton != null && (newButton.NumberKey == previousButton.NumberKey)))
                        && wCount < (wButtonCount * 2))
                {
                    int wRandom = Random.Range(0, wButtonCount);
                    //Keep cycling until we get a different random key than the one 
                    newButton = gameButtons[wRandom];

                    wCount++;
                }
                //NewButton should not be the same as current button, so replace it.
                return newButton;
            }
            else
            {

                _previousIndex++;
                if (_previousIndex >= wButtonCount)
                    _previousIndex = 0;

                return gameButtons[_previousIndex];

            }
        }
    }

    #endregion

    #region Check Buttons

    public static bool IsQuitKey()
    {
        return Input.GetKeyDown(KeyCode.Escape);
    }
    
    public GameButton GetCurrentParty1ActiveButton()
    {
        return Party1CurrentButton;
    }

    public GameButton GetCurrentParty2ActiveButton()
    {
        return Party2CurrentButton;
    }
    
    public List<KeyCode> GetAllActiveKeys()
    {
        return AllActiveKeys;
    }

    public bool IsCurrentButtonParty1(KeyCode numberKey)
    {
        //Debug.Log("Current Party 2 button: " + Party1CurrentButton.NumberKey + " ... " + Party1CurrentButton.Name);
        return (Party1CurrentButton != null
            && Party1CurrentButton.NumberKey == numberKey);
    }

    public bool IsCurrentButtonParty2(KeyCode numberKey)
    {
        //Debug.Log("Current Party 2 button: " + Party2CurrentButton.NumberKey + " ... " + Party2CurrentButton.Name);
        return (Party2CurrentButton != null
            && Party2CurrentButton.NumberKey == numberKey);
    }

    public bool IsKeyParty1(KeyCode key)
    {
        return Party1Dictionary.ContainsKey(key);
    }

    public bool IsKeyParty2(KeyCode key)
    {
        return Party2Dictionary.ContainsKey(key);
    }

    #endregion
}

public class GameButtonGroup
{
    public GameButton Team1Button = null;
    public GameButton Team2Button = null;

    public GameButtonGroup(GameButton team1Button)
        : this(team1Button, null)
    {
    }

    public GameButtonGroup(GameButton team1Button, GameButton team2Button)
    {
        Team1Button = team1Button;
        Team2Button = team2Button;
    }
}

public class GameButton
{
    public string Name;
    //public KeyCode LetterKey;
    public KeyCode NumberKey;

    public GameButton(KeyCode numberKey, string sName)
    {
        //this.LetterKey = letterKey;
        this.NumberKey = numberKey;
        this.Name = sName;
    }
}

public enum JoystickAssignment : int
{
    Joystick1 = 1,
    Joystick2 = 2,
    Joystick3 = 3,
    Joystick4 = 4,
    Joystick5 = 5,
    Joystick6 = 6,
    Joystick7 = 7,
    Joystick8 = 8,
    Joystick9 = 9,
}

public enum BoardPositon
{
    A,
    B,
    C,
    D,
    Admin
}
