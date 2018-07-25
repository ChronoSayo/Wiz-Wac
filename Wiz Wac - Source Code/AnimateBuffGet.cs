using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateBuffGet : Cinematic
{
    public Material material;

    private UnlockBuff _unlockBuffScript;
    private ParticleSystem.MainModule _mainModule;
    private ParticleSystem.MinMaxCurve _minMaxCurve;
    private float _time, _tick;

    void Start()
    {
        Init();

        _unlockBuffScript = GetComponent<UnlockBuff>();

        ParticleSystem particleSystem = GetComponent<ParticleSystem>();
        _mainModule = particleSystem.main;
        _minMaxCurve = _mainModule.startSpeed;

        _state = State.None;

        _material = material;

        _time = 1;
        _tick = 0;
    }

    void Update()
    {
        switch(_state)
        {
            case State.Middle2:
                StopParticles();

                _state = State.End;

                GetComponent<Collider>().enabled = true;

                TurnCinematicMaterial(false);
                TurnOnGravity(true);

                _unlockBuffScript.SetBuff();
                _tick = 0;
                break;
        }
    }

    void FixedUpdate()
    {
        switch (_state)
        {
            case State.None:
                break;
            case State.Start:
                MoveToCenter();
                break;
            case State.Middle1:
                Shake();
                break;
            case State.End:
                break;
        }
    }

    private void MoveToCenter()
    {
        _player.position += (transform.position - _player.position).normalized * Time.deltaTime * 10;
        if (Vector3.Distance(_player.position, transform.position) <= 1)
            _state = State.Middle1;
    }

    private void Shake()
    {
        _tick += Time.deltaTime;
        if (_tick < _time)
            _player.position += GetRandomDirection * 0.02f;
        else
            _state = State.Middle2;
    }

    private void StopParticles()
    {
        _minMaxCurve.constant = 0;
        _mainModule.startSpeed = _minMaxCurve;
        GetComponent<ParticleSystem>().Stop();
    }

    private Vector3 GetRandomDirection
    {
        get
        {
            int rand = Random.Range(0, 5);
            Vector3 v = Vector3.up;
            switch (rand)
            {
                case 0:
                    v = Vector3.left;
                    break;
                case 1:
                    v = Vector3.right;
                    break;
                case 2:
                    v = Vector3.down;
                    break;
            }
            return v;
        }
    }

    protected override void TurnOnGravity(bool enable)
    {
        base.TurnOnGravity(enable);
    }

    protected override void TurnCinematicMaterial(bool enable)
    {
        base.TurnCinematicMaterial(enable);
    }

    public override void Begin()
    {
        base.Begin();
        TurnCinematicMaterial(true);
        TurnOnGravity(false);
        GetComponent<Collider>().enabled = false;
        _state = State.Start;
    }

    public override bool End
    {
        get
        {
            return base.End;
        }
    }
}
