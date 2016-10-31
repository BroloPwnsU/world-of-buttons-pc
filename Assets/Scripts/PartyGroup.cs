using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PartyGroup : MonoBehaviour
{
    #region Private Properties

    //There is a component called...
    private HealthBar _healthBar;
    private DamageSprite _damageSprite;
    private CritSprite _critSprite;
    private Text _healthText;
    private bool _bIsPVP = false;

    private AudioSource _audioSource;

    private float _timeElapsed = 0;
    private float _jumpDuration = 0.5f;
    private float _pressDuration = 0.01f;
    private Vector3 _characterSpriteEndPosition;

    private PlayerSpriteJumpState _currentJumpState;
    private PlayerSprite _activePlayerSprite;
    private Transform _activePlayerSpriteTransform;
    private Vector3 _activePlayerSpriteStartPosition;

    private Vector3 _PVP1StartPosition;
    private Vector3 _PVP2StartPosition;
    private Vector3 _PVE1StartPosition;
    private Vector3 _PVE2StartPosition;
    private Vector3 _PVE3StartPosition;
    private Vector3 _PVE4StartPosition;

    private PVPSprite _activeSpriteScript;
    private PVPSprite _pvp1;
    private PVPSprite _pvp2;

    private System.Action SendPressNotification;

    #endregion

    #region Public Properties

    public List<AudioClip> JumpSounds;
    public List<AudioClip> AttackSounds;
    public List<AudioClip> CritSounds;
    public List<AudioClip> FailSounds;

    public float JumpVolume = 0.5f;

    public GameObject PVESpritePanel;
    public GameObject PVPSpritePanel;

    public GameObject Round1VictorySprite;
    public GameObject Round2VictorySprite;
    public GameObject Round3VictorySprite;
    public GameObject Round4VictorySprite;

    public Material SpriteSubdueMaterial;
    public Material SpriteActiveMaterial;

    public float ButtonX = 0;
    public float ButtonY = 0;
    public float ButtonZ = 0;
    public float JumpYMultiplier = 1.0f;

    #endregion

    // Use this for initialization
    void Awake()
    {
        _healthBar = gameObject.GetComponentInChildren<HealthBar>(true);
        _damageSprite = gameObject.GetComponentInChildren<DamageSprite>(true);
        _critSprite = gameObject.GetComponentInChildren<CritSprite>(true);
        _audioSource = GetComponent<AudioSource>();

        //If we have to add multiple text boxes then we will need this script to sort through them and select the proper textbox
        Text[] textBoxes = gameObject.GetComponentsInChildren<Text>(true);
        if (textBoxes != null)
        {
            foreach (Text text in textBoxes)
            {
                if (text.gameObject.name == "PlayerHealthText")
                {
                    _healthText = text;
                    break;
                }
            }
        }
        
        PVPSprite[] pvpSprites = gameObject.GetComponentsInChildren<PVPSprite>(true);
        if (pvpSprites != null)
        {
            foreach (PVPSprite x in pvpSprites)
            {
                if (x.gameObject.name == "PVP1")
                {
                    _pvp1 = x;
                }
                else if (x.gameObject.name == "PVP2")
                {
                    _pvp2 = x;
                }
            }
        }

        _activeSpriteScript = null;
        _currentJumpState = PlayerSpriteJumpState.None;
        SetJumpStartPositions();
        _characterSpriteEndPosition = new Vector3(ButtonX, ButtonY, 1);
    }

    void Update()
    {
        if (_currentJumpState == PlayerSpriteJumpState.Attacking
            || _currentJumpState == PlayerSpriteJumpState.Retreating
            || _currentJumpState == PlayerSpriteJumpState.Pressing)
        {
            _timeElapsed += Time.deltaTime;

            _activePlayerSpriteTransform.position = GetJumpingSpritePositon(
                _currentJumpState,
                _timeElapsed,
                _jumpDuration,
                _activePlayerSpriteStartPosition,
                _characterSpriteEndPosition
                );

            if (_currentJumpState == PlayerSpriteJumpState.Attacking)
            {
                if (_timeElapsed >= _jumpDuration)
                {
                    //We've been standing still too long, return to the start position.
                    StartPressingTranslation();
                }
            }
            else if (_currentJumpState == PlayerSpriteJumpState.Pressing)
            {
                if (_timeElapsed >= _pressDuration)
                {
                    //We've been standing still too long, return to the start position.
                    StartRetreatJumpTranslation();
                }
            }
            else if (_currentJumpState == PlayerSpriteJumpState.Retreating)
            {
                if (_timeElapsed >= _jumpDuration)
                {
                    //We've been standing still too long, return to the start position.
                    StopJumpTranslation();
                }
            }
        }
    }

    public void PrepForBattle(bool bIsPVP, float StartingHealth, int wVictoriesNeededToWin, int wCurrentVictores)
    {
        _bIsPVP = bIsPVP;

        RefreshHealth(StartingHealth, StartingHealth);

        if (_bIsPVP)
        {
            PVESpritePanel.SetActive(true);
            PVPSpritePanel.SetActive(false);
        }
        else
        {
            PVESpritePanel.SetActive(false);
            PVPSpritePanel.SetActive(true);
        }

        if (wVictoriesNeededToWin <= 1)
        {
            //If the party only needs one victory to win, then we don't need to keep count of victories
            for (int i = 1; i <= 4; i++)
            {
                GetVictorySprite(i).SetActive(false);
            }
        }
        else
        {
            //We only support up to 3 victories per player. Because we're lazy coders
            for (int i = 1; i <= 4; i++)
            {
                if (i <= wVictoriesNeededToWin)
                {
                    //Show this sprite
                    GetVictorySprite(i).SetActive(true);

                    //Tell it whether to make it bright or make it boring.
                    if (i <= wCurrentVictores)
                    {
                        //The party has already won this many games. Make it shine!
                        GetVictorySprite(i).GetComponent<Renderer>().material = SpriteActiveMaterial;
                    }
                    else
                    {
                        //The party has yet to win this many games. Make it dull.
                        GetVictorySprite(i).GetComponent<Renderer>().material = SpriteSubdueMaterial;
                    }
                }
                else
                {
                    //Hide this sprite. It's more than we need.
                    GetVictorySprite(i).SetActive(false);
                }
            }
        }

        _currentJumpState = PlayerSpriteJumpState.None;
        //Reset jumper positions
        ResetAllJumpPositions();
    }

    GameObject GetVictorySprite(int i)
    {
        if (i == 1) return Round1VictorySprite;
        else if (i == 2) return Round2VictorySprite;
        else if (i == 3) return Round3VictorySprite;
        else if (i == 4) return Round4VictorySprite;

        return Round1VictorySprite;
    }

    public void RefreshHealth(float currentHP, float originalHP)
    {
        _healthText.text = ((int)currentHP).ToString();
        _healthBar.SetCurrentHealth(currentHP, originalHP);
    }

    public void ShowVictoryDance()
    {
        _pvp1.Win();
        _pvp2.Win();
    }

    public void MakeAttack(AttackStats attackStats, System.Action sendPressNotification)
    {
        SendPressNotification = sendPressNotification;

        //Attack consists of:
        //1a. One player jumps, translating in an arc towards a button in the center of the screen over the course of 0.5s
        //1b. The player sprite enters the jump animation. 
        //1c. A jump noise is played.
        //2a. The player sprite lands and stops translating.
        //2b. The player sprite transitions from jump animation to landing animation.
        //2c. A landing sound is played.
        //3a. The player jumps back, translating in an arc over 0.5s.
        //3b. The player sprite enters the jump animation.
        //3c. (No jump sound)
        //4a. The player sprite lands and stops translating.
        //4b. The player sprite transitions from jump animation to idle animation.

        PlayerSprite player;
        PVPSprite spriteScript;
        
        if (UnityEngine.Random.Range(0,2) == 1)
        {
            player = PlayerSprite.PVP1;
            spriteScript = _pvp1;
        }
        else
        {
            player = PlayerSprite.PVP2;
            spriteScript = _pvp2;
        }

        _activePlayerSprite = player;
        _activeSpriteScript = spriteScript;
        _activePlayerSpriteTransform = GetPlayerSpriteTransform(player);

        StartAttackJumpTranslation(player);
        PlayJumpNoise();
    }
    
    #region Jumper Position Accessors

    void SetJumpStartPositions()
    {
        foreach (Transform t in PVPSpritePanel.transform)
        {
            if (t.name == PVP1_NAME)
            {
                _PVP1StartPosition = t.position;
            }
            else if (t.name == PVP2_NAME)
            {
                _PVP2StartPosition = t.position;
            }
            else if (t.name == PVE1_NAME)
            {
                _PVE1StartPosition = t.position;
            }
            else if (t.name == PVE2_NAME)
            {
                _PVE2StartPosition = t.position;
            }
            else if (t.name == PVE3_NAME)
            {
                _PVE3StartPosition = t.position;
            }
            else if (t.name == PVE4_NAME)
            {
                _PVE4StartPosition = t.position;
            }
        }
    }

    void ResetAllJumpPositions()
    {
        foreach (Transform t in PVPSpritePanel.transform)
        {
            if (t.name == PVP1_NAME)
            {
                t.position = _PVP1StartPosition;
            }
            else if (t.name == PVP2_NAME)
            {
                t.position = _PVP2StartPosition;
            }
            else if (t.name == PVE1_NAME)
            {
                t.position = _PVE1StartPosition;
            }
            else if (t.name == PVE2_NAME)
            {
                t.position = _PVE2StartPosition;
            }
            else if (t.name == PVE3_NAME)
            {
                t.position = _PVE3StartPosition;
            }
            else if (t.name == PVE4_NAME)
            {
                t.position = _PVE4StartPosition;
            }
        }
    }

    Vector3 GetStartPosition(PlayerSprite player)
    {
        switch (player)
        {
            case PlayerSprite.PVP1:
                return _PVP1StartPosition;
            case PlayerSprite.PVP2:
                return _PVP2StartPosition;
            case PlayerSprite.PVE1:
                return _PVE1StartPosition;
            case PlayerSprite.PVE2:
                return _PVE2StartPosition;
            case PlayerSprite.PVE3:
                return _PVE3StartPosition;
            case PlayerSprite.PVE4:
                return _PVE4StartPosition;
            default:
                return Vector3.zero;
        }
    }

    Transform GetPlayerSpriteTransform(PlayerSprite player)
    {
        if (player == PlayerSprite.PVP1 || player == PlayerSprite.PVP2)
        {
            foreach (Transform t in PVPSpritePanel.transform)
            {
                if (player == PlayerSprite.PVP1 && t.name == PVP1_NAME)
                {
                    return t;
                }
                else if (player == PlayerSprite.PVP2 && t.name == PVP2_NAME)
                {
                    return t;
                }
            }
        }
        else if (player == PlayerSprite.PVE1 || player == PlayerSprite.PVE2 || player == PlayerSprite.PVE3 || player == PlayerSprite.PVE4)
        {
            foreach (Transform t in PVESpritePanel.transform)
            {
                if (player == PlayerSprite.PVE1 && t.name == PVE1_NAME)
                {
                    return t;
                }
                else if (player == PlayerSprite.PVE2 && t.name == PVE2_NAME)
                {
                    return t;
                }
                else if (player == PlayerSprite.PVE3 && t.name == PVE3_NAME)
                {
                    return t;
                }
                else if (player == PlayerSprite.PVE4 && t.name == PVE4_NAME)
                {
                    return t;
                }
            }
        }

        return null;
    }

    #endregion

    #region State transition

    void StartAttackJumpTranslation(PlayerSprite player)
    {
        StartJumpAnimation(_activeSpriteScript);

        Transform playerSpriteTransform = GetPlayerSpriteTransform(player);
        if (playerSpriteTransform != null)
        {
            _activePlayerSpriteStartPosition = GetStartPosition(player);
            _currentJumpState = PlayerSpriteJumpState.Attacking;
            _timeElapsed = 0;
        }
    }

    void StartRetreatJumpTranslation()
    {
        _currentJumpState = PlayerSpriteJumpState.Retreating;
        _timeElapsed = 0;

        //StartJumpAnimation(_activeSpriteScript);
    }

    void StartPressingTranslation()
    {
        _timeElapsed = 0;
        _currentJumpState = PlayerSpriteJumpState.Pressing;

        //EndJumpAnimation(_activeSpriteScript);
        SendPressNotification();
    }

    void StopJumpTranslation()
    {
        _timeElapsed = 0;
        _currentJumpState = PlayerSpriteJumpState.None;

        EndJumpAnimation(_activeSpriteScript);
        //SendPressNotification();
    }

    #endregion

    #region Jump Translations

    Vector3 GetJumpingSpritePositon(PlayerSpriteJumpState jumpState, float timeElapsed, float jumpDuration, Vector3 currentStartPosition, Vector3 buttonPosition)
    {
        if (jumpState == PlayerSpriteJumpState.None)
        {
            //Don't do anything. No jump is happening. This should never even be called.
            return currentStartPosition;
        }
        else if (jumpState == PlayerSpriteJumpState.Pressing)
        {
            //He's standing on the button for just a split second.
            //Just have him hold the standing point.

            return buttonPosition;
        }
        else
        {
            if (timeElapsed > jumpDuration)
            {
                timeElapsed = jumpDuration;
            }

            //Jumping, either towards or away. Render the current position.
            Vector3 startPoint;
            Vector3 endPoint;

            if (jumpState == PlayerSpriteJumpState.Attacking)
            {
                startPoint = currentStartPosition;
                endPoint = buttonPosition;
            }
            else
            {
                startPoint = buttonPosition;
                endPoint = currentStartPosition;
            }

            float x = startPoint.x + (((endPoint.x - startPoint.x) / jumpDuration) * timeElapsed);
            float y = startPoint.y + (((endPoint.y - startPoint.y) / jumpDuration) * timeElapsed);
            float z = startPoint.z;


            //Y bonus parabola
            float ybonus = 1 - ((4 / (jumpDuration * jumpDuration)) * Mathf.Pow((timeElapsed - (jumpDuration / 2)), 2));
            ybonus *= JumpYMultiplier;
            return new Vector3(x, y + ybonus, z);
        }
    }

    #endregion

    #region Sprite Animations

    void StartJumpAnimation( PVPSprite spriteScript)
    {
        //Doesn't do shit. Need to add this once we have created a jump animation clip.
        spriteScript.Jump();
    }

    void EndJumpAnimation(PVPSprite spriteScript)
    {
        spriteScript.Land();
    }

    #endregion

    #region Noises

    void PlayJumpNoise()
    {
        if (JumpSounds != null && JumpSounds.Count > 0)
        {
            _audioSource.PlayOneShot(
            JumpSounds[Random.Range(0, JumpSounds.Count - 1)],
            JumpVolume
            );
        }
    }

    void PlayAttackSound(bool bCrit)
    {
        if (bCrit && CritSounds != null && CritSounds.Count > 0)
        {
            _audioSource.PlayOneShot(
                CritSounds[Random.Range(0, CritSounds.Count - 1)],
                0.5f
                );
        }
        else if (AttackSounds != null && AttackSounds.Count > 0)
        {
            _audioSource.PlayOneShot(
                AttackSounds[Random.Range(0, AttackSounds.Count - 1)],
                0.5f
                );
        }
    }

    void PlayFailSound()
    {
        if (FailSounds != null && FailSounds.Count > 0)
        {
            _audioSource.PlayOneShot(
                FailSounds[Random.Range(0, FailSounds.Count - 1)],
                0.5f
                );
        }
    }

    private PlayerSprite _victim = PlayerSprite.PVP1;

    public void ChooseNewVictim()
    {
        if (UnityEngine.Random.Range(0, 2) == 1)
        {
            _victim = PlayerSprite.PVP1;
        }
        else
        {
            _victim = PlayerSprite.PVP2;
        }
    }

    public Vector3 GetVictimPosition()
    {
        if (_victim == PlayerSprite.PVP1)
        {
            return _PVP1StartPosition;
        }
        else
        {
            return _PVP2StartPosition;
        }
    }
    
    public Vector3 GetVictimHeadPosition()
    {
        if (_victim == PlayerSprite.PVP1)
        {
            return _PVP1StartPosition + new Vector3(0, 0.8f, 0);
        }
        else
        {
            return _PVP2StartPosition + new Vector3(0, 0.8f, 0);
        }
    }

    public void TakeDamage(float damage, float newCurrentHP, float originalHP, bool bCrit, bool bSelfAttack)
    {
        _healthBar.TakeDamage(damage, false, newCurrentHP, originalHP);

        /*
        if (bCrit)
            _critSprite.Show();
        else
            _damageSprite.Show();
        */

        _healthText.text = ((int)newCurrentHP).ToString();

        if (bSelfAttack)
        {
            PlayFailSound();
        }
        else
        {
            PlayAttackSound(bCrit);
        }

        //Now show the damage animation
        if (newCurrentHP <= 0)
        {
            //If they're dead, play the dead animation
            _pvp1.Die();
            _pvp2.Die();
        }
        else
        {
            if (bSelfAttack)
            {
                _pvp1.TakeDamage();
                _pvp2.TakeDamage();
            }
            //If they're still alive, just make them jiggle a little bit
            if (_victim == PlayerSprite.PVP1)
               _pvp1.TakeDamage();
            else
               _pvp2.TakeDamage();
        }
    }

    #endregion

    #region Utility Enums and Consts

    public enum PlayerSprite
    {
        PVP1,
        PVP2,
        PVE1,
        PVE2,
        PVE3,
        PVE4
    }

    public enum PlayerSpriteJumpState
    {
        None,
        Attacking,
        Retreating,
        Pressing
    }

    const string PVP1_NAME = "PVP1";
    const string PVP2_NAME = "PVP2";
    const string PVE1_NAME = "PVE1";
    const string PVE2_NAME = "PVE2";
    const string PVE3_NAME = "PVE3";
    const string PVE4_NAME = "PVE4";

    #endregion
}