using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    public Sprite[] Sprites;
    public float RotationEulerAngle = 20;
    public float FallVectorDistance = 0.8f;

    private SpriteRenderer _spriteRenderer;
    private Vector3 _originPosition;
    private Vector3 _targetPosition;
    private Vector3 _fallVector;
    private Vector3 _fallPosition;
    private System.Action OnHit;
    private float _flightTime = 0.25f;
    private float _fallTime = 0.15f;
    private float _bounceTime = 0.15f;
    private float _deadTime = 3.0f;
    private float _timeElapsed = 0;
    private bool _flying = false;
    private bool _falling = false;
    private bool _bouncing = false;
    private bool _dead = false;
    private int _maxBounces = 0;
    private int _bounces = 0;

    private float _totalLifetime = 0;

	// Use this for initialization
	void Awake ()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _flying = false;
        _fallVector = new Vector3(FallVectorDistance, FallVectorDistance, 0.0f);
    }

    public void Destroy()
    {
        Debug.Log("Destorying");

        if (gameObject != null)
            Destroy(gameObject);
    }

    // Update is called once per frame
    void Update ()
    {
	    //fuck bitches get money, gee
        _timeElapsed += Time.deltaTime;

        _totalLifetime += Time.deltaTime;
        if (_totalLifetime > 10)
            this.Destroy();


        if (_flying)
        {
            UpdateFlight();
            if (_timeElapsed >= _flightTime)
            {
                //The dude has impacted.
                Fall();
            }
        }
        else if (_falling)
        {
            UpdateFall();
            if (_timeElapsed >= _fallTime)
            {
                //The dude has impacted.
                Bounce();
            }
        }
        else if (_bouncing)
        {
            UpdateBounce();
            if (_timeElapsed >= _bounceTime)
            {
                //The dude has impacted.
                Die();
            }
        }
        else if (_dead)
        {
            //When it's dead it doesn't move. But it fades...
            UpdateDead();
            if (_timeElapsed >= _deadTime)
            {
                //The dude has impacted.
                Destroy(this.gameObject);
            }
        }
	}

    public void Fly(Vector3 originPosition, Vector3 targetPosition, System.Action onHit)
    {
        //First pick a random sprite to use
        if (Sprites != null && Sprites.Length > 0)
        {
            int wRandomSpriteIndex = UnityEngine.Random.Range(0, Sprites.Length);
            _spriteRenderer.sprite = Sprites[wRandomSpriteIndex];
        }


        //Going to shoot this bitch down from the source location towards the evil bastard who deserves to die.
        //Need to know who to shoot at, and what his global position is.
        OnHit = onHit;
        _originPosition = originPosition;
        _targetPosition = targetPosition;
        _timeElapsed = 0.0f;
        _flying = true;
        _falling = false;
        _bouncing = false;
    }

    void UpdateFlight()
    {
        if (_timeElapsed > _flightTime)
            _timeElapsed = _flightTime;

        transform.position = GetFlyPositon(
            _timeElapsed,
            _flightTime,
            _originPosition,
            _targetPosition
            );

        transform.Rotate(new Vector3(0, 0, RotationEulerAngle));

        Debug.Log("Position: " + transform.position.x + ", " + transform.position.y + ", " + transform.position.z);
        Debug.Log("Local Position: " + transform.localPosition.x + ", " + transform.localPosition.y + ", " + transform.localPosition.z);
    }

    Vector3 GetFlyPositon(float timeElapsed, float flightTime, Vector3 startPoint, Vector3 endPoint)
    {
        float x = startPoint.x + (((endPoint.x - startPoint.x) / flightTime) * timeElapsed);
        float y = startPoint.y + (((endPoint.y - startPoint.y) / flightTime) * timeElapsed);
        float z = startPoint.z;
        
        return new Vector3(x, y, z);
    }

    void Fall()
    {
        OnHit();

        //Fall
        _flying = false;
        _falling = true;
        _bouncing = false;
        _timeElapsed = 0.0f;

        float xMultiplier = UnityEngine.Random.Range(0.8f, 1.2f);
        if (_targetPosition.x - _originPosition.x > 0)
            xMultiplier *= -1;

        float yMultiplier = -1 * UnityEngine.Random.Range(0.8f, 1.2f);

        _fallPosition = _targetPosition + new Vector3(_fallVector.x * xMultiplier, _fallVector.y * yMultiplier, _fallVector.z);
    }

    void UpdateFall()
    {
        if (_timeElapsed > _fallTime)
            _timeElapsed = _fallTime;
        
        transform.position = GetFlyPositon(
            _timeElapsed,
            _fallTime,
            _targetPosition,
            _fallPosition
            );

        transform.Rotate(new Vector3(0, 0, RotationEulerAngle));
    }

    Vector3 GetFallPositon(float timeElapsed, float flightTime, Vector3 startPoint, Vector3 endPoint)
    {
        float x = startPoint.x + (((endPoint.x - startPoint.x) / flightTime) * timeElapsed);
        float y = startPoint.y + (((endPoint.y - startPoint.y) / flightTime) * timeElapsed);
        float z = startPoint.z;

        return new Vector3(x, y, z);
    }

    void Bounce()
    {
        //Fall
        _flying = false;
        _falling = false;

        if (_maxBounces > _bounces)
        {
            _bouncing = true;
            _dead = false;
        }
        else
        {
            _bouncing = false;
            _dead = true;
        }
        _timeElapsed = 0.0f;
    }

    void UpdateBounce()
    {
        //Just don't do nothing. FUck it.
    }

    void Die()
    {
        //Fall
        _flying = false;
        _falling = false;
        _bouncing = false;
        _dead = true;
        _timeElapsed = 0.0f;
    }

    void UpdateDead()
    {
        //Fade over time. TBD.
    }
}
