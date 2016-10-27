using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ButtonMaster
{
    #region Properties
    
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
    
    private JoystickAssignment Party1PositonAJoystickAssignment = JoystickAssignment.Joystick2;
    private JoystickAssignment Party1PositonBJoystickAssignment = JoystickAssignment.Joystick4;
    private JoystickAssignment Party1PositonCJoystickAssignment = JoystickAssignment.Joystick3;
    private JoystickAssignment Party1PositonDJoystickAssignment = JoystickAssignment.Joystick1;

    //private JoystickAssignment Party2PositonAJoystickAssignment = JoystickAssignment.Joystick5;
    //private JoystickAssignment Party2PositonBJoystickAssignment = JoystickAssignment.Joystick6;
    //private JoystickAssignment Party2PositonCJoystickAssignment = JoystickAssignment.Joystick7;
    //private JoystickAssignment Party2PositonDJoystickAssignment = JoystickAssignment.Joystick8;

    #endregion

    #region Constructors/Initializers

    public ButtonMaster()
    {

    }

    public ButtonMaster(bool bIsBossFight)
    {
        SetupButtons(bIsBossFight);
    }

    public void SetupButtons(bool bIsBossFight)
    {
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
            Party1ActiveButtons.AddRange(GetParty1Buttons());
            Party1ActiveButtons.AddRange(GetParty2Buttons());

            //Leave Party2ActiveButtons empty, because there is no party 2. Boss is AI!
        }
        else
        {
            //PVP
            //Player 1 and 2 on party 1
            Party1ActiveButtons.AddRange(GetParty1Buttons());

            //player 3 and 4 on party 2
            Party2ActiveButtons.AddRange(GetParty2Buttons());
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

    public List<GameButton> GetParty1Buttons()
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
        /*buttonList.AddRange(BuildButtonList(
            BoardPositon.C,
            Party1PositonCJoystickAssignment
            ));

        //Add position D
        buttonList.AddRange(BuildButtonList(
            BoardPositon.D,
            Party1PositonDJoystickAssignment
            ));*/

        return buttonList;
    }

    public List<GameButton> GetParty2Buttons()
    {
        //Add position A
        List<GameButton> buttonList = BuildButtonList(
            BoardPositon.C,
            Party1PositonCJoystickAssignment
            );

        //Add position B
        buttonList.AddRange(BuildButtonList(
            BoardPositon.D,
            Party1PositonDJoystickAssignment
            ));
            
        //Add position C
        /*buttonList.AddRange(BuildButtonList(
            BoardPositon.C,
            Party2PositonCJoystickAssignment
            ));

        //Add position D
        buttonList.AddRange(BuildButtonList(
            BoardPositon.D,
            Party2PositonDJoystickAssignment
            ));*/

        return buttonList;
    }

    #endregion

    public GameButton SelectNewButtonForParty1()
    {
        GameButton gb = GetRandomButton(Party1ActiveButtons, Party1CurrentButton, Party1PreviousButton);

        Party1PreviousButton = Party1CurrentButton;
        Party1CurrentButton = gb;

        return gb;
    }

    public GameButton SelectNewButtonForParty2()
    {
        GameButton gb = GetRandomButton(Party2ActiveButtons, Party2CurrentButton, Party2PreviousButton);

        Party2PreviousButton = Party2CurrentButton;
        Party2CurrentButton = gb;

        return gb;
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
        return (Party1CurrentButton != null
            && Party1CurrentButton.NumberKey == numberKey);
    }

    public bool IsCurrentButtonParty2(KeyCode numberKey)
    {
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
    
    public enum BoardPositon
    {
        A,
        B,
        C,
        D
    }

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

    //Argue over spelling


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
                        KeyCode.Keypad0,
                        KeyCode.Keypad1,
                        KeyCode.Keypad2,
                        KeyCode.Keypad3,
                        KeyCode.Keypad4,
                        KeyCode.Keypad5,
                        KeyCode.Keypad6,
                        KeyCode.Keypad7,
                        KeyCode.Keypad8,
                        KeyCode.Keypad9,
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
                        KeyCode.LeftShift,
                        KeyCode.RightShift,
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
                        KeyCode.Underscore,
                        KeyCode.Backslash,
                        KeyCode.Backspace,
                        KeyCode.Tab,
                        KeyCode.KeypadMultiply,
                };
            default:
                throw new System.Exception("No joystick that high.");
        }
    }

    public static bool IsQuitKey()
    {
        return Input.GetKeyDown(KeyCode.Escape);
    }

    public static bool IsResetKey()
    {
        return Input.GetKeyDown(KeyCode.Minus);
    }

    public static bool IsStartKey()
    {
        return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
    }

    List<string> GetButtonNameListByBoardPosition(BoardPositon boardPositon)
    {
        switch (boardPositon)
        {
            case BoardPositon.D:
                return new List<string>()
                {
                    { "Flying Groin Stomp" },
                    { "Aggrevate Old Tap-Dancing Injury" },
                    { "Spray and Pray" },
                    { "Tank and Spank" },
                    { "Falcon Punch!" },
                    { "Overcook the Roast" },
                    { "Put Gum In Hair" },
                    { "360 No Scope" },
                    { "Turn Off and Back On" },
                    { "Kill With Fire" },
                };
            case BoardPositon.C:
                return new List<string>()
                {
                    { "Scratch Their Bieber CDs" },
                    { "Press Alt + F4" },
                    { "Wet Willy" },
                    { "Don't Send Xmas Card" },
                    { "Don't Touch Anything" },
                    { "Fap Quietly to Food Network" },
                    { "Run in Circles" },
                    { "Kill with Kindness" },
                    { "Tiger's Claw Grasps the Pearls" },
                    { "Flail Wildly" },
                };
            case BoardPositon.B:
                return new List<string>()
                {
                    { "Execute" },
                    { "Eye Gouge" },
                    { "Long Distance Expectoration" }, //Especially Good Expectoration
                    { "Fake High Five" },
                    { "Release the Kraken" },
                    { "Sweep the Leg" },
                    { "Camp Spawn" },
                    { "420 Blaze 'Em" },
                    { "Shit Self" },
                    { "Spin to Win" },
                };
            case BoardPositon.A:
                return new List<string>()
                {
                    { "Grab Pitchfork!" },
                    { "Call Tech Support" },
                    { "Fire da Lazzzoorrr!!!" },
                    { "Smoke (If you got 'em)" },
                    { "Cheat" },
                    { "Rage Silently" },
                    { "Kill the Messenger" },
                    { "Spinning Neck Chop" },
                    { "Farm Jungle" },
                    { "Disenchant Legendary" },
                };
            default:
                throw new System.Exception("Invalid board positon.");
        }
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

public enum JoystickAssignment
{
    Joystick1,
    Joystick2,
    Joystick3,
    Joystick4,
    Joystick5,
    Joystick6,
    Joystick7,
    Joystick8
}