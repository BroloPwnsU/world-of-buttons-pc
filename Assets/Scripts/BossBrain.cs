/*
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossBrain : MonoBehaviour
{
    //There is a component called...
    private HealthBar _healthBarScript;
    private DamageSpriteScript _damageSpriteScript;
    private CritSpriteScript _critSpriteScript;

    private AudioSource _audioSource;
    public List<AudioClip> AttackSounds;
    public List<AudioClip> CritSounds;

    // Use this for initialization
    void Awake()
    {
        _healthBarScript = GameObject.Find("BossHealthBar").GetComponent<HealthBar>();
        _damageSpriteScript = GameObject.Find("BossDamageSprite").GetComponent<DamageSpriteScript>();
        _critSpriteScript = GameObject.Find("BossCritSprite").GetComponent<DamageSpriteScript>();
        _audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void MakeAttack(bool bCrit)
    {
        //Bosses don't actually make crit sounds, but we have it here anyway
        _audioSource.PlayOneShot(
            AttackSounds[Random.Range(0, AttackSounds.Count - 1)],
            1.0f
            );
    }

    public void TakeDamage(float damage, bool bCrit, float newCurrentHP, float originalHP)
    {
        _healthBarScript.TakeDamage(damage, bCrit, newCurrentHP, originalHP);

        _damageSpriteScript.Show();
    }
}
*/