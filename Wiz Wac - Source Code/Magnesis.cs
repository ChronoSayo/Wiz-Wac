using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Magnesis : Rune
{
    private GameObject _contactObject;
    private Transform _magnetized;
    private Transform _contactEffect;
    private Image _crosshair;
    private float _distance, _maxDistance, _stuckWait;
    private State _state;

    private enum State
    {
        None, StandBy, Aim, Attract, Release
    }

    void Start ()
    {
        _state = State.StandBy;

        _maxDistance = 200;

        _stuckWait = 0;
    }

    public override void Run()
    {
        switch (_state)
        {
            case State.None:
                break;
            case State.StandBy:
                if (Input.GetKeyDown(EnableKey) || PlayerInput.UseRune)
                {
                    _state = State.Aim;
                    _icon.enabled = true;

                    GameObject magnetEffect = Instantiate(_contactObject, transform.position, Quaternion.identity);
                    _contactEffect = magnetEffect.transform;
                    _crosshair.enabled = true;
                    _contactEffect.GetComponent<Renderer>().enabled = false;
                }
                break;
            case State.Aim:
                RaycastHit hit;
                if (Physics.Raycast(_mainCamera.position, _mainCamera.forward, out hit, _maxDistance))
                {
                    float dist = Vector3.Distance(hit.transform.position, transform.position);
                    bool xSide = dist > hit.transform.localScale.x;
                    bool ySide = dist > hit.transform.localScale.y;
                    bool zSide = dist > hit.transform.localScale.z;
                    bool allSidesApproved = xSide && ySide && zSide;
                    
                    if ((hit.transform.name.Contains("Magnet")  || hit.transform.name.Contains("Bomb")) && allSidesApproved)
                    {
                        if (!_contactEffect.GetComponent<Renderer>().enabled)
                            _contactEffect.GetComponent<Renderer>().enabled = true;

                        MagnetEffectBeam(hit.transform);

                        if (hit.transform.name.Contains("Bomb"))
                            Tutorial.GetInstance.PlayTutorial("MagnetBomb");

                        if (PlayerInput.UseRune)
                        {
                            _state = State.Attract;
                            _magnetized = hit.transform;
                            _crosshair.enabled = false;
                            _distance = DistanceBetweenPlayerAndMagnetized();
                            _magnetized.GetComponent<Rigidbody>().velocity = Vector3.zero;
                            MagnetizedObject(false);
                        }
                    }
                    else
                    {
                        if (_contactEffect.GetComponent<Renderer>().enabled)
                            _contactEffect.GetComponent<Renderer>().enabled = false;
                    }
                }
                else
                {
                    if (_contactEffect.GetComponent<Renderer>().enabled)
                        _contactEffect.GetComponent<Renderer>().enabled = false;
                }

                if (Input.GetKeyDown(EnableKey) || PlayerInput.AltRune || PlayerInput.TurboHold)
                    _state = State.Release;
                break;
            case State.Attract:
                float currentDist = _distance;
                if (DistanceBetweenPlayerAndMagnetized() < _distance)
                    currentDist = DistanceBetweenPlayerAndMagnetized();
                if (DistanceBetweenPlayerAndMagnetized() > _distance)
                    currentDist = _distance;

                Rigidbody rb = _magnetized.GetComponent<Rigidbody>();
                float power = 0.5f;
                if (PlayerInput.UseRuneHold)
                {
                    rb.AddForce((transform.position - _magnetized.position).normalized * power, ForceMode.VelocityChange);
                    currentDist = DistanceBetweenPlayerAndMagnetized();
                }
                if (PlayerInput.AltRuneHold)
                {
                    rb.AddForce(-(transform.position - _magnetized.position).normalized * power, ForceMode.VelocityChange);
                    currentDist = DistanceBetweenPlayerAndMagnetized();
                }
                if (!PlayerInput.UseRuneHold && !PlayerInput.AltRuneHold)
                    rb.velocity = Vector3.zero;
                float min = _magnetized.localScale.x;
                if (currentDist <= min)
                    rb.velocity = Vector3.zero;

                ShowTutorialStuck(rb.velocity.magnitude);

                _distance = currentDist;

                MagnetEffectBeam(_magnetized);

                if (Input.GetKeyDown(EnableKey) || PlayerInput.TurboHold || 
                    Vector3.Distance(transform.position, _magnetized.position) >= _maxDistance)
                    _state = State.Release;
                break;
            case State.Release:
                LoseAttraction();
                break;
        }
    }

    private void ShowTutorialStuck(float magnitude)
    {
        if ((PlayerInput.UseRuneHold || PlayerInput.AltRuneHold) && magnitude < 1)
        {
            _stuckWait += Time.deltaTime;
            if (_stuckWait >= 1)
                Tutorial.GetInstance.PlayTutorial("MagnetStuck");
        }
        else
            _stuckWait = 0;
    }
    
    private bool IsCollided(bool attract)
    {
        if(attract)
        {
            if (Physics.Raycast(transform.position, (_magnetized.position - transform.position).normalized))
                return MagnetizedCollide();
        }
        else
        {
            if (Physics.Raycast(_magnetized.position, (_magnetized.position - transform.position).normalized))
            {
                if (MagnetizedCollide())
                {
                    _magnetized.position = 
                        _magnetized.position + _magnetized.GetComponent<MagnetizedCollision>().ContactPoints[0].normal * 2;
                }
                return MagnetizedCollide();
            }
        }
        return true;
    }

    private bool MagnetizedCollide()
    {
        ContactPoint[] contactPoints = _magnetized.GetComponent<MagnetizedCollision>().ContactPoints;
        if (contactPoints == null)
            return false;
        else
            return true;
    }

    private float DistanceBetweenPlayerAndMagnetized()
    {
        return Vector3.Distance(transform.position, _magnetized.position);
    }

    private void MagnetEffectBeam(Transform hit)
    {
        _contactEffect.position = (hit.position + transform.position) / 2;
        _contactEffect.forward = (hit.position - transform.position).normalized;
        _contactEffect.localScale = new Vector3(0.1f, 0.1f, Vector3.Distance(transform.position, hit.position));
    }

    private void LoseAttraction()
    {
        if (_magnetized)
        {
            MagnetizedObject(true);
            if (_magnetized.GetComponent<Rigidbody>().velocity.magnitude > 15)
                Tutorial.GetInstance.PlayTutorial("MagnetFly");
            _magnetized = null;
        }
        if (_contactEffect)
            Destroy(_contactEffect.gameObject);

        _state = State.StandBy;
        _crosshair.enabled = false;
    }

    private void MagnetizedObject(bool release)
    {
        _magnetized.GetComponent<Rigidbody>().useGravity = release;
        _magnetized.GetComponent<Rigidbody>().freezeRotation = !release;
    }

    public void SetContactEffect(GameObject effect)
    {
        _contactObject = effect;
    }

    public void SetCrosshair(Image crosshair)
    {
        _crosshair = crosshair;
        _crosshair.enabled = false;
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
    }

    public override KeyCode EnableKey
    {
        set { base.EnableKey = value; }
        get { return base.EnableKey; }

    }
    public override void Disable()
    {
        base.Disable();
        _state = State.Release;
        LoseAttraction();
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
