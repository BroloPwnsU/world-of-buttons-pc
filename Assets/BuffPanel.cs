using UnityEngine;
using System.Collections;

public class BuffPanel : MonoBehaviour {

    public AudioClip BuffScreenMusic;

    private AudioSource _audioSource;
    private bool _previousEnable = false;

	// Use this for initialization
	void Awake () {
        _audioSource = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {

        bool bCurrentEnable = this.gameObject.activeSelf;
        if (!_previousEnable)
        {
            if (bCurrentEnable)
            {
                _audioSource.PlayOneShot(BuffScreenMusic, 1.0f);
            }
        }
        
        _previousEnable = bCurrentEnable;
    }
}
