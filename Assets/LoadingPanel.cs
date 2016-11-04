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
    private float _wLoadingScreenDuration = 5.0f;

    private Action EndLoadingNotification;
    private float _loadingTimeLeft = 0;
    private Text _loadingText;
    private GameObject _loadingBar;
    private float _barInitialScaleX;

    public GameObject AdSpriteObject;
    private SpriteRenderer _adSpriteRenderer;
    public GameObject AdQuipObject;
    private Text _adQuipText;

    public List<Sprite> AdSprites;
    public List<string> AdQuips;

    void Awake()
    {
        //Assign the text blurb, the loading bar, and possibly the loading bar wrapper.
        //_loadingText = GameObject.Find("LoadingBlurbText").GetComponent<Text>();
        _loadingBar = GameObject.Find("LoadingBar");
        _adSpriteRenderer = AdSpriteObject.GetComponent<SpriteRenderer>();
        _adQuipText = AdQuipObject.GetComponent<Text>();

        _barInitialScaleX = _loadingBar.transform.localScale.x;
    }

    public void StartLoadingPanel(float fLoadingScreenDuration, Action endLoadingNotification)
    {
        //What what in the butt.
        base.Show();
        EndLoadingNotification = endLoadingNotification;
        _wLoadingScreenDuration = fLoadingScreenDuration;
        _loadingTimeLeft = fLoadingScreenDuration;

        //The ad is a sprite and a blurb together, forever
        RenderAd();
    }

    void CloseLoadingPanel()
    {
        EndLoadingNotification();
    }

    // Update is called once per frame
    void Update()
    {
        _loadingTimeLeft -= Time.deltaTime;
        RenderLoadingBar();
        
        //The loading bar has dwindled to zero...
        if (_loadingTimeLeft <= 0)
        {
            //Has the loading screen overstayed its welcome? Yes? What a pity. What. A. Pity.
            CloseLoadingPanel();
        }
    }

    #region Loading Bar/Timer

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
        float currentScaleX = (_loadingTimeLeft / _wLoadingScreenDuration) * _barInitialScaleX;
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

    #endregion

    void RenderAd()
    {
        //Pull in an Ad sprite and the corresponding quip.
        //Woohoo.
        //Ad sprite and quip count should be the same.
        if (this.AdSprites != null && this.AdSprites.Count > 0)
        {
            int wRandomIndex = UnityEngine.Random.Range(0, this.AdSprites.Count);
            _adSpriteRenderer.sprite = AdSprites[wRandomIndex];
            _adQuipText.text = AdQuips[wRandomIndex];
        }
        else
        {
            AdSpriteObject.SetActive(false);
        }
    }

    #region Blurbs

    /*
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

    //private List<string> _blurbList = new List<string>()
    //{
    //    "Enjoy this free advertisement while you wait...",
    //    "And now a message from our sponsors...",
    //    "Go Prime to hide ads! Kappa!",
    //    "Loading only takes like a second, but we need you to look at this ad for at least 5 seconds in order to create the desire to buy something. It's basic marketing.",
    //    "Conform Buy Consume Obey Conform Buy Consume Obey...",
    //    "Would you kindly look at this ad? Would you? Kindly?",
    //    "The advertisments will continue until morale improves!",

        /*
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
        */
    //};

    #endregion
}
