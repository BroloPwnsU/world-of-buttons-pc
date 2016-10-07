﻿using UnityEngine;
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

    void Reset()
    {
        transform.localScale += new Vector3(
            _healthBarInitialScaleX,
            transform.localScale.y,
            transform.localScale.z
            );
    }

    public void TakeDamage(float damage, float newCurrentHP, float originalHP)
    {
        float currentScaleX = (newCurrentHP/originalHP) * _healthBarInitialScaleX;
        transform.localScale = new Vector3(
            currentScaleX,
            transform.localScale.y,
            transform.localScale.z
            );
    }
}
