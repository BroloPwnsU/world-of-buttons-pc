using UnityEngine;
using System.Collections;

public class BossBrain : MonoBehaviour
{
    //There is a component called...
    private HealthBar _healthBarScript;
    private DamageSpriteScript _damageSpriteScript;

    // Use this for initialization
    void Awake()
    {
        _healthBarScript = GameObject.Find("BossHealthBar").GetComponent<HealthBar>();
        _damageSpriteScript = GameObject.Find("BossDamageSprite").GetComponent<DamageSpriteScript>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void MakeAttack()
    {
    }

    public void TakeDamage(float damage, bool bCrit, float newCurrentHP, float originalHP)
    {
        _healthBarScript.TakeDamage(damage, bCrit, newCurrentHP, originalHP);

        _damageSpriteScript.Show();
    }
}