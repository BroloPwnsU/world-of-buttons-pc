using UnityEngine;
using System.Collections;

public class MenuArrow : MonoBehaviour {

    public Renderer rend;

    private Color colorShow = new Color(255, 0, 0, 1);
    private Color colorHide = new Color(255, 0, 0, 0);

    void Awake()
    {
        rend = GetComponent<Renderer>();
        rend.material.color = colorShow;
    }

    public void Show(bool bActive)
    {
        rend.material.color = (bActive) ? colorShow : colorHide;
    }
}
