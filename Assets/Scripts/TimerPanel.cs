using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TimerPanel : MonoBehaviour
{ 
    //There is a component called...
    private TimerBar _barScript;
    private Text _timerText;

    // Use this for initialization
    void Awake()
    {
        _barScript = GameObject.Find("TimerBar").GetComponent<TimerBar>();
        _timerText = GameObject.Find("TimeLeftText").GetComponent<Text>();
    }

    public void SetTime(float timeLeft, float originalTimeLeft)
    {
        _barScript.SetTime(timeLeft, originalTimeLeft);
        _timerText.text = timeLeft.ToString(".00");
    }
}
