using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private MoveSettings[] _settings = null;
    [SerializeField] private CinemachineVirtualCamera vcam;
    [SerializeField] private GameObject _spray;

    private Vector3 _moveDirection;
    public CharacterController _controller;

    [Header("Idle cam values")]
    public float idleCamNoiseAmplitude;
    public float idleCamNoiseFrequency;

    [Header("Walking cam values")]
    public float walkingCamNoiseAmplitude;
    public float walkingCamNoiseFrequency;

    [Header("Running cam values")]
    public float runningCamNoiseAmplitude;
    public float runningCamNoiseFrequency;

    [Header("Holding Camera cam values")]
    public float holdingCamNoiseAmplitude;
    public float holdingCamNoiseFrequency;

    private bool isIdle = true;
    private bool isRunning = false;
    private float _staminaTimer = 5;
    private bool canRun = true;

    public static bool falling = false;

    //VAR SOUND

    private enum CURRENT_TERRAIN { WOOD, CONCRETE, GRASS, CARPET}
    private float walkFtpTimer = 0.67f;
    private float runFtpTimer = 0.34f;
    private float ftpTimer = 0.0f;

    [Header("Sound")]
    [SerializeField] private CURRENT_TERRAIN currentTerrain;
    [SerializeField] private AK.Wwise.Event ftpsEvent;
    [SerializeField] private AK.Wwise.Switch[] terrainSwitch;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();

        //player is idle on awake
        vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = idleCamNoiseAmplitude;
        vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = idleCamNoiseFrequency;
    }

    // Update is called once per frame
    private void Update()
    {
        checkTerrain();

        if (CameraManager.instance.canPlay)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                    if (_staminaTimer > 0 && canRun)
                    {
                        isRunning = true;
                        RunningMovement();
                        _staminaTimer -= Time.deltaTime;
                    }
                    else
                    {
                        canRun = false;
                        isRunning = false;
                        DefaultMovement();
                    }              
            }
            else
            {
                isRunning = false;
                DefaultMovement();
            }

            if (CameraManager.instance._isCameraUp)
            {
                HoldingCamMovement();
            }

            CamMouvement();
        }

        else
        {
            isIdle = true;
            MovementForbidden();
        }

        ftpTimer += Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (!falling)
        {
            _controller.Move(_moveDirection * Time.deltaTime);
        }

    }

    private void DefaultMovement()
    {
        if(CameraManager.instance.canPlay)
        {
            if (_controller.isGrounded)
            {
                Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")); //inputs

                if (input.x != 0 && input.y != 0)
                {
                    input *= 0.777f; //Normalizes Vector2 "input". Prevent the player from being faster if walking diagonally
                }

                if (input.x != 0 || input.y != 0)
                {
                    isIdle = false;

                    if (ftpTimer > walkFtpTimer)
                    {
                        SelectAndPlayFootstep();
                        ftpTimer = 0.0f;
                    }
                }
                else
                {
                    isIdle = true;
                }

                _moveDirection.x = input.x * _settings[0].speed;
                _moveDirection.z = input.y * _settings[0].speed;
                _moveDirection.y = -_settings[0].speed;

                _moveDirection = transform.TransformDirection(_moveDirection);
            }
            else
            {
                _moveDirection.y -= _settings[0].gravity * Time.deltaTime;
            }

            if (_staminaTimer <= 5.1) 
            {
                _staminaTimer += Time.deltaTime;
            }
            if (_staminaTimer >= 5.0) 
            {
                canRun = true;
            }
        }

        else
        {
            isIdle = true;      
        }
    }

    private void RunningMovement()
    {
        if (_controller.isGrounded)
        {
            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")); //inputs

            if (input != Vector2.zero)
            {
                if (input.x != 0 && input.y != 0)
                {
                    input *= 0.777f; //Normalizes Vector2 "input". Prevent the player from being faster if walking diagonally
                }

                _moveDirection.x = input.x * _settings[1].speed;
                _moveDirection.z = input.y * _settings[1].speed;
                _moveDirection.y = -_settings[1].speed;

                _moveDirection = transform.TransformDirection(_moveDirection);

                if (ftpTimer > runFtpTimer)
                {
                    SelectAndPlayFootstep();
                    ftpTimer = 0.0f;
                }
            }
        }
        else
        {
            _moveDirection.y -= _settings[1].gravity * Time.deltaTime;
        }
    }

    private void HoldingCamMovement()
    {
        if (_controller.isGrounded)
        {
            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")); //inputs

            if (input.x != 0 && input.y != 0)
            {
                input *= 0.777f; //Normalizes Vector2 "input". Prevent the player from being faster if walking diagonally
            }

            _moveDirection.x = input.x * _settings[2].speed;
            _moveDirection.z = input.y * _settings[2].speed;
            _moveDirection.y = -_settings[2].speed;

            _moveDirection = transform.TransformDirection(_moveDirection);


        }
        else
        {
            _moveDirection.y -= _settings[2].gravity * Time.deltaTime;
        }
    }

    private void MovementForbidden()
    {
        vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = idleCamNoiseAmplitude;
        vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = idleCamNoiseFrequency;

        if (_controller.isGrounded)
        {
            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")); //inputs

            if (input.x != 0 && input.y != 0)
            {
                input *= 0.777f; //Normalizes Vector2 "input". Prevent the player from being faster if walking diagonally
            }

            _moveDirection.x = input.x * _settings[3].speed;
            _moveDirection.z = input.y * _settings[3].speed;
            _moveDirection.y = -_settings[3].speed;

            _moveDirection = transform.TransformDirection(_moveDirection);
        }
        else
        {
            _moveDirection.y -= _settings[3].gravity * Time.deltaTime;
        }
    }

    public void CamMouvement() //Change the cinemachine virtual camera's noise according to the player's sate (idle, walking, running)
    {
        if(!isIdle && !isRunning &&!CameraManager.instance._isCameraUp)
        {
            //adjust frequency and amplitude when the player is walking
            vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = walkingCamNoiseAmplitude;
            vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = walkingCamNoiseFrequency;
        }

        else if (isRunning && !isIdle && !CameraManager.instance._isCameraUp)
        {
            //adjust frequency and amplitude when the player is running
            vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = runningCamNoiseAmplitude;
            vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = runningCamNoiseFrequency;
        }

        else if (CameraManager.instance._isCameraUp)
        {
            //adjust frequency and amplitude when the player is holding the camera
            vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = holdingCamNoiseAmplitude;
            vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = holdingCamNoiseFrequency;
        }

        else if(isIdle)
        {
            //adjust frequency and amplitude when the player is idle
            vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = idleCamNoiseAmplitude;
            vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = idleCamNoiseFrequency;
        }
    }

    private void checkTerrain() 
    {
        RaycastHit[] hit;

        hit = Physics.RaycastAll(transform.position, Vector3.down, 2.5f);

        foreach (RaycastHit rayhit in hit) 
        {
            if (rayhit.transform.gameObject.layer == LayerMask.NameToLayer("Wood"))
            {
                currentTerrain = CURRENT_TERRAIN.WOOD;
            }
            else if (rayhit.transform.gameObject.layer == LayerMask.NameToLayer("Concrete"))
            {
                currentTerrain = CURRENT_TERRAIN.CONCRETE;
            }
            else if (rayhit.transform.gameObject.layer == LayerMask.NameToLayer("Grass"))
            {
                currentTerrain = CURRENT_TERRAIN.GRASS;
            }
            else if (rayhit.transform.gameObject.layer == LayerMask.NameToLayer("Carpet"))
            {
                currentTerrain = CURRENT_TERRAIN.CARPET;
            }
        }
    }

    private void playFootstep(int terrain) 
    {
        terrainSwitch[terrain].SetValue(this.gameObject);
        AkSoundEngine.PostEvent(ftpsEvent.Id, this.gameObject);
    }

    public void SelectAndPlayFootstep() 
    {
        switch (currentTerrain) 
        {
            case CURRENT_TERRAIN.WOOD:
                playFootstep(0);
                break;
            case CURRENT_TERRAIN.CONCRETE:
                playFootstep(1);
                break;
            case CURRENT_TERRAIN.GRASS:
                playFootstep(2);
                break;
            case CURRENT_TERRAIN.CARPET:
                break;
        }
    }
}
