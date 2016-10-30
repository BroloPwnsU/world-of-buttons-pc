using UnityEngine;
using System.Collections;


public class TheBigButton : MonoBehaviour
{
    private Animator animator;

	// Use this for initialization
	void Awake () {
        animator = GetComponent<Animator>();
	}

    public void PressMeBaby()
    {
        animator.SetTrigger("Press");
    }
}
