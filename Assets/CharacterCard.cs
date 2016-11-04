using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CharacterCard : MonoBehaviour {

    private Text _characterText;
    private SpriteRenderer _characterCardSprite;
    //private SpriteRenderer _characterCardBackdrop;
    private CurrentColor _currentColor;

    public Sprite BlueFemale;
    public Sprite BlueMale;
    public Sprite GreenFemale;
    public Sprite GreenMale;

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
        BlueMale,
        BlueFemale,
        GreenMale,
        GreenFemale
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
            case CurrentColor.GreenFemale:
                _currentColor = CurrentColor.BlueMale;
                LoadBlueMale();
                break;
            case CurrentColor.BlueMale:
                _currentColor = CurrentColor.GreenMale;
                LoadGreenMale();
                break;
            case CurrentColor.GreenMale:
                _currentColor = CurrentColor.BlueFemale;
                LoadBlueFemale();
                break;
            case CurrentColor.None:
            case CurrentColor.BlueFemale:
            default:
                _currentColor = CurrentColor.GreenFemale;
                LoadGreenFemale();
                break;

        }
    }

    void LoadGreenFemale()
    {
        _characterCardSprite.sprite = GreenFemale;
        _characterText.text = FormatText(
            "The Mint Markswoman",
            "California Gaming",
            "360 No Scope",
            "A gentle soul. Likes long walks on the beach, comfortable sweatshirts with concealing hoods, and max range headshots."
            );
    }

    void LoadGreenMale()
    {
        _characterCardSprite.sprite = GreenMale;
        _characterText.text = FormatText(
            "The Emerald Assassin",
            "Gamer to the Grave",
            "Activate Hax",
            "Cunning, deceiptful, and a total prick. Would stab his own mother if he hadn't already. Known to employ poisoned daggers. Likes cats."
            );
    }

    void LoadBlueFemale()
    {
        _characterCardSprite.sprite = BlueFemale;
        _characterText.text = FormatText(
            "The Cobalt Conjurer",
            "Party Wizard",
            "Mildly Magic Missile",
            "Former wielder of the Staff of Gal'thorin, now retired. Nowadays spends her free time at frat parties making refreshments 'disappear'."
            );
    }

    void LoadBlueMale()
    {
        _characterCardSprite.sprite = BlueMale;
        _characterText.text = FormatText(
            "The Denim Delinquent",
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
