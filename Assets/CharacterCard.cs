using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CharacterCard : MonoBehaviour {

    private Text _characterText;
    private SpriteRenderer _characterCardSprite;
    //private SpriteRenderer _characterCardBackdrop;
    private CurrentColor _currentColor;

    // Use this for initialization
    void Awake ()
    {
        _currentColor = CurrentColor.None;
        _characterText = GetComponentInChildren<Text>(true);
        _characterCardSprite = GameObject.Find("CharacterCardSprite").GetComponent<SpriteRenderer>();
        //_characterCardBackdrop = GameObject.Find("CharacterCardBackdrop").GetComponent<SpriteRenderer>();
    }

    enum CurrentColor
    {
        None,
        Blue,
        Red,
        Green,
        Gray
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);

        switch (_currentColor)
        {
            //Go to the next color
            case CurrentColor.Red:
                _currentColor = CurrentColor.Green;
                LoadGreen();
                break;
            case CurrentColor.Green:
                _currentColor = CurrentColor.Blue;
                LoadBlue();
                break;
            case CurrentColor.Blue:
                _currentColor = CurrentColor.Gray;
                LoadGray();
                break;
            case CurrentColor.None:
            case CurrentColor.Gray:
            default:
                _currentColor = CurrentColor.Red;
                LoadRed();
                break;

        }
    }

    void LoadRed()
    {
        _characterCardSprite.color = new Color(255, 0, 0, 1.0f);
        _characterText.text = FormatText(
            "Crimson Crusher",
            "California Gaming",
            "360 No Scope",
            "A gentle soul. Likes long walks on the beach, comfortable sweatshirts with concealing hoods, and max range headshots."
            );
    }

    void LoadGreen()
    {
        _characterCardSprite.color = new Color(0, 255, 0, 1.0f);
        _characterText.text = FormatText(
            "Emerald Assassin",
            "Gamer to the Grave",
            "Activate Hax",
            "Cunning, deceiptful, and a total prick. Would stab his own mother if he hadn't already. Known to employ poisoned daggers. Likes cats."
            );
    }

    void LoadBlue()
    {
        _characterCardSprite.color = new Color(0, 0, 255, 1.0f);
        _characterText.text = FormatText(
            "Steve the Sorcerer",
            "Party Wizard",
            "Conjure Beer",
            "Once weilder of the Staff of Gal'thorin, now retired. Spends free time frat parties making refreshments 'disappear.'"
            );
    }

    void LoadGray()
    {
        _characterCardSprite.color = new Color(180, 180, 180, 1.0f);
        _characterText.text = FormatText(
            "The Gray Goon",
            "Polygons",
            "Flail Wildly",
            "What he lacks in subtlety he makes up for in strong arms and stronger odors. Wields his halitosis like a weapon. A lethal weapon."
            );
    }

    string FormatText(string sName, string sTee, string sButton, string sBio)
    {
        return string.Format(
            "Character:{0}{1}{0}{0}Favorite J!NX Tee:{0}{2}{0}{0}Favorite Button:{0}{3}{0}{0}Bio:{0}{4}",
            "\r\n",
            sName,
            sTee,
            sButton,
            sBio
            );
    }
}
