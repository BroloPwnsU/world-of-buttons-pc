using UnityEngine;
using System.Collections;

public class PVPSprite : MonoBehaviour
{    
    Animator animator;
    
    // Use this for initialization
    void Awake () {
        animator = GetComponent<Animator>();
    }

    public void Jump()
    {
        //Debug.Log("Animator layer count: " + animator.layerCount);
        //animator.Play("bf_jump", 0);
        animator.SetTrigger("Jump");
    }

    public void Land()
    {
        //Debug.Log("Animator layer count: " + animator.layerCount);
        //animator.Play("bf_jump", 0);
        animator.SetTrigger("Land");
    }
}
