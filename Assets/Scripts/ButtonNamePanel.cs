using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonNamePanel : GamePanel
{
    private Text _victoryText;

    void Awake()
    {
        _victoryText = GetComponentInChildren<Text>();
        _victoryText.text = "Get Ready...";
    }

    public void SetText(string sText)
    {
        _victoryText.text = sText;
    }
}
