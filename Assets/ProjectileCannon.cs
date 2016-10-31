using UnityEngine;
using System.Collections;

public class ProjectileCannon : MonoBehaviour
{
    public GameObject ProjectilePrefab;
    public AudioClip[] FireSounds;
    private AudioSource _audioSource;
    public float FireVolume = 1.0f;
    public float BulletScale = 600.0f;

	// Use this for initialization
	void Awake ()
    {
        _audioSource = GetComponent<AudioSource>();
	}

    public void Fire(Vector3 targetPosition, System.Action onHit)
    {
        PlayLaser();

        Vector3 thisPosition = new Vector3(0, 0, 0);
        Quaternion quaternion = new Quaternion(0, 0, 0, 0);

        Debug.Log("fire!");
        GameObject projectileClone = (GameObject)Instantiate(
            ProjectilePrefab,
            gameObject.transform.position, //transform.position, 
            quaternion,// transform.rotation
            transform
            );
        //projectileClone.transform.parent = gameObject.transform;

        projectileClone.transform.localScale = new Vector3(BulletScale, BulletScale, 1);

        Debug.Log("Origin Position: " + projectileClone.transform.position.x + ", " + projectileClone.transform.position.y + ", " + projectileClone.transform.position.z);
        Debug.Log("Origin Local Position: " + projectileClone.transform.localPosition.x + ", " + projectileClone.transform.localPosition.y + ", " + projectileClone.transform.localPosition.z);

        Debug.Log("Target Position: " + targetPosition.x + ", " + targetPosition.y + ", " + targetPosition.z);

        //We created the projectile. Now get it going!
        Projectile projectile = projectileClone.GetComponent<Projectile>();
        projectile.Fly(gameObject.transform.position, targetPosition, onHit);
    }

    void PlayLaser()
    {
        if (FireSounds != null && FireSounds.Length > 0)
        {
            _audioSource.PlayOneShot(
                FireSounds[UnityEngine.Random.Range(0, FireSounds.Length - 1)],
                FireVolume
                );
        }
    }
}
