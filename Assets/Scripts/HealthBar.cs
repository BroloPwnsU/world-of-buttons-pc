using UnityEngine;
using System.Collections;

public class HealthBar : MonoBehaviour {

    private float _healthBarInitialScaleX;

    // Use this for initialization
    void Awake () {
        _healthBarInitialScaleX = transform.localScale.x;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Reset()
    {
        transform.localScale += new Vector3(
            _healthBarInitialScaleX,
            transform.localScale.y,
            transform.localScale.z
            );
    }

    public void TakeDamage(float damage, bool bCrit, float newCurrentHP, float originalHP)
    {
        SetCurrentHealth(newCurrentHP, originalHP);
    }

    public void SetCurrentHealth(float newCurrentHP, float originalHP)
    {
        float currentScaleX = (newCurrentHP / originalHP) * _healthBarInitialScaleX;
        transform.localScale = new Vector3(
            currentScaleX,
            transform.localScale.y,
            transform.localScale.z
            );
    }
}
