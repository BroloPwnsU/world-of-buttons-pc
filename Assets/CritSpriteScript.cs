using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CritSpriteScript : MonoBehaviour
{

    float _duration = 1.0f;
    float _timeLeft = 0.0f;
    public Renderer rend;

    private Color colorStart = new Color(0, 255, 0, 1);
    private Color colorEnd = new Color(0, 255, 0, 0);

    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.material.color = Color.Lerp(colorStart, colorEnd, 1);
    }

    public void Show()
    {
        _timeLeft = _duration;
    }

    // Update is called once per frame
    void Update()
    {
        if (_timeLeft > 0)
        {
            _timeLeft -= Time.deltaTime;

            float a = _timeLeft * 10;
            float b = 2;

            float lerp = (a - b * Mathf.Floor(a / b)) / b;
            rend.material.color = Color.Lerp(colorStart, colorEnd, lerp);
        }
    }
}