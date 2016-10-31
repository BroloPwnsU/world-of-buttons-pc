using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class LoadingPanel : GamePanel
{
    //The minimum blurb duration is the shortest it can be shown, regardless of text length.
    public float MinimumTextDuration = 1.0f;
    public float TextDurationIncreasePerCharacter = 0.1f;
    public float LoadingScreenDuration = 5.0f;

    private Action EndLoadingNotification;
    private float _blurbTimeLeft = 0;
    private float _blurbOriginalTimeLeft = 0;
    private float _loadingTimeLeft = 0;
    private Stack<int> _blurbStack;
    private Text _loadingText;
    private GameObject _loadingBar;
    private float _barInitialScaleX;


    void Awake ()
    {
        //Assign the text blurb, the loading bar, and possibly the loading bar wrapper.
        _loadingText = GameObject.Find("LoadingBlurbText").GetComponent<Text>();
        _loadingBar = GameObject.Find("LoadingBar");

        _barInitialScaleX = _loadingBar.transform.localScale.x;
    }

    public void StartLoadingPanel(Action endLoadingNotification)
    {
        //What what in the butt.
        base.Show();
        EndLoadingNotification = endLoadingNotification;
        _blurbTimeLeft = MinimumTextDuration;
        _blurbOriginalTimeLeft = _blurbTimeLeft;
        _loadingTimeLeft = LoadingScreenDuration;
        RebuildBlurbStack();
        RenderBlurb();
    }

    void CloseLoadingPanel()
    {
        EndLoadingNotification();
    }
	
	// Update is called once per frame
	void Update ()
    {
	    /*if (ButtonMaster.IsStartKey())
        {
            //We can skip the loading screen by pressing the start button, just for convenience.
            CloseLoadingPanel();
        }
        else
        {*/
            _blurbTimeLeft -= Time.deltaTime;
            _loadingTimeLeft -= Time.deltaTime;
            RenderLoadingBar();

            if (_blurbTimeLeft <= 0)
            {
                //The loading bar has dwindled to zero...
                if (_loadingTimeLeft <= 0)
                {
                    //Has the loading screen overstayed its welcome? Yes? What a pity. What. A. Pity.
                    CloseLoadingPanel();
                }
                else
                {
                    //Still have some loading screen power left in us? Show another blurb!
                    RenderBlurb();
                }
            }
        //}
	}

    void ResetLoadingBar()
    {
        transform.localScale += new Vector3(
            _barInitialScaleX,
            transform.localScale.y,
            transform.localScale.z
            );
    }

    void RenderLoadingBar()
    {
        float currentScaleX = (_blurbTimeLeft / _blurbOriginalTimeLeft) * _barInitialScaleX;
        _loadingBar.transform.localScale = new Vector3(
            currentScaleX,
            _loadingBar.transform.localScale.y,
            _loadingBar.transform.localScale.z
            );
    }

    public void SetTime(float timeLeft, float originalTimeLeft)
    {
        float currentScaleX = (timeLeft / originalTimeLeft) * _barInitialScaleX;
        transform.localScale = new Vector3(
            currentScaleX,
            transform.localScale.y,
            transform.localScale.z
            );
    }

    void RenderBlurb()
    {
        int wNextIndex = 0;
        if (_blurbStack.Count <= 0)
        {
            //Have we run out of blurb indexes? Damn.
            RebuildBlurbStack();
        }

        //Grab the next blurb.
        wNextIndex = _blurbStack.Pop();
        
        //Bind it to the text element
        string sText = _blurbList[wNextIndex] + "...";
        _loadingText.text = sText;

        //Now figure out the time duration to make sure it shows longer for long blurbs
        _blurbTimeLeft = MinimumTextDuration + (TextDurationIncreasePerCharacter * sText.Length);
        _blurbOriginalTimeLeft = _blurbTimeLeft;
    }

    void RebuildBlurbStack()
    {
        _blurbStack = new Stack<int>();

        List<int> indexList = new List<int>();
        int count = _blurbList.Count;
        for (int i = 0; i < count; i++)
        {
            indexList.Add(i);
        }
        
        while (indexList.Count > 1)
        {
            int removeIndex = UnityEngine.Random.Range(0, indexList.Count);
            _blurbStack.Push(
                indexList[removeIndex]
                );
            indexList.RemoveAt(removeIndex);
        }
        //Push in the very last one.
        _blurbStack.Push(indexList[0]);
    }

    private List<string> _blurbList = new List<string>()
    {
        "Downloading launch day DLC",
        "Looking for raid",
        "Flaunting item score",
        "Clearing browser history",
        "Ignoring the Prime Directive",
        "**SPONSORED** - Getting super comfortable in new J!NX Brand clothing",
        "Spending grocery money on vanity item",
        "Trolling in all chat",
        "Eating a delicious cookie",
        "Reading your email",
        "Buttoning all the buttons",
        "Depolarizing hydropositronic button recoupler",
        "Summoning demonic buttons from the void",
        "Deleting self-pressing button subrouting",
        "Hyperventilating cuz there are so many buttons and I never learned to read",
        "Thinking of ways to confuse you",
        "Shooting kerbals into space without space suits",
        "Ganking Alliance noobs in Darkshire",
        "Buying gold from a gold farmer",
        "WTS extra buttons for CHEAP!!! PST best offer",
        "Waiting for High Noon",
        "Watching my Hanzo Play of the Game, nbd",
        "Eating cheetos and drinking Mountain Dew",
    };
}
