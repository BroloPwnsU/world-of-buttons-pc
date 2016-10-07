using UnityEngine;
using System.Collections;

public class DamageSpriteScript : MonoBehaviour {

    float _duration = 1.0f;
    float _timeLeft = 0.0f;
    public Renderer rend;

    private Color colorStart = new Color(255, 0, 0, 1);
    private Color colorEnd = new Color(255, 0, 0, 0);
    
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
        _timeLeft -= Time.deltaTime;

        float lerp = 1 - (_timeLeft / _duration);
        rend.material.color = Color.Lerp(colorStart, colorEnd, lerp);
    }
}