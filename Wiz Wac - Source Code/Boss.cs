using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    public Transform magnetSphere;
    public float turnSpeed;
    public float minAngle;
    public float prepareShoot;
    public float cooldown;
    public List<AudioClip> _songs;

    private Transform _player, _spawn, _explosion, _fakeBoss;
    private Vector3 _startPos, _startRot;
    private float _speed, _cooldown;
    private Ending _endingScript;
    private AudioSource _audio;
    private bool _playerHighUp;
    private State _state;
    private List<Transform> _mainBodyParts;
    private List<Transform> _allBodyPartsDefault;
    private List<Transform> _allBodyParts;
    private List<SwitchLeader> _switchLeaderScripts;

    public static bool DEFEATED;

    private enum State
    {
        Ending, Looking, Charging, Shoot, Cooldown, LookAway, Explode
    }

    void Start ()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _endingScript = GameObject.Find("Ending").GetComponent<Ending>();
        _audio = GameObject.Find("AudioComponent").GetComponent<AudioSource>();

        _fakeBoss = GameObject.Find("FakeBoss").transform;

        _explosion = GameObject.Find("BossBoom").transform;
        _explosion.gameObject.SetActive(false);

        _switchLeaderScripts = new List<SwitchLeader>();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Switch"))
            _switchLeaderScripts.Add(go.GetComponent<SwitchLeader>());

        _mainBodyParts = new List<Transform>();
        _allBodyParts = new List<Transform>();
        foreach (Transform t in transform)
        {
            _mainBodyParts.Add(t);
            if (t.name == "Head")
            {
                foreach (Transform tChild in t.GetComponentsInChildren<Transform>())
                {
                    if (tChild.name == "Spawn")
                        _spawn = tChild;
                }
            }
        }
        foreach (Transform t in _fakeBoss.transform)
        {
            _allBodyParts.Add(t);
            if (t.name == "Head")
            {
                foreach (Transform tChild in t.GetComponentsInChildren<Transform>())
                {
                    _allBodyParts.Add(tChild);
                    if (tChild.name == "Spawn")
                        _spawn = tChild;
                }
            }
            if (t.name == "Body")
            {
                foreach (Transform tChild in t.GetComponentsInChildren<Transform>())
                    _allBodyParts.Add(tChild);
            }
        }
        foreach(Transform t in _allBodyParts)
        {
            if (t.GetComponent<Collider>())
                t.GetComponent<Collider>().enabled = false;
            if (t.GetComponent<Renderer>())
                t.GetComponent<Renderer>().enabled = false;
        }

        RenderSettings.skybox.SetColor("_Tint", Color.black);

        _state = State.Looking;
    }

    void Update()
    {
        switch(_state)
        {
            case State.Ending:
                EndingExplosion();
                break;
            case State.Looking:
                InitiateExplosion();
                HandleSwitchCounters();
                if (_playerHighUp)
                    _state = State.LookAway;
                break;
            case State.Charging:
                InitiateExplosion();
                HandleSwitchCounters();
                PrepareShoot();
                if (_playerHighUp)
                    _state = State.LookAway;
                break;
            case State.Shoot:
                InitiateExplosion();
                HandleSwitchCounters();
                Shoot();
                if (_playerHighUp)
                    _state = State.LookAway;
                break;
            case State.Cooldown:
                InitiateExplosion();
                HandleSwitchCounters();
                if (_playerHighUp)
                    _state = State.LookAway;
                break;
            case State.LookAway:
                InitiateExplosion();
                HandleSwitchCounters();
                if (!_playerHighUp)
                    _state = State.Looking;
                break;
            case State.Explode:
                TurnOffSwitchContactEffects();

                StartCoroutine(DelayExplosion());

                _audio.Stop();

                _state = State.Ending;
                break;
        }

        _playerHighUp = _player.position.y > 135;
    }
    
    void FixedUpdate ()
    {
        switch (_state)
        {
            case State.Ending:
                break;
            case State.Looking:
                LookAtTarget(false);
                break;
            case State.LookAway:
                LookAtTarget(true);
                break;
            case State.Explode:
                break;
        }
    }

    private void EndingExplosion()
    {
        if (_explosion.localScale.x < 260 && _explosion.GetComponent<Renderer>().material.color.a == 1)
            _explosion.localScale += Vector3.one / (Time.timeScale == 1 ? 1 : 30);
        else
        {
            _explosion.GetComponent<Renderer>().material.color -= new Color(0, 0, 0, 0.0003f);
            if (_explosion.GetComponent<Renderer>().material.color.a <= 0)
                _explosion.gameObject.SetActive(false);

            _explosion.localScale -= Vector3.one / 20;
        }
        float rot = 0.025f;
        _explosion.Rotate(rot, rot, rot);
    }

    private void InitiateExplosion()
    {
        if (!_endingScript.off)
        {
            _state = State.Explode;
            _explosion.gameObject.SetActive(true);

            //Hide it so the fake can take its place.
            transform.position = Vector3.up * 10000;

            foreach (Transform t in _allBodyParts)
            {
                if (t.GetComponent<Collider>())
                    t.GetComponent<Collider>().enabled = true;
                if (t.GetComponent<Renderer>())
                    t.GetComponent<Renderer>().enabled = true;
            }
            _fakeBoss.forward = Vector3.forward;
        }
    }

    private void TurnOffSwitchContactEffects()
    {
        foreach (Transform t in _allBodyParts)
        {
            if (t.GetComponent<SwitchLeader>())
                t.GetComponent<SwitchLeader>().TurnOffContactEffect();
            t.parent = null;
        }
    }

    private IEnumerator SlowMotion()
    {
        yield return new WaitForSeconds(0.1f);
        Time.timeScale = 0.01f;
    }

    private IEnumerator DelayExplosion()
    {
        yield return new WaitForSeconds(1); ;

        _audio.clip = _songs[3];
        _audio.Play();
        _audio.loop = false;

        StartCoroutine(Explosion());
        StartCoroutine(SlowMotion());
    }

    private IEnumerator Explosion()
    {
        yield return new WaitForFixedUpdate();
        foreach (Transform t in _allBodyParts)
        {
            if (!t.GetComponent<Rigidbody>())
                continue;
            t.GetComponent<Rigidbody>().useGravity = true;
            t.GetComponent<Rigidbody>().isKinematic = false;

            float min = -10;
            float max = 11;
            float x = Random.Range(min, max);
            float y = Random.Range(min, max);
            float z = Random.Range(min, max);
            Vector3 v = new Vector3(x, y, z);
            //Extra force on the extra switch to make it go fly off better.
            if (t.name.Contains("()"))
                v += Vector3.up * 1000;
            t.GetComponent<Rigidbody>().AddForce(v, ForceMode.VelocityChange);
        }
        _player.GetComponent<Rigidbody>().AddForce(Vector3.up * 100, ForceMode.VelocityChange);
        _player.GetComponent<Link>().ControlCar = false;

        DEFEATED = true;
    }

    private void HandleSwitchCounters()
    {
        int counter = 0;
        foreach(SwitchLeader sl in _switchLeaderScripts)
        {
            if (!sl.off)
                counter++;
        }
        AudioClip clip = _audio.clip;
        _speed = turnSpeed;
        _cooldown = cooldown;
        switch (counter)
        {
            case 0:
                clip = _songs[0];
                break;
            case 1:
                _speed += 5;
                _cooldown -= 5;
                clip = _songs[0];
                break;
            case 2:
                clip = _songs[1];
                _speed += 10;
                _cooldown -= 8;
                break;
            case 3:
                clip = _songs[2];
                _speed += 15;
                _cooldown -= 10;
                break;
        }
        if (_audio.clip != clip)
        {
            _audio.clip = clip;
            _audio.Play();
        }
    }

    private void LookAtTarget(bool inverse)
    {
        Vector3 dir = inverse ? _mainBodyParts[0].position - _player.position : _player.position - _mainBodyParts[0].position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, _speed * Time.deltaTime);

        BodyRotation();

        if (!inverse && Vector3.Angle(transform.forward, dir) < minAngle)
        {
            _state = State.Charging;
            StartCoroutine(PrepareShoot());
        }
    }

    private void BodyRotation()
    {
        foreach (Transform t in _mainBodyParts)
            t.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
    }

    private IEnumerator PrepareShoot()
    {
        yield return new WaitForSeconds(prepareShoot);

        Vector3 spawn = _spawn.position;
        StartCoroutine(Spawn(0, spawn));
        StartCoroutine(Spawn(0.5f, spawn + Vector3.up));
        StartCoroutine(Spawn(1, spawn + Vector3.up * 2));

        _state = State.Shoot;
    }

    private IEnumerator Spawn(float sec, Vector3 pos)
    {
        yield return new WaitForSeconds(sec);
        GameObject bullet = Instantiate(magnetSphere.gameObject, pos, Quaternion.identity);
        bullet.GetComponent<Rigidbody>().AddForce(_player.position - pos, ForceMode.VelocityChange);
    }

    private void Shoot()
    {
        _state = State.Cooldown;
        StartCoroutine(BackToLooking());
    }

    private IEnumerator BackToLooking()
    {
        yield return new WaitForSeconds(_cooldown);
        _state = _playerHighUp ? State.LookAway : State.Looking;
    }
}
