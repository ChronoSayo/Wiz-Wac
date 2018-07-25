using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cryonis : Rune
{
    private GameObject _rampObject;
    private Transform _currentRamp, _oldRamp;
    private Vector3 _oldPosition, _oldForward;
    private Material _material;
    private bool _enabled;
    private State _state;
    private List<Transform> _ramps;

    private const int MAX_RAMPS = 10;

    private enum State
    {
        None, StandBy, Aim, Create, Rise, StandByDestroy, Destroy, Disable
    }
    
    void Start ()
    {
        _state = State.StandBy;

        _enabled = false;

        _ramps = new List<Transform>();

        _currentRamp = null;
    }

    void Update()
    {
        switch (_state)
        {
            case State.Rise:
                HandleRise();
                break;
        }
    }
    
    override public void Run ()
    {
        switch (_state)
        {
            case State.None:
                break;
            case State.StandBy:
                StandByStartRamp();
                StandByEnabled();
                break;
            case State.Aim:
                AimButtonAction();
                AimToSetRamp();

                _currentRamp.position = _oldPosition;
                RotateRamp();
                break;
            case State.Create:
                CreateRamp();
                _state = State.Rise;

                LimitRamps();
                break;
            case State.StandByDestroy:
                StandByModeForDestroyRamp();
                break;
            case State.Destroy:
                _ramps.Remove(_oldRamp);

                Destroy(_oldRamp.gameObject);
                _oldRamp = null;

                _state = State.Disable;
                break;
            case State.Disable:
                DestroyRamp();
                break;
        }
    }

    private void StandByStartRamp()
    {
        if (Input.GetKeyDown(_enableKey) || PlayerInput.UseRune)
        {
            _enabled = !_enabled;
            if (!enabled)
                _state = State.Disable;
            _icon.enabled = _enabled;
        }
    }

    private void StandByEnabled()
    {
        if (_enabled)
        {
            RaycastHit hit;
            if (Physics.Raycast(_mainCamera.position, _mainCamera.forward, out hit, 100))
            {
                if (OnFlatSurface(hit.normal) || hit.transform.tag == "Ramp")
                {
                    _state = State.Aim;
                    GameObject rampInstantiate = Instantiate(_rampObject, hit.point, Quaternion.Euler(45, 0, 0));
                    _currentRamp = rampInstantiate.transform;
                    _material = _currentRamp.GetComponent<Renderer>().material;
                    RotateRamp();

                    ShowRamp(false);
                    _enabled = false;
                }
            }
        }
    }

    private void AimButtonAction()
    {
        if (Input.GetKeyDown(_enableKey) || PlayerInput.AltRune)
            _state = State.Disable;
        if (Input.GetMouseButtonDown(0) || PlayerInput.UseRune)
            _state = State.Create;
    }

    private void AimToSetRamp()
    {
        RaycastHit hit;
        if (Physics.Raycast(_mainCamera.position, _mainCamera.forward, out hit, 100))
        {
            if (OnFlatSurface(hit.normal))
                _oldPosition = hit.point;

            if (hit.transform.tag == "Ramp")
                EnableStandByDestroy(hit.transform);

        }
    }

    private void StandByModeForDestroyRamp()
    {
        RaycastHit hit;
        if (Physics.Raycast(_mainCamera.position, _mainCamera.forward, out hit, 100))
        {
            if (hit.transform.tag != "Ramp")
                CancelStandByDestroy();
            else
            {
                if (PlayerInput.UseRune)
                    _state = State.Destroy;
            }
        }
        else
            CancelStandByDestroy();
    }

    private void LimitRamps()
    {
        if (_ramps.Count > MAX_RAMPS)
        {
            Destroy(_ramps[0].gameObject);
            _ramps[0] = null;
            _ramps.RemoveAt(0);
        }
    }

    private void RotateRamp()
    {
        float offset = 3;
        if (Vector3.Angle(Vector3.up, transform.forward) > 90 - offset
            && Vector3.Angle(Vector3.up, transform.forward) < 90 + offset)
        {
            _oldForward = transform.forward;
        }
        _currentRamp.forward = _oldForward;
        _currentRamp.Rotate(45, 0, 0);
    }

    private void CreateRamp()
    {
        _currentRamp.position = new Vector3(_currentRamp.position.x, _currentRamp.position.y - 3, _currentRamp.position.z);
        ShowRamp(true);
        _ramps.Add(_currentRamp);
    }

    private void HandleRise()
    {
        _currentRamp.position += Vector3.up * 10 * Time.deltaTime;
        if (_currentRamp.position.y > _oldPosition.y)
        {
            _currentRamp.position = new Vector3(_currentRamp.position.x, _oldPosition.y, _currentRamp.position.z);
            _state = State.StandBy;
        }
    }

    private void EnableStandByDestroy(Transform hit)
    {
        _state = State.StandByDestroy;
        _oldRamp = hit;
        if(_oldRamp != _currentRamp)
            ShowCurrentRamp(false);
        StandByDestroyRamp(true);
    }

    private void CancelStandByDestroy()
    {
        _state = State.Aim;
        ShowCurrentRamp(true);
        StandByDestroyRamp(false);
        _oldRamp = null;
    }

    private void ShowCurrentRamp(bool visible)
    {
        _currentRamp.GetComponent<Renderer>().enabled = visible;
    }

    private void StandByDestroyRamp(bool enable)
    {
        Color c = _oldRamp.GetComponent<Renderer>().material.color;
        _oldRamp.GetComponent<Renderer>().material.color = enable ? Color.red : 
            new Color(_material.color.r, _material.color.g, _material.color.b, 1);
    }

    private void ShowRamp(bool enable)
    {
        _currentRamp.GetComponent<Collider>().enabled = enable;
        Color c = _currentRamp.GetComponent<Renderer>().material.color;
        _currentRamp.GetComponent<Renderer>().material.color = new Color(c.r, c.g, c.b, enable ? 1 : 0.2f);
    }

    private bool OnFlatSurface(Vector3 hitNormal)
    {
        return Vector3.Angle(hitNormal, Vector3.up) == 0;
    }

    private void DestroyRamp()
    {
        if (_currentRamp)
        {
            Destroy(_currentRamp.gameObject);
            _currentRamp = null;
        }
        _state = State.StandBy;
    }

    public void SetRamp(GameObject ramp)
    {
        _rampObject = ramp;
    }

    public override void EnableRune()
    {
        base.EnableRune();
    }

    override public void Disable()
    {
        base.Disable();
        if(_state == State.Aim)
            DestroyRamp();
    }

    public override KeyCode EnableKey
    {
        set { base.EnableKey = value; }
        get { return base.EnableKey; }
    }

    public override void SetMainCamera(Transform camera)
    {
        base.SetMainCamera(camera);
    }

    override public void SetIcon(Image icon)
    {
        base.SetIcon(icon);
    }

    public override bool Unlocked
    {
        get
        {
            return base.Unlocked;
        }

        set
        {
            base.Unlocked = value;
        }
    }
}
