using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RemoteBomb : Rune
{
    private GameObject _bombObject;
    private Transform _bomb, _explosion, _shadow, _head;
    private Vector3 _throwDir, _equipPosition, _shadowRot;
    private RectTransform _bombCooldown;
    private bool _exploded;
    private State _state, _oldState;

    private enum State
    {
        None, StandBy, Aim, Released, Explode, Destroy, Cooldown
    }

    void Start()
    {
        _state = State.StandBy;
        _exploded = false;

        foreach(Transform t in transform)
        {
            if(t.name == "Head")
            {
                _head = t;
                break;
            }
        }
    }

    void Update()
    {
        switch(_state)
        {
            case State.Released:
                CastShadow();
                break;
            case State.Explode:
                StartCoroutine(RunExplosion());
                break;
            case State.Destroy:
                DestroyBomb();
                break;
            case State.Cooldown:
                if (Input.GetKeyDown(EnableKey))
                {
                    _bombCooldown.gameObject.SetActive(!_icon.enabled);
                    _icon.enabled = !_icon.enabled;

                }

                _bombCooldown.localScale -= new Vector3((Time.deltaTime / _cooldownTime), 0);
                _cooldownX = _bombCooldown.localScale.x;
                
                if (_cooldownX <= 0 || _finishCooldown)
                {
                    if (_icon.enabled)
                    {
                        CreateBomb();
                        _cooldownX = 1;
                        _bombCooldown.localScale = new Vector3(_cooldownX, _cooldownY);
                        _bombCooldown.gameObject.SetActive(false);
                    }
                    else
                        _state = State.StandBy;
                }
                break;
        }
    }
    
    override public void Run()
    {
        switch (_state)
        {
            case State.None:
                break;
            case State.StandBy:
                if (Input.GetKeyDown(_enableKey) || PlayerInput.UseRune)
                    CreateBomb();
                break;
            case State.Aim:
                _bomb.rotation = transform.rotation;
                _bomb.position = GetEquipPosition;
                _throwDir = _mainCamera.forward;

                if (UseRuneButton)
                {
                    _state = State.Released;
                    float playerSpeed = GetComponent<Link>().Speed;
                    float speed = playerSpeed > 0 || playerSpeed < 0 ? playerSpeed / 6 : 1;
                    StartCoroutine(ThrowBomb(speed));
                    _bomb.GetComponent<Collider>().enabled = true;
                    _shadow.GetComponent<Renderer>().enabled = true;
                    IgnoreHeadCollision();
                    if (playerSpeed > 10)
                        Tutorial.GetInstance.PlayTutorial("SuperThrow");
                }
                if (AltRuneButton)
                {
                    _state = State.Released;
                    _bomb.GetComponent<Collider>().enabled = true;
                    IgnoreHeadCollision();
                    _shadow.GetComponent<Renderer>().enabled = true;
                }

                if (Input.GetKeyDown(_enableKey) || PlayerInput.TurboHold)
                    DeleteBomb();
                break;
            case State.Released:
                if (UseRuneButton)
                    PrepareExplosion();
                break;
        }
    }

    private void CastShadow()
    {
        RaycastHit hit;
        if (Physics.Raycast(_bomb.position, Vector3.down, out hit))
        {
            if (!_shadow.GetComponent<Renderer>().enabled)
                _shadow.GetComponent<Renderer>().enabled = true;

            _shadow.position = hit.point + (Vector3.up * 0.1f);
            if (_bomb.name.Contains("Round"))
                _shadow.rotation = Quaternion.Euler(_shadowRot);
            else
                _shadow.rotation = Quaternion.Euler(_shadowRot.x, _bomb.rotation.eulerAngles.y, _shadowRot.z);
        }
        else
            _shadow.GetComponent<Renderer>().enabled = false;
    }

    private void IgnoreHeadCollision()
    {
        Physics.IgnoreCollision(_bomb.GetComponent<Collider>(), _head.GetComponent<Collider>(), true);
    }

    private IEnumerator ThrowBomb(float speed)
    {
        yield return new WaitForFixedUpdate();
        _bomb.GetComponent<Rigidbody>().AddForce(
            ((_throwDir * speed) + transform.up) * _bomb.GetComponent<Rigidbody>().mass * 10, ForceMode.Impulse);
    }

    private void PrepareExplosion()
    {
        if (_state == State.Explode || _state == State.Cooldown || _state == State.None)
            return;
        _state = State.Explode;
        _explosion.gameObject.SetActive(true);
        _bomb.GetComponent<Renderer>().enabled = false;
    }

    private IEnumerator RunExplosion()
    {
        yield return new WaitForFixedUpdate();
        _explosion.localScale += Vector3.one * 135 * Time.deltaTime;
        if (_explosion.localScale.x >= 20)
        {
            _state = State.Destroy;
            _exploded = true;
        }
    }

    private void CreateBomb()
    {
        _state = State.Aim;
        
        GameObject bombObject = Instantiate(_bombObject, GetEquipPosition, transform.rotation);
        _bomb = bombObject.transform;
        _bomb.GetComponent<Collider>().enabled = false;
        _explosion = _bomb.GetChild(0);
        _shadow = _bomb.GetChild(1);
        _shadowRot = _bomb.rotation.eulerAngles;
        _shadow.GetComponent<Renderer>().enabled = false;
        _icon.enabled = true;

        _finishCooldown = false;
    }

    public override void EnableRune()
    {
        base.EnableRune();
        if (_state == State.Cooldown)
        {
            _bombCooldown.gameObject.SetActive(true);
            _bombCooldown.localScale = new Vector3(_cooldownX, _cooldownY);
        }
        else
            _bombCooldown.gameObject.SetActive(false);
    }

    override public void Disable()
    {
        base.Disable();

        if (_state == State.Aim)
            DeleteBomb();

        if (_state == State.Cooldown)
        {
            _bombCooldown.gameObject.SetActive(false);
            _bombCooldown.localScale = new Vector3(_cooldownX, _cooldownY);
        }
    }

    private void DestroyBomb()
    {
        DeleteBomb();

        if (_exploded)
        {
            _bombCooldown.gameObject.SetActive(true);
            _bombCooldown.localScale = new Vector3(_defaultCooldownX, _cooldownY);
            _cooldownX = _defaultCooldownX;
            _state = State.Cooldown;
            _exploded = false;
            if (!_icon.enabled)
                _icon.enabled = true;
        }
        else if(_state != State.Aim)
            CreateBomb();
    }

    private void DeleteBomb()
    {
        Destroy(_bomb.gameObject);
        Destroy(_explosion.gameObject);
        Destroy(_shadow.gameObject);

        _bomb = null;

        _state = State.StandBy;
        _bombCooldown.gameObject.SetActive(false);
    }

    public void ForceExplosion()
    {
        PrepareExplosion();
    }

    public void SetBomb(GameObject bomb)
    {
        _bombObject = bomb;
    }

    private Vector3 GetEquipPosition
    {
        get { return transform.position + (transform.up / 1.4f); }
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

    public override void SetCooldownMeter(RectTransform cooldown)
    {
        base.SetCooldownMeter(cooldown);
        _bombCooldown = _cooldownMeter;
    }

    public override bool IsCooldown
    {
        get
        {
            return _state == State.Cooldown;
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
