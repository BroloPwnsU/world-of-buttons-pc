using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PartyGroupBrain : MonoBehaviour
{
    //There is a component called...
    private HealthBar _healthBarScript;
    private DamageSpriteScript _damageSpriteScript;
    private CritSpriteScript _critSpriteScript;
    private Text _healthText;

    private AudioSource _audioSource;
    public List<AudioClip> AttackSounds;
    public List<AudioClip> CritSounds;

    // Use this for initialization
    void Awake()
    {
        _healthBarScript = gameObject.GetComponentInChildren<HealthBar>();
        _damageSpriteScript = gameObject.GetComponentInChildren<DamageSpriteScript>();
        _critSpriteScript = gameObject.GetComponentInChildren<CritSpriteScript>();
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
        _healthBarScript.TakeDamage(damage, false, newCurrentHP, originalHP);

        if (bCrit)
            _critSpriteScript.Show();
        else
            _damageSpriteScript.Show();

        _healthText.text = ((int)newCurrentHP).ToString();

    }
}