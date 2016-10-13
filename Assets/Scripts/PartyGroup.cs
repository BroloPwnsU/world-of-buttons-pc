﻿using UnityEngine;
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

    public bool IsPVE = true;
    public GameObject PVESpritePanel;
    public GameObject PVPSpritePanel;

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

    // Update is called once per frame
    void Update()
    {

    }

    public void RefreshHealth(float currentHP, float originalHP)
    {
        _healthText.text = ((int)currentHP).ToString();
        _healthBar.SetCurrentHealth(currentHP, originalHP);
    }

    public void MakeAttack(bool bCrit)
    {
        if (bCrit)
            _audioSource.PlayOneShot(
                AttackSounds[Random.Range(0, AttackSounds.Count - 1)],
                0.1f
                );
        else
            _audioSource.PlayOneShot(
                CritSounds[Random.Range(0, CritSounds.Count - 1)],
                0.1f
                );
    }

    public void TakeDamage(float damage, float newCurrentHP, float originalHP, bool bCrit)
    {
        _healthBar.TakeDamage(damage, false, newCurrentHP, originalHP);

        if (bCrit)
            _critSprite.Show();
        else
            _damageSprite.Show();

        _healthText.text = ((int)newCurrentHP).ToString();

    }
}