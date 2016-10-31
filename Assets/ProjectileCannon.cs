using UnityEngine;
using System.Collections;

public class ProjectileCannon : MonoBehaviour
{
    public GameObject ProjectilePrefab;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Fire(Vector3 targetPosition, System.Action onHit)
    {
        Vector3 thisPosition = new Vector3(0, 0, 0);
        Quaternion quaternion = new Quaternion(0, 0, 0, 0);

        Debug.Log("fire!");
        GameObject projectileClone = (GameObject)Instantiate(
            ProjectilePrefab,
            thisPosition, //transform.position, 
            quaternion// transform.rotation
            );
        projectileClone.transform.parent = gameObject.transform;

        //We created the projectile. Now get it going!
        Projectile projectile = projectileClone.GetComponent<Projectile>();
        projectile.Fly(thisPosition, targetPosition, onHit);
    }
}
