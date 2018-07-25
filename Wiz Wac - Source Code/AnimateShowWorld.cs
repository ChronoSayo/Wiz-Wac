using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Cameras;

public class AnimateShowWorld : Cinematic
{
    public Transform mainCamera;
    public GameObject minimap;
    
    private GameObject _fuelMeter, _dirLight;
    private Transform _fakeLight;
    private Image _title;
    private Vector3 _lookAt, _zoomOut, _panEnd, _carPos, _introPos;
    private Vector3 _newStartPosition;

    private static bool WATCHED;

    void Start ()
    {
        Init();

        _state = State.None;

        foreach(Transform t in transform)
        {
            if (t.name == "LookAt")
                _lookAt = t.position;
            else if (t.name == "ZoomOut")
                _zoomOut = t.position;
            else if (t.name == "PanEnd")
                _panEnd = t.position;
            else if (t.name == "CarPos")
                _carPos = t.position;
            else if (t.name == "IntroPos")
                _introPos = t.position;
            else if (t.name == "FakeLight")
                _fakeLight = t;
        }

        _fuelMeter = GameObject.Find("FuelBG");
        _dirLight = GameObject.Find("Directional Light");
        _title = GameObject.Find("Title").GetComponent<Image>();

        _newStartPosition = GameObject.Find("NewStartPosition").transform.position;

        if (WATCHED)
        {
            Destroy(gameObject);
            _player.GetComponent<Link>().StartPositionOverworld = _newStartPosition;

            if (GameplayHandler.IS_OVERWORLD && !Boss.DEFEATED)
            {
                minimap.SetActive(true);
                RenderSettings.fog = true;
            }
            else
            {
                minimap.SetActive(false);
                RenderSettings.fog = false;
            }
        }
        else
        {
            minimap.SetActive(false);
            _dirLight.SetActive(false);
        }
    }
    
    void FixedUpdate ()
    {
        switch(_state)
        {
            case State.None:
                break;
            case State.Start:
                MoveCamera(_introPos, 4);
                MoveCar();
                MoveLookAt();
                break;
            case State.Middle1:
                MoveCamera(_zoomOut, 65);
                MoveCar();
                break;
            case State.Middle2:
                MoveCamera(_panEnd, 20.5f);
                MoveLookAt();

                bool show = true;
                if (Vector3.Distance(mainCamera.position, _panEnd) < 100)
                    show = false;
                HandleTitle(show);
                break;
            case State.Middle3:
                MoveCamera(mainCamera.parent.position + Vector3.up * 2, 50);
                MoveLookAt();
                break;
            case State.End:
                EndCinematic();
                break;
        }
    }

    private IEnumerator NextState()
    {
        //Start
        yield return new WaitForSeconds(4);
        _state++;
        //Middle1
        yield return new WaitForSeconds(17);
        _state++;
        //Middle2
        yield return new WaitForSeconds(16.5f);
        _state++;
        //Middle3
        yield return new WaitForSeconds(20.5f);
        _state++;
    }

    private void HandleTitle(bool show)
    {
        float speed = 0.005f;
        if (!show)
            speed *= -1;
        
        _title.color += new Color(0, 0, 0, speed);

        Color c = _title.color;
        c.a = Mathf.Clamp(c.a, 0, 1);
        _title.color = c;
    }

    private void MoveLookAt()
    {
        if (_state == State.Start)
            _lookAt = Vector3.Slerp(_lookAt, _player.position, 1 * Time.deltaTime);
        else
            _lookAt += (_player.position - _lookAt) * 0.1f * Time.deltaTime;
    }

    private void MoveCar()
    {
        _player.LookAt(_carPos);
        _player.position += (_carPos - _player.position) * 0.3f * Time.deltaTime;
    }

    private void MoveCamera(Vector3 destination, float speed)
    {
        mainCamera.position += (destination - mainCamera.position).normalized * speed * Time.deltaTime;
        mainCamera.rotation = Quaternion.LookRotation(_lookAt - mainCamera.position);
    }

    private void EndCinematic()
    {
        FreeLookCam cam = _player.GetComponent<Link>().cameraRig.GetComponent<FreeLookCam>();
        cam.ResetCamera();

        _fuelMeter.SetActive(true);
        minimap.SetActive(true);
        RenderSettings.fog = true;

        mainCamera.parent.rotation = Quaternion.Euler(Vector3.zero);
        mainCamera.rotation = mainCamera.parent.rotation;
        mainCamera.position = mainCamera.parent.position + (Vector3.up * 2);

        Destroy(_title.gameObject);
        
        WATCHED = true;

        ShowMessage.GetInstance.PlayMessage(
            "Find the four Shrines on this plateau and acquire the containing Runes to venture beyond.", 15);

        _player.GetComponent<Link>().ReplaceStartPosition = _newStartPosition;
        _player.GetComponent<LocationText>().ShowPlateauName();

        Tutorial.GetInstance.Resume();

        Destroy(gameObject);
    }

    public override void Begin()
    {
        if (WATCHED)
        {
            _state = State.None;
            return;
        }

        base.Begin();

        Link playerScript = _player.GetComponent<Link>();
        playerScript.cameraRig.GetComponent<FreeLookCam>().Cinematic = true;
        playerScript.TurnOffFireEffects();

        RenderSettings.fog = false;
        transform.GetComponent<Renderer>().enabled = false;

        minimap.SetActive(false);
        _fuelMeter.SetActive(false);
        _dirLight.SetActive(true);

        Destroy(_fakeLight.gameObject);

        AudioComponent.GetInstance.PlayIntro();
        Tutorial.GetInstance.Pause();

        StartCoroutine(NextState());
    }

    public override bool End
    {
        get
        {
            return base.End;
        }
    }
}
