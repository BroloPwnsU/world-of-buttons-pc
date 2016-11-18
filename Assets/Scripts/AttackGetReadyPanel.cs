using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AttackGetReadyPanel : GamePanel
{
    private Text _text;

    public string NormalText = "Get Ready...";
    public string SuddenDeathText = "Sudden Death!!!\r\nGet Ready...";

    void Awake()
    {
        _text = GetComponentInChildren<Text>(true);
    }

    public override void Show()
    {
        this.Show(AttackMode.Normal);
    }

    public void Show(AttackMode attackMode)
    {
        if (_text != null)
        {
            if (attackMode == AttackMode.Normal)
            {
                _text.text = NormalText;
            }
            else
            {
                _text.text = SuddenDeathText;
            }
        }

        base.Show();
    }
}