using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PartyGroupBrain : MonoBehaviour
{
    //There is a component called...
    private HealthBar _healthBarScript;
    private DamageSpriteScript _damageSpriteScript;

    private AudioSource _audioSource;
    public List<AudioClip> AttackSounds;
    public List<AudioClip> CritSounds;

    // Use this for initialization
    void Awake()
    {
        _healthBarScript = GameObject.Find("PlayerHealthBar").GetComponent<HealthBar>();
        _damageSpriteScript = GameObject.Find("PlayerDamageSprite").GetComponent<DamageSpriteScript>();
        _audioSource = GetComponent<AudioSource>();
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
                1.0f
                );
        else
            _audioSource.PlayOneShot(
                CritSounds[Random.Range(0, CritSounds.Count - 1)],
                1.0f
                );

    }

    public void TakeDamage(float damage, float newCurrentHP, float originalHP)
    {
        _healthBarScript.TakeDamage(damage, false, newCurrentHP, originalHP);

        _damageSpriteScript.Show();
    }
}