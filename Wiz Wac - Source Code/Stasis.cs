using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stasis : Rune
{
    private GameObject _stasisArrowObject;
    private Transform _stasisArrow, _boss;
    private Rigidbody _rigidbody;
    private RectTransform _stasisCooldown;
    private Material _material;
    private Vector3 _chargedDir, _storedVelocity;
    private float _force, _saveSpeed;
    private bool _stasisLaunching;
    private State _state;
    private List<Transform> _allCarParts;
    private List<Material> _defaultMaterials;
    private List<ParticleSystem> _particleEffects;

    private enum State
    {
        None, StandBy, Charge, Release, Cancel, Cooldown
    }   

    void Start ()
    {
        _state = State.StandBy;

        _rigidbody = GetComponent<Rigidbody>();

        _particleEffects = new List<ParticleSystem>();
        _defaultMaterials = new List<Material>();
        _allCarParts = new List<Transform>();
        _allCarParts.Add(transform);
        _defaultMaterials.Add(GetComponent<Renderer>().material);
        foreach (Transform t in transform)
        {
            if (t.GetComponent<ParticleSystem>())
            {
                _particleEffects.Add(t.GetComponent<ParticleSystem>());
                continue;
            }
            _allCarParts.Add(t);
            _defaultMaterials.Add(t.GetComponent<Renderer>().material);
        }
    }

    void Update()
    {
        switch(_state)
        {
            case State.Cooldown:
                if (_stasisLaunching && !GetComponent<Link>().IsAirborne)
                {
                    StopCoroutine(LockAirMovement(0));
                    _stasisLaunching = false;
                }
                if (Input.GetKeyDown(EnableKey))
                {
                    _stasisCooldown.gameObject.SetActive(!_icon.enabled);
                    _icon.enabled = !_icon.enabled;
                }

                _stasisCooldown.localScale -= new Vector3((Time.deltaTime / _cooldownTime), 0);
                _cooldownX = _stasisCooldown.localScale.x;

                if (_cooldownX <= 0 || _finishCooldown)
                {
                    _state = State.StandBy;

                    _cooldownX = 1;
                    _stasisCooldown.localScale = new Vector3(_cooldownX, _cooldownY);
                    _stasisCooldown.gameObject.SetActive(false);
                }
                break;
        }
    }

    public override void Run()
    {
        switch(_state)
        {
            case State.None:
                break;
            case State.StandBy:
                if (Input.GetKeyDown(EnableKey) || PlayerInput.UseRune)
                    ExecuteCharging();
                break;
            case State.Charge:
                if (Input.GetKeyDown(EnableKey) || PlayerInput.TurboHold)
                {
                    if (_force == 0)
                    {
                        Revert();
                        StartCoroutine(AddForce(_storedVelocity));
                    }
                    else
                        _state = State.Release;
                }

                if (PlayerInput.UseRune)
                    AddForceDirection(_mainCamera.forward);
                if(PlayerInput.AltRune)
                {
                    _force--;
                    if (_force > 0)
                    {
                        _chargedDir = _mainCamera.forward;
                        _stasisArrow.forward = _chargedDir;
                        _stasisArrow.localScale = new Vector3(1, 1, _force);
                        ChangeArrowColor(true);
                    }
                    else if(_force <= 0)
                    {
                        if (_stasisArrow.gameObject.activeSelf)
                            _stasisArrow.gameObject.SetActive(false);
                        _force = 0;
                    }
                }
                break;
            case State.Release:
                Revert();

                Vector3 forceDir = (_chargedDir * _force);
                GetComponent<Link>().AirDirection = forceDir.normalized;
                GetComponent<Link>().Speed = forceDir.magnitude;

                _stasisLaunching = true;

                if (_force >= 3)
                    Tutorial.GetInstance.PlayTutorial("StasisCooldown");

                StartCoroutine(AddForce(forceDir * 10));
                _cooldownTime = _force;
                StartCoroutine(LockAirMovement(0.2f));
                break;
            case State.Cancel:
                Revert();
                break;
        }
    }

    private void AddForceDirection(Vector3 dir)
    {
        if (_force < 11)
        {
            _force++;
            ChangeArrowColor(false);
        }
        _chargedDir = dir;
        if (!_stasisArrow.gameObject.activeSelf)
            _stasisArrow.gameObject.SetActive(true);
        _stasisArrow.forward = _chargedDir;
        _stasisArrow.localScale = new Vector3(1, 1, _force);
    }

    private IEnumerator LockAirMovement(float sec)
    {
        yield return new WaitForSeconds(sec);
        _stasisLaunching = false;
    }

    private IEnumerator AddForce(Vector3 direction)
    {
        yield return new WaitForFixedUpdate();
        _rigidbody.AddForce(direction, ForceMode.VelocityChange);
    }

    private void ExecuteCharging()
    {
        _storedVelocity = _rigidbody.velocity;

        _state = State.Charge;
        TurnStasisMaterial(true);
        Freeze(true);
        _icon.enabled = true;
        _force = 0;
        _chargedDir = Vector3.zero;
        _finishCooldown = false;

        GameObject stasisArrow = Instantiate(_stasisArrowObject, transform.position, Quaternion.identity);
        _stasisArrow = stasisArrow.transform;
        _stasisArrow.gameObject.SetActive(false);

        GetComponent<Link>().TurnOffFireEffects();
    }

    private void ChangeArrowColor(bool decreasePower)
    {
        float scaleR = 0.2f;
        float scaleG = 0.1f;
        foreach (Transform t in _stasisArrow)
        {
            Color c = t.GetComponent<Renderer>().material.color;
            if (decreasePower)
            {
                if (t.GetComponent<Renderer>().material.color.r < 1)
                    t.GetComponent<Renderer>().material.color += new Color(scaleR, 0, 0);
                else
                    t.GetComponent<Renderer>().material.color += new Color(0, scaleG, 0);
            }
            else
            {
                if (t.GetComponent<Renderer>().material.color.g <= 0)
                    t.GetComponent<Renderer>().material.color -= new Color(scaleR, 0, 0);
                else
                    t.GetComponent<Renderer>().material.color -= new Color(0, scaleG, 0);
            }
        }
    }

    private void Revert()
    {
        if (_state == State.Release)
        {
            _state = State.Cooldown;
            StartCooldownGUI();
        }
        else
            _state = State.StandBy;

        TurnStasisMaterial(false);
        Freeze(false);

        if (_stasisArrow)
            Destroy(_stasisArrow.gameObject);
        _stasisArrow = null;
    }

    private void TurnStasisMaterial(bool enable)
    {
        if (enable)
        {
            for (int i = 0; i < _allCarParts.Count; i++)
                _allCarParts[i].GetComponent<Renderer>().material = _material;
        }
        else
        {
            for (int i = 0; i < _allCarParts.Count; i++)
                _allCarParts[i].GetComponent<Renderer>().material = _defaultMaterials[i];
        }
    }

    private void Freeze(bool enable)
    {
        if (!GetComponent<Link>().IsClimbing || _force > 0)
        {
            _rigidbody.isKinematic = enable;
            _rigidbody.useGravity = !enable;
            if (GameplayHandler.IS_BOSS)
                GetComponent<Collider>().isTrigger = enable;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (_state == State.Charge)
        {
            AddForceDirection(col.contacts[0].normal);
            Tutorial.GetInstance.PlayTutorial("StasisAffect");
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (GameplayHandler.IS_BOSS && _state == State.Charge)
        {
            if (!col.transform.name.Contains("Magnet"))
            {
                _state = State.Release;
                Tutorial.GetInstance.PlayTutorial("StasisAffect");
            }
        }
    }

    private void StartCooldownGUI()
    {
        _stasisCooldown.gameObject.SetActive(true);
        _stasisCooldown.localScale = new Vector3(_cooldownX, _cooldownY);
    }

    public void SetStasisMaterial(Material material)
    {
        _material = material;
    }

    public override void SetMainCamera(Transform camera)
    {
        base.SetMainCamera(camera);
    }

    public override void SetIcon(Image icon)
    {
        base.SetIcon(icon);
    }

    public override void EnableRune()
    {
        base.EnableRune();
        if (_state == State.Cooldown)
            StartCooldownGUI();
        else
            _stasisCooldown.gameObject.SetActive(false);
    }

    public override KeyCode EnableKey
    {
        get { return base.EnableKey; }
        set { base.EnableKey = value; }
    }

    public override void Disable()
    {
        base.Disable();
        if (_state == State.Cooldown)
        {
            _stasisCooldown.gameObject.SetActive(false);
            _stasisCooldown.localScale = new Vector3(_cooldownX, _cooldownY);
        }
        else if(_state == State.Charge)
        {
            _state = State.Cancel;
            Revert();
        }
    }

    public void SetStasisArrowObject(GameObject stasisArrow)
    {
        _stasisArrowObject = stasisArrow;
    }

    public override void SetCooldownMeter(RectTransform cooldown)
    {
        base.SetCooldownMeter(cooldown);
        _stasisCooldown = _cooldownMeter;
    }

    public bool ChargedStasis
    {
        get { return _state == State.Charge; }
    }

    public bool GetLockAirMovement
    {
        get { return _stasisLaunching; }
    }

    public override bool IsCooldown
    {
        get
        {
            return _state == State.Cooldown;
        }
    }

    public override float CooldownTime
    {
        get
        {
            return base.CooldownTime;
        }

        set
        {
            base.CooldownTime = value;
        }
    }

    public override bool FinishCooldown
    {
        get
        {
            return base.FinishCooldown;
        }

        set
        {
            base.FinishCooldown = value;
        }
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
