﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class OptionsPanel : GamePanel
{

    public AudioClip ChangeValueAudioClip;
    public AudioClip ContinueAudioClip;
    public AudioClip ChangeValueFailAudioClip;
    private AudioSource _audioSource;

    public Text OptionsHeaderText;
    public GameObject _menuSelectorPrefab;

    private bool _panelActive = false;
    private ButtonMaster _buttonMaster;

    private System.Action EndOptionsScreenNotification;

    // Use this for initialization
    void Awake ()
    {
        _panelActive = false;
        _audioSource = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (_panelActive)
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
            else if (_buttonMaster.IsStartKey())
            {
                if (gameObject.activeSelf)
                    //Pressed enter. Only works when hovering on the Continue button.
                    Continue();
            }
        }
    }

    void Continue()
    {
        _panelActive = false;

        PlayContinueSound();
        foreach (OptionMenu om in _menus)
        {
            om.Script.Destroy();
        }

        EndOptionsScreenNotification();
    }

    public void StartOptions(OptionPanelSettings settings, ButtonMaster buttonMaster, System.Action endOptionsScreenNotification)
    {
        _buttonMaster = buttonMaster;
        EndOptionsScreenNotification = endOptionsScreenNotification;

        Vector3 thisPosition = new Vector3(0, 0, 0);
        Quaternion quaternion = new Quaternion(0, 0, 0, 0);
        if (settings != null)
        {
            _menus = settings.OptionMenuList.ToArray();

            for (int i = 0; i < _menus.Length; i++)
            {
                //Set the position and scale of the newly spawned menu items.

                thisPosition.y = (-1f * i) + 0.5f;
                thisPosition.z = 10;

                GameObject menuSelectorClone = (GameObject)Instantiate(
                    _menuSelectorPrefab,
                    thisPosition, //transform.position, 
                    quaternion// transform.rotation
                    );
                menuSelectorClone.transform.localScale = new Vector3(0.015f, 0.015f, 1);
                menuSelectorClone.transform.parent = gameObject.transform;

                //Attach the script to the menu object so we can manipulate the object programmatically
                MenuSelector mss = menuSelectorClone.GetComponent<MenuSelector>();
                _menus[i].Script = mss;

                OptionMenu om = _menus[i];
                mss.SetMenuOptionText(om.Title);
                mss.SetValueText(om.GetSelectedValue());
                mss.ShowLeftArrow(!om.IsFirstValue());
                mss.ShowRightArrow(!om.IsLastValue());

                mss.SetHighlight(i == 0);
            }

            _menuIndex = 0;
        }

        _panelActive = true;
    }

    public Dictionary<string, string> GetSelectedOptions()
    {
        Dictionary<string, string> selectedOptions = new Dictionary<string, string>();

        foreach (OptionMenu menu in _menus)
        {
            selectedOptions[menu.Title] = menu.GetSelectedValue();
        }

        return selectedOptions;
    }

    private OptionMenu[] _menus;
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
        bool bSuccess = false;
        if (newIndex < 0)
        {
            _menuIndex = 0;
        }
        else if (newIndex >= _menus.Length)
        {
            _menuIndex = _menus.Length - 1;
        }
        else
        {
            bSuccess = true;
            _menuIndex = newIndex;
        }

        UpdateSelectedMenu();
        if (bSuccess)
        {
            PlayChangeSuccessSound();
        }
        else
        {
            PlayChangeFailSound();
        }
    }

    public void MoveLeft()
    {
        MoveValue(false);
    }

    public void MoveRight()
    {
        MoveValue(true);
    }

    public void MoveValue(bool bNext)
    {
        bool bSuccess = false;
        if (bNext)
        {
            bSuccess = _menus[_menuIndex].NextOption();
        }
        else
        {
            bSuccess = _menus[_menuIndex].PreviousOption();
        }

        UpdateSelectedOption();
        if (bSuccess)
        {
            PlayChangeSuccessSound();
        }
        else
        {
            PlayChangeFailSound();
        }
    }

    void UpdateSelectedMenu()
    {
        for (int i = 0; i < _menus.Length; i++)
        {
            OptionMenu menu = _menus[i];
            menu.Script.SetHighlight(i == _menuIndex);
        }

        UpdateSelectedOption();
    }

    void UpdateSelectedOption()
    {
        OptionMenu menu = _menus[_menuIndex];
        menu.Script.SetValueText(menu.GetSelectedValue());
        menu.Script.ShowLeftArrow(!menu.IsFirstValue());
        menu.Script.ShowRightArrow(!menu.IsLastValue());
    }

    #region sounds

    void PlayChangeSuccessSound()
    {
        _audioSource.PlayOneShot(ChangeValueAudioClip);
    }

    void PlayChangeFailSound()
    {
        _audioSource.PlayOneShot(ChangeValueFailAudioClip);
    }

    void PlayContinueSound()
    {
        _audioSource.PlayOneShot(ContinueAudioClip);
    }

    #endregion
}

public class OptionPanelSettings
{
    public List<OptionMenu> OptionMenuList = new List<OptionMenu>();

    public OptionPanelSettings(List<OptionMenu> optionList)
    {
        OptionMenuList.AddRange(optionList);
    }
}

public class OptionMenu
{
    public string[] MenuOptions;
    public string Title;
    public int OptionIndex = 0;
    public MenuSelector Script;

    public OptionMenu(string sTitle, string[] optionParams)
        : this(sTitle, optionParams, null)
    {
    }

    public OptionMenu(string sTitle, string[] optionParams, string sDefaultValue)
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

    public string GetSelectedValue()
    {
        return MenuOptions[OptionIndex];
    }

    public bool IsFirstValue()
    {
        return (OptionIndex == 0);
    }

    public bool IsLastValue()
    {
        return (OptionIndex == (MenuOptions.Length - 1));
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