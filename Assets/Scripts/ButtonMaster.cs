using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ButtonMaster : MonoBehaviour
{
    #region Properties

    private List<GameButton> Player1Buttons;
    private List<GameButton> Player2Buttons;
    private List<GameButton> Player3Buttons;
    private List<GameButton> Player4Buttons;

    private List<GameButton> Party1ActiveButtons;
    private List<GameButton> Party2ActiveButtons;

    private List<KeyCode> AllNumberKeys = new List<KeyCode>();

    private List<KeyCode> AllLetterKeys = new List<KeyCode>()
    {
        KeyCode.Semicolon,
        KeyCode.Colon,
        KeyCode.Quote,
        KeyCode.DoubleQuote,
        KeyCode.RightBracket,
        KeyCode.LeftBracket
    };

    //These can change based on settings and configuration that happens at runtime.
    private KeyCode Player1LetterKey = KeyCode.Semicolon;
    private KeyCode Player2LetterKey = KeyCode.Colon;
    private KeyCode Player3LetterKey = KeyCode.Quote;
    private KeyCode Player4LetterKey = KeyCode.DoubleQuote;

    private KeyCode Party1DummyLetterKey = KeyCode.RightBracket;
    private KeyCode Party2DummyLetterKey = KeyCode.LeftBracket;
    private KeyCode Party1DummyNumberKey = KeyCode.RightParen;
    private KeyCode Party2DummyNumberKey = KeyCode.LeftParen;

    private GameButton Party1DummyButton;
    private GameButton Party2DummyButton;
    private GameButton Party1CurrentButton;
    private GameButton Party1PreviousButton;
    private GameButton Party2CurrentButton;
    private GameButton Party2PreviousButton;

    private Dictionary<KeyCode, KeyCode> NumberToLetterKeyMapping = new Dictionary<KeyCode, KeyCode>();

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

        Player1Buttons = this.GetPlayer1Buttons();
        Player2Buttons = this.GetPlayer2Buttons();
        Player3Buttons = this.GetPlayer3Buttons();
        Player4Buttons = this.GetPlayer4Buttons();
        Party1DummyButton = this.GetParty1DummyButton();
        Party2DummyButton = this.GetParty2DummyButton();

        //The party buttons are the list of buttons we can use for the next random button press.
        Party1ActiveButtons = new List<GameButton>();
        Party2ActiveButtons = new List<GameButton>();
        if (bIsBossFight)
        {
            //Boss fight combines all the buttons.
            Party1ActiveButtons.AddRange(Player1Buttons);
            Party1ActiveButtons.AddRange(Player2Buttons);
            Party1ActiveButtons.AddRange(Player3Buttons);
            Party1ActiveButtons.AddRange(Player4Buttons);

            //Leave Party2ActiveButtons empty, because there is no party 2. Boss is AI!
        }
        else
        {
            //PVP
            //Player 1 and 2 on party 1
            Party1ActiveButtons.AddRange(Player1Buttons);
            Party1ActiveButtons.AddRange(Player2Buttons);

            //player 3 and 4 on party 2
            Party2ActiveButtons.AddRange(Player3Buttons);
            Party2ActiveButtons.AddRange(Player4Buttons);
        }

        AllNumberKeys = new List<KeyCode>();
        for (int i = 1; i <= 10; i++)
        {
            AllNumberKeys.Add(this.GetKeycode(
                JoystickAssignment.Joystick1,
                i
                ));
            AllNumberKeys.Add(this.GetKeycode(
                JoystickAssignment.Joystick2,
                i
                ));
            AllNumberKeys.Add(this.GetKeycode(
                JoystickAssignment.Joystick3,
                i
                ));
            AllNumberKeys.Add(this.GetKeycode(
                JoystickAssignment.Joystick4,
                i
                ));
        }

        NumberToLetterKeyMapping = new Dictionary<KeyCode, KeyCode>();

        foreach (GameButton gb in Player1Buttons) NumberToLetterKeyMapping[gb.NumberKey] = gb.LetterKey;
        foreach (GameButton gb in Player2Buttons) NumberToLetterKeyMapping[gb.NumberKey] = gb.LetterKey;
        foreach (GameButton gb in Player3Buttons) NumberToLetterKeyMapping[gb.NumberKey] = gb.LetterKey;
        foreach (GameButton gb in Player4Buttons) NumberToLetterKeyMapping[gb.NumberKey] = gb.LetterKey;
        NumberToLetterKeyMapping[Party1DummyButton.NumberKey] = Party1DummyButton.LetterKey;
        NumberToLetterKeyMapping[Party2DummyButton.NumberKey] = Party2DummyButton.LetterKey;
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
            while (((newButton.LetterKey == currentButton.LetterKey && newButton.NumberKey == currentButton.NumberKey)
                        || (previousButton != null && (newButton.LetterKey == previousButton.LetterKey && newButton.NumberKey == previousButton.NumberKey)))
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

    public KeyCode GetLetterKeyFromNumberKey(KeyCode numberKey)
    {
        if (NumberToLetterKeyMapping != null && NumberToLetterKeyMapping.ContainsKey(numberKey))
            return NumberToLetterKeyMapping[numberKey];
        else
            return KeyCode.None;
    }

    public GameButton GetCurrentParty1ActiveButton()
    {
        return Party1CurrentButton;
    }

    public GameButton GetCurrentParty2ActiveButton()
    {
        return Party2CurrentButton;
    }
    
    public List<KeyCode> GetAllNumberKeys()
    {
        return AllNumberKeys;
    }

    public List<KeyCode> GetAllLetterKeys()
    {
        return AllLetterKeys;
    }

    public bool IsCurrentButtonParty1(KeyCode letterKey, KeyCode numberKey)
    {
        return (Party1CurrentButton != null
            && Party1CurrentButton.LetterKey == letterKey
            && Party1CurrentButton.NumberKey == numberKey);
    }

    public bool IsCurrentButtonParty2(KeyCode letterKey, KeyCode numberKey)
    {
        return (Party2CurrentButton != null
            && Party2CurrentButton.LetterKey == letterKey
            && Party2CurrentButton.NumberKey == numberKey);
    }

    public bool IsLetterKeyParty1(KeyCode letterKey)
    {
        return ((letterKey == Player1LetterKey) || (letterKey == Player2LetterKey) || (letterKey == Party1DummyLetterKey));
    }

    public bool IsLetterKeyParty2(KeyCode letterKey)
    {
        return ((letterKey == Player3LetterKey) || (letterKey == Player4LetterKey) || (letterKey == Party2DummyLetterKey));
    }

    public List<GameButton> BuildButtonList(KeyCode letterKey, JoystickAssignment ja, List<string> keyOrder)
    {
        List<GameButton> gbList = new List<GameButton>();

        for (int i = 0; i < keyOrder.Count; i++)
        {
            gbList.Add(new GameButton(
                letterKey,
                GetKeycode(ja, i + 1),
                keyOrder[i]
                ));
        }

        return gbList;
    }

    //Argue over spelling


    //When AutoInputDualActionMode is true then we simulate the Button Letter Key input during the active screen.
    
    public KeyCode GetKeycode(JoystickAssignment ja, int buttonNumber)
    {
        if (ja == JoystickAssignment.Joystick1)
        {
            switch (buttonNumber)
            {
                case 1: return KeyCode.A;
                case 2: return KeyCode.B;
                case 3: return KeyCode.C;
                case 4: return KeyCode.D;
                case 5: return KeyCode.E;
                case 6: return KeyCode.F;
                case 7: return KeyCode.G;
                case 8: return KeyCode.H;
                case 9: return KeyCode.I;
                case 10: return KeyCode.J;
            }
        }
        else if (ja == JoystickAssignment.Joystick2)
        {
            switch (buttonNumber)
            {
                case 1: return KeyCode.K;
                case 2: return KeyCode.L;
                case 3: return KeyCode.M;
                case 4: return KeyCode.N;
                case 5: return KeyCode.O;
                case 6: return KeyCode.P;
                case 7: return KeyCode.Q;
                case 8: return KeyCode.R;
                case 9: return KeyCode.S;
                case 10: return KeyCode.T;
            }
        }
        else if (ja == JoystickAssignment.Joystick3)
        {
            switch (buttonNumber)
            {
                case 1: return KeyCode.U;
                case 2: return KeyCode.V;
                case 3: return KeyCode.W;
                case 4: return KeyCode.X;
                case 5: return KeyCode.Y;
                case 6: return KeyCode.Z;
                case 7: return KeyCode.Comma;
                case 8: return KeyCode.Period;
                case 9: return KeyCode.Slash;
                case 10: return KeyCode.Backslash;
            }
        }
        else if (ja == JoystickAssignment.Joystick4)
        {
            switch (buttonNumber)
            {
                case 1: return KeyCode.Alpha1;
                case 2: return KeyCode.Alpha2;
                case 3: return KeyCode.Alpha3;
                case 4: return KeyCode.Alpha4;
                case 5: return KeyCode.Alpha5;
                case 6: return KeyCode.Alpha6;
                case 7: return KeyCode.Alpha7;
                case 8: return KeyCode.Alpha8;
                case 9: return KeyCode.Alpha9;
                case 10: return KeyCode.Alpha0;
            }
        }
        return KeyCode.None;
    }

    private JoystickAssignment Player1JoystickAssignment = JoystickAssignment.Joystick4;
    public List<GameButton> GetPlayer1Buttons()
    {
        return BuildButtonList(
            Player1LetterKey,
            Player1JoystickAssignment,
            new List<string>()
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
        });
    }

    public bool IsResetKey()
    {
        return Input.GetKeyDown(KeyCode.Minus);
    }

    public bool IsStartKey()
    {
        return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
    }

    private JoystickAssignment Player2JoystickAssignment = JoystickAssignment.Joystick3;
    public List<GameButton> GetPlayer2Buttons()
    {
        return BuildButtonList(
            Player2LetterKey,
            Player2JoystickAssignment,
            new List<string>()
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
        });
    }

    private JoystickAssignment Player3JoystickAssignment = JoystickAssignment.Joystick1;
    public List<GameButton> GetPlayer3Buttons()
    {
        return BuildButtonList(
            Player3LetterKey,
            Player3JoystickAssignment,
            new List<string>()
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
        });
    }

    private JoystickAssignment Player4JoystickAssignment = JoystickAssignment.Joystick2;
    public List<GameButton> GetPlayer4Buttons()
    {
        return BuildButtonList(
            Player4LetterKey,
            Player4JoystickAssignment,
            new List<string>()
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
            });
    }

    public GameButton GetParty1DummyButton()
    {
        return new GameButton(Party1DummyLetterKey, Party1DummyNumberKey, "Party 1 Dummy");
    }

    public GameButton GetParty2DummyButton()
    {
        return new GameButton(Party2DummyLetterKey, Party2DummyNumberKey, "Party 2 Dummy");
    }

    public List<GameButton> GetParty1Buttons()
    {
        List<GameButton> combinedList = new List<GameButton>();

        combinedList.Add(GetParty1DummyButton());
        combinedList.AddRange(GetPlayer1Buttons());
        combinedList.AddRange(GetPlayer2Buttons());

        return combinedList;
    }

    public List<GameButton> GetParty2Buttons()
    {
        List<GameButton> combinedList = new List<GameButton>();

        combinedList.Add(GetParty2DummyButton());
        combinedList.AddRange(GetPlayer3Buttons());
        combinedList.AddRange(GetPlayer4Buttons());

        return combinedList;
    }
}


public class GameButton
{
    public string Name;
    public KeyCode LetterKey;
    public KeyCode NumberKey;

    public GameButton(KeyCode letterKey, KeyCode numberKey, string sName)
    {
        this.LetterKey = letterKey;
        this.NumberKey = numberKey;
        this.Name = sName;
    }
}

public enum JoystickAssignment
{
    Joystick1,
    Joystick2,
    Joystick3,
    Joystick4
}