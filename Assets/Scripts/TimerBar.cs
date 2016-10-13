using UnityEngine;
using System.Collections;

public class TimerBar : MonoBehaviour
{

    private float _barInitialScaleX;

    // Use this for initialization
    void Awake()
    {
        _barInitialScaleX = transform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Reset()
    {
        transform.localScale += new Vector3(
            _barInitialScaleX,
            transform.localScale.y,
            transform.localScale.z
            );
    }

    public void SetTime(float timeLeft, float originalTimeLeft)
    {
        float currentScaleX = (timeLeft / originalTimeLeft) * _barInitialScaleX;
        transform.localScale = new Vector3(
            currentScaleX,
            transform.localScale.y,
            transform.localScale.z
            );
    }
}
