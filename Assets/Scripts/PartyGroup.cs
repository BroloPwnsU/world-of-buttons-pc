using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PartyGroup : MonoBehaviour
{
    //There is a component called...
    private HealthBar _healthBar;
    private DamageSprite _damageSprite;
    private CritSprite _critSprite;
    private Text _healthText;

    private AudioSource _audioSource;
    public List<AudioClip> AttackSounds;
    public List<AudioClip> CritSounds;
    public List<AudioClip> FailSounds;

    private bool _bIsPVP = false;
    public GameObject PVESpritePanel;
    public GameObject PVPSpritePanel;

    public GameObject Round1VictorySprite;
    public GameObject Round2VictorySprite;
    public GameObject Round3VictorySprite;
    public GameObject Round4VictorySprite;

    public Material SpriteSubdueMaterial;
    public Material SpriteActiveMaterial;

    // Use this for initialization
    void Awake()
    {
        _healthBar = gameObject.GetComponentInChildren<HealthBar>();
        _damageSprite = gameObject.GetComponentInChildren<DamageSprite>();
        _critSprite = gameObject.GetComponentInChildren<CritSprite>();
        _audioSource = GetComponent<AudioSource>();

        //If we have to add multiple text boxes then we will need this script to sort through them and select the proper textbox
        Text [] textBoxes = gameObject.GetComponentsInChildren<Text>();
        if (textBoxes != null)
        {
            foreach (Text text in textBoxes)
            {
                if (text.gameObject.name == "PlayerHealthText")
                {
                    _healthText = text;
                    break;
                }
            }
        }
    }

    public void PrepForBattle(bool bIsPVP, float StartingHealth, int wVictoriesNeededToWin, int wCurrentVictores)
    {
        _bIsPVP = bIsPVP;

        RefreshHealth(StartingHealth, StartingHealth);

        if (_bIsPVP)
        {
            PVESpritePanel.SetActive(true);
            PVPSpritePanel.SetActive(false);
        }
        else
        {
            PVESpritePanel.SetActive(false);
            PVPSpritePanel.SetActive(true);
        }

        if (wVictoriesNeededToWin <= 1)
        {
            //If the party only needs one victory to win, then we don't need to keep count of victories
            for (int i = 1; i <= 4; i++)
            {
                GetVictorySprite(i).SetActive(false);
            }
        }
        else
        {
            //We only support up to 3 victories per player. Because we're lazy coders
            for (int i = 1; i <= 4; i++)
            {
                if (i <= wVictoriesNeededToWin)
                {
                    //Show this sprite
                    GetVictorySprite(i).SetActive(true);

                    //Tell it whether to make it bright or make it boring.
                    if (i <= wCurrentVictores)
                    {
                        //The party has already won this many games. Make it shine!
                        GetVictorySprite(i).GetComponent<Renderer>().material = SpriteActiveMaterial;
                    }
                    else
                    {
                        //The party has yet to win this many games. Make it dull.
                        GetVictorySprite(i).GetComponent<Renderer>().material = SpriteSubdueMaterial;
                    }
                }
                else
                {
                    //Hide this sprite. It's more than we need.
                    GetVictorySprite(i).SetActive(false);
                }
            }
        }
    }

    GameObject GetVictorySprite(int i)
    {
        if (i == 1) return Round1VictorySprite;
        else if (i == 2) return Round2VictorySprite;
        else if (i == 3) return Round3VictorySprite;
        else if (i == 4) return Round4VictorySprite;

        return Round1VictorySprite;
    }

    public void RefreshHealth(float currentHP, float originalHP)
    {
        _healthText.text = ((int)currentHP).ToString();
        _healthBar.SetCurrentHealth(currentHP, originalHP);
    }

    public void MakeAttack(bool bCrit)
    {
        PlayAttackSound(bCrit);
    }

    void PlayAttackSound(bool bCrit)
    {
        if (bCrit && CritSounds != null && CritSounds.Count > 0)
        {
            _audioSource.PlayOneShot(
                CritSounds[Random.Range(0, CritSounds.Count - 1)],
                0.5f
                );
        }
        else if (AttackSounds != null && AttackSounds.Count > 0)
        {
            _audioSource.PlayOneShot(
                AttackSounds[Random.Range(0, AttackSounds.Count - 1)],
                0.5f
                );
        }
    }

    void PlayFailSound()
    {
        if (FailSounds != null && FailSounds.Count > 0)
        {
            _audioSource.PlayOneShot(
                FailSounds[Random.Range(0, FailSounds.Count - 1)],
                0.5f
                );
        }
    }

    public void TakeDamage(float damage, float newCurrentHP, float originalHP, bool bCrit, bool bSelfAttack)
    {
        _healthBar.TakeDamage(damage, false, newCurrentHP, originalHP);

        if (bCrit)
            _critSprite.Show();
        else
            _damageSprite.Show();

        _healthText.text = ((int)newCurrentHP).ToString();

        if (bSelfAttack)
        {
            PlayFailSound();
        }
    }
}