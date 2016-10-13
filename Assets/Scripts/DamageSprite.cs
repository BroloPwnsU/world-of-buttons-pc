using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DamageSprite : MonoBehaviour {

    float _duration = 1.0f;
    float _timeLeft = 0.0f;
    private Renderer _rend;

    private Color colorStart = new Color(255, 0, 0, 1);
    private Color colorEnd = new Color(255, 0, 0, 0);
    
    void Start()
    {
        _rend = GetComponent<Renderer>();
        _rend.material.color = Color.Lerp(colorStart, colorEnd, 1);
    }

    public void Show()
    {
        _timeLeft = _duration;
    }

    // Update is called once per frame
    void Update()
    {
        _timeLeft -= Time.deltaTime;

        float lerp = 1 - (_timeLeft / _duration);
        _rend.material.color = Color.Lerp(colorStart, colorEnd, lerp);
    }
}