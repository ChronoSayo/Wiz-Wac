using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameplayHandler : MonoBehaviour
{
    private Link _playerScript;
    private Cinematic _cinematic;
    private Transform _player;
    private GameObject _finalShrine;
    private Text _shrineName;
    private float _alphaSpeed;
    private State _state;

    public static bool TRAVELED;
    public static string OVERWORLDLEVEL = "hyoctrule";
    public static string BOSSLEVEL = "final";
    public static bool IS_OVERWORLD;
    public static bool IS_BOSS;
    public static Text LOAD_TEXT;

    private enum State
    {
        None, Gameplay, Cinematic
    }

    void Awake()
    {
        IS_OVERWORLD = SceneManager.GetActiveScene().name == OVERWORLDLEVEL;
        IS_BOSS = SceneManager.GetActiveScene().name == BOSSLEVEL;

        LOAD_TEXT = GameObject.Find("Loading").GetComponent<Text>();
        LOAD_TEXT.gameObject.SetActive(false);
    }

    void Start ()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _playerScript = _player.GetComponent<Link>();

        _shrineName = GameObject.Find("Name").GetComponent<Text>();

        HandleCameras();

        if (TRAVELED)
            _playerScript.EnterCinematic = true;
        _state = State.Gameplay;

        AssignPosition();

        StartCoroutine(DelayFogSettings());

        HandleShowShrineName();

        StartCoroutine(CheckFinalShrine());

    }

    private void HandleCameras()
    {
        if (IS_OVERWORLD)
        {
            if (Boss.DEFEATED)
            {
                GameObject.Find("FreeLookCameraRig").SetActive(false);
                GameObject.FindGameObjectWithTag("EndingCamera").SetActive(true);
                _playerScript.ControlCar = false;
                _player.gameObject.SetActive(false);
            }
            else
                GameObject.FindGameObjectWithTag("EndingCamera").SetActive(false);
        }
    }

    private void AssignPosition()
    {
        if (EnterShrine.LOAD_POSITION != Vector3.zero && SceneManager.GetActiveScene().name == OVERWORLDLEVEL)
        {
            _player.position = EnterShrine.LOAD_POSITION;
            _playerScript.StartPositionOverworld = _player.position;
        }
    }

    private void HandleShowShrineName()
    {
        if (!IS_OVERWORLD)
        {
            _alphaSpeed = 0;
            StartCoroutine(AddAlphaSpeed());
        }
        else
            _shrineName.gameObject.SetActive(false);
    }

    private IEnumerator CheckFinalShrine()
    {
        yield return new WaitForEndOfFrame();
        _finalShrine = GameObject.Find("FinalShrine");
        if (_finalShrine)
            _finalShrine.SetActive(RuneHandler.RUNES_UNLOCKED);
    }

    private IEnumerator DelayFogSettings()
    {
        yield return new WaitForEndOfFrame();

        float fog = 700;
        RenderSettings.fogEndDistance = RuneHandler.RUNES_UNLOCKED ? fog * 2 : fog;
    }
    
    void Update ()
    {
        PlayerInput.SimulateKeyPress();

        if (Cursor.lockState != CursorLockMode.Locked)
            Cursor.lockState = CursorLockMode.Locked;

        if (SceneManager.GetActiveScene().name != OVERWORLDLEVEL || _shrineName.color.a <= 0)
            ShowShrineName();

        switch (_state)
        {
            case State.None:
                break;
            case State.Gameplay:
                if (_playerScript.EnterCinematic)
                {
                    _state = State.Cinematic;
                    Transform trigger = _playerScript.GetCinematicTrigger;
                    if (!trigger)
                        trigger = transform;
                    if (_playerScript.StartTeleport)
                        trigger = _player;
                    _cinematic = trigger.GetComponent<Cinematic>();
                    _cinematic.Begin();
                    _playerScript.ControlCar = false;
                }
                break;
            case State.Cinematic:
                if (_cinematic.End)
                {
                    _state = State.Gameplay;
                    _playerScript.ControlCar = true;
                    _playerScript.EnterCinematic = false;
                }
                break;
        }
    }

    private void ShowShrineName()
    {
        _shrineName.color -= new Color(0, 0, 0, _alphaSpeed);
    }

    private IEnumerator AddAlphaSpeed()
    {
        yield return new WaitForSeconds(5);
        _alphaSpeed = 0.005f;
    }
}
