using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuSelector : MonoBehaviour {

    private Text _menuOptionText;
    private Text _valueText;
    private MenuArrow _leftArrow;
    private MenuArrow _rightArrow;

    public Color HighlightColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
    public Color LowlightColor = new Color(0.0f, 0.0f, 1.0f, 1.0f);

    // Use this for initialization
    void Awake ()
    {
        //If we have to add multiple text boxes then we will need this script to sort through them and select the proper textbox
        Text[] textBoxes = gameObject.GetComponentsInChildren<Text>();
        if (textBoxes != null)
        {
            foreach (Text text in textBoxes)
            {
                if (text.gameObject.name == "MenuOptionText")
                {
                    _menuOptionText = text;
                }
                else if (text.gameObject.name == "ValueText")
                {
                    _valueText = text;
                }
            }
        }

        MenuArrow[] arrows = gameObject.GetComponentsInChildren<MenuArrow>();
        if (arrows != null)
        {
            foreach (MenuArrow scr in arrows)
            {
                if (scr.gameObject.name == "LeftArrow")
                {
                    _leftArrow = scr;
                }
                else if (scr.gameObject.name == "RightArrow")
                {
                    _rightArrow = scr;
                }
            }
        }
    }

    void Start()
    {
    }

    public void Destroy()
    {
        Debug.Log("Destorying");

        if (gameObject != null)
            Destroy(gameObject);
    }

    public void ShowMe()
    {
        gameObject.SetActive(true);
    }

    // Update is called once per frame
    //float scalexduration = 2.0f;
    //float scaleyduration = 3.0f;
    //float rotationxduration = 5.0f;
    //float rotationyduration = 6.0f;
    //float rotationzduration = 7.0f;
    void Update ()
    {
        //float scaleXLerp = Mathf.PingPong(Time.time, scaleXDuration) / scaleXDuration;
        //float scaleYLerp = Mathf.PingPong(Time.time, scaleYDuration) / scaleYDuration;
        //float rotationXLerp = Mathf.PingPong(Time.time, rotationXDuration) / rotationXDuration;
        //float rotationYLerp = Mathf.PingPong(Time.time, rotationYDuration) / rotationYDuration;
        //float rotationZLerp = Mathf.PingPong(Time.time, rotationZDuration) / rotationZDuration;
        //gameObject.transform.localScale += new Vector3(-.0001f, -.0001f, 0);
    }

    public void SetMenuOptionText(string sText)
    {
        _menuOptionText.text = sText;
    }

    public void SetValueText(string sText)
    {
        _valueText.text = sText;
    }

    public void ShowLeftArrow(bool bVisible)
    {
        _leftArrow.Show(bVisible);
    }

    public void ShowRightArrow(bool bVisible)
    {
        _rightArrow.Show(bVisible);
    }

    public void SetHighlight(bool bHighlighted)
    {
        if (bHighlighted)
        {
            _menuOptionText.color = HighlightColor;
            _valueText.color = HighlightColor;
        }
        else
        {
            _menuOptionText.color = LowlightColor;
            _valueText.color = LowlightColor;
        }
    }
}
