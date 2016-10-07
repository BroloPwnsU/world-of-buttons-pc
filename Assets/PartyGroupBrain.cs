using UnityEngine;
using System.Collections;

public class PartyGroupBrain : MonoBehaviour
{
    //There is a component called...
    private HealthBar _healthBarScript;
    private DamageSpriteScript _damageSpriteScript;

    // Use this for initialization
    void Awake()
    {
        _healthBarScript = GameObject.Find("PlayerHealthBar").GetComponent<HealthBar>();
        _damageSpriteScript = GameObject.Find("PlayerDamageSprite").GetComponent<DamageSpriteScript>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void MakeAttack(bool bCrit)
    {
    }

    public void TakeDamage(float damage, float newCurrentHP, float originalHP)
    {
        _healthBarScript.TakeDamage(damage, false, newCurrentHP, originalHP);

        _damageSpriteScript.Show();
    }
}