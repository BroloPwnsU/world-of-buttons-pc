using UnityEngine;
using System.Collections;

public class BuffPanel : MonoBehaviour {

    public AudioClip ChangeValueAudioClip;
    public AudioClip ContinueAudioClip;
    private AudioSource _audioSource;

	// Use this for initialization
	void Awake ()
    {
        _audioSource = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            //Move Up in menu.
            MoveUp();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            //Move Down in menu.
            MoveDown();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            //Move Left in menu.
            MoveLeft();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            //Move Right in menu.
            MoveRight();
        }
        else if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        {
            //Pressed enter. Only works when hovering on the Continue button.
            //Continue();
        }
    }

    public void StartBuffPanel(BuffPanelSettings settings)
    {
    }

    private BuffMenu[] _menus;
    private int _menuIndex = 0;

    public void MoveUp()
    {
        SelectMenu(_menuIndex - 1);
    }

    public void MoveDown()
    {
        SelectMenu(_menuIndex + 1);
    }

    private void SelectMenu(int newIndex)
    {
        if (newIndex < 0)
            _menuIndex = 0;
        else if (newIndex >= _menus.Length)
            _menuIndex = _menus.Length - 1;
        else
            _menuIndex = newIndex;
    }

    public void MoveLeft()
    {
        _menus[_menuIndex].PreviousOption();
    }

    public void MoveRight()
    {
        _menus[_menuIndex].NextOption();
    }

    class BuffMenu
    {
        public string[] MenuOptions;
        public string Title;
        public int OptionIndex = 0;

        public BuffMenu(string sTitle, string[] optionParams)
            : this(sTitle, optionParams, null)
        {
        }

        public BuffMenu(string sTitle, string[] optionParams, string sDefaultValue)
        {
            //Set the buff menu title.
            Title = sTitle;
            OptionIndex = 0;

            //Copy the params into our own array of options.
            if (optionParams != null && optionParams.Length > 0)
            {
                MenuOptions = (string[])optionParams.Clone();
            }
            else
            {
                //If the option menu is empty just show a default value so the code does't break.
                MenuOptions = new string[1];
                MenuOptions[0] = "Default";
            }

            OptionIndex = 0;
            if (!string.IsNullOrEmpty(sDefaultValue))
            {
                for (int i = 0; i < MenuOptions.Length; i++)
                {
                    if (MenuOptions[i] == sDefaultValue)
                    {
                        OptionIndex = i;
                        break;
                    }
                }
            }
        }

        public bool NextOption()
        {
            return ChangeOption(OptionIndex + 1);
        }

        public bool PreviousOption()
        {
            return ChangeOption(OptionIndex - 1);
        }

        public bool ChangeOption(int wNext)
        {
            if (wNext >= MenuOptions.Length)
            {
                //Don't do nothing! We've gone too far! Tell them we can't pull over any farther!
                return false;
            }
            else if (wNext < 0)
            {
                //Don't do nothing! We've gone too far! Tell them we can't pull over any farther!
                return false;
            }
            else
            {
                OptionIndex = wNext;
                return true;
            }
        }
    }
}

public class BuffPanelSettings
{

}
