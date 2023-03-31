using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour

{
    public static CameraManager instance { get; private set; }
    private int _screenNumber = 0;


    public bool _isCameraUp = false;

    public bool _isUIup = false;

    public bool _takingPhoto = false;

    public GameObject UI;

    public MeshRenderer MeshRenderer;
    public Material _whiteMaterial;
    public Material _blackMaterial;
    public Texture image;

    public List<Texture2D> _takenPictures = new List<Texture2D>();
    public List<Sprite> _spriteList = new List<Sprite>();

    public List<MeshRenderer> _photoPlanes = new List<MeshRenderer>();
    public Texture2D test;

    public RenderTexture rt;

    public GameObject _porte;

    public GameObject _objetImportant;

    public Animator _camera;

    public Animator _cameraUI;

    public GameObject _lightCamera;

    [SerializeField] GameObject _cameraUIParent;

    public bool canPlay;

    int pageNumber = 0;


    [Header("References")]
    [SerializeField] GalleryManager galleryManager;


    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(gameObject);    // Suppression d'une instance pr�c�dente

        instance = this;
    }
    private void Start()
    {
        canPlay = true;
        /*if(test != null)
        {
            Debug.Log("J'ai un truc");
        }
        else
        {
            Debug.Log("J'ai rien trouv�");
        }*/

        test = Resources.Load<Texture2D>("capture0");
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(1) && !_isUIup) 
        {
            if (!_isCameraUp)
            {
                CameraUp();
                _lightCamera.SetActive(true);
            }
            else
            {
                CameraDown();
                _lightCamera.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.E) && !_isCameraUp)
        {
            if (!_isUIup)
            {

                _lightCamera.SetActive(false);
                UIup();
            }
            else
            {

                UIdown();
            }
        }
        //CameraUp();
        //ChangeMaterial();
        TakePhoto();
    }

    private void FixedUpdate()
    {
        
    }

    private void CameraUp()
    {
        _isCameraUp = true;
        _camera.SetTrigger("camera_activation");
        //UI.SetActive(true);
    }
    private void CameraDown()
    {
        _isCameraUp = false;
        _camera.SetTrigger("camera_desactivation");
        //UI.SetActive(false);
    }

    private void UIup()
    {
        //Chargement des images de gallerie
        galleryManager.OnGalleryUpdatePage();

        _isCameraUp = false;
        canPlay = false;
        _isUIup = true;
        _cameraUI.SetTrigger("UICameraUp");

    }

    private void UIdown()
    {

        canPlay = true;
        _isUIup = false;
        _cameraUI.SetTrigger("UICameraDown");

    }

    private void TakePhoto()
    {
        //Possibilit� de mettre une condition pour limiter le nombre de prise de photo avec _screenNumber
        if (Input.GetMouseButtonDown(0) && _isCameraUp)
        {
            _takingPhoto = true;

            Raycast();

            SaveRenderTextureToFile.SaveRTToFile(rt, _screenNumber);
            Texture2D temp = Resources.Load<Texture2D>("capture" + _screenNumber);
            Sprite _temp = Sprite.Create(SaveRenderTextureToFile.tex, new Rect(0, 0, SaveRenderTextureToFile.tex.width, SaveRenderTextureToFile.tex.height), new Vector2(0.5f, 0.5f));
            //_takenPictures.Add(SaveRenderTextureToFile.tex);
            _spriteList.Add(_temp);
            _screenNumber++;


            //photo1.sprite = _spriteList[0 % 4 ];



            /*
            if(_spriteList.Count > 1)
                photo2.sprite = _spriteList[_screenNumber % 4];
            if (_spriteList.Count > 2)
                photo3.sprite = _spriteList[_screenNumber % 4 ];
            if (_spriteList.Count > 3)
                photo4.sprite = _spriteList[_screenNumber % 4 ];
            */
        }

    }

    public void Raycast()
    {
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward), out hit, 50f))
        {
            
            //Debug.Log("Did Hit");
            if (hit.transform.tag == "_photoImportante" && _takingPhoto)
            {
                Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward) * hit.distance, Color.green);
                Debug.Log("Je d�tecte quelque chose d'important");

                //MeshRenderer.GetComponent<MeshRenderer>().material = _blackMaterial;
                _takingPhoto = false;
                _objetImportant.SetActive(false);
                _porte.SetActive(false);
            }

            else
            {
                Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            }
        }
        else
        {
            Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward) * 1000, Color.red);
            Debug.Log("Did not Hit");
            if (_takingPhoto)
            {
                //MeshRenderer.GetComponent<MeshRenderer>().material = _whiteMaterial;
                _takingPhoto = false;
            }
        }
    }


}
