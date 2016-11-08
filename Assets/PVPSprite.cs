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
        //animator.Play("bf_jump", 0);
        animator.SetTrigger("Jump");
    }

    public void Land()
    {
        //animator.Play("bf_jump", 0);
        animator.SetTrigger("Land");
    }

    public void TakeDamage()
    {
        animator.SetTrigger("Damage");
    }

    public void Win()
    {
        animator.SetTrigger("Win");
    }

    public void Die()
    {
        animator.SetTrigger("Die");
    }
}
