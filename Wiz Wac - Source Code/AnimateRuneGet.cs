using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateRuneGet : Cinematic
{
    public Material material;

    private ParticleSystem.MainModule _mainModule;
    private ParticleSystem.MinMaxCurve _minMaxCurve;
    private float _rotationSpeed, _rotationLeft;

    void Start ()
    {
        Init();

        ParticleSystem particleSystem = GetComponent<ParticleSystem>();
        _mainModule = particleSystem.main;
        _minMaxCurve = _mainModule.startSpeed;

        _state = State.None;

        _material = material;

        _rotationLeft = 360 * 2;
        _rotationSpeed = 180;
    }
    
    void FixedUpdate ()
    {
        switch(_state)
        {
            case State.None:
                break;
            case State.Start:
                //Move to center.
                _player.position += (transform.position - _player.position).normalized * Time.deltaTime * 2;
                if (Vector3.Distance(_player.position, transform.position) <= 1)
                    _state = State.Middle1;
                break;
            case State.Middle1:
                float currentRotation = _rotationSpeed * Time.deltaTime;
                if (_rotationLeft > currentRotation)
                    _rotationLeft -= currentRotation;
                else
                {
                    currentRotation = _rotationLeft;

                    _state = State.End;

                    TurnCinematicMaterial(false);
                    TurnOnGravity(true);
                }
                _player.Rotate(0, currentRotation, 0);
                break;
            case State.End:
                break;
        }
    }

    private void StopParticle()
    {
        _minMaxCurve.constant = 0;
        _mainModule.startSpeed = _minMaxCurve;
        GetComponent<ParticleSystem>().Stop();
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
        if (_state != State.End)
        {
            TurnCinematicMaterial(true);
            TurnOnGravity(false);
            GetComponent<Collider>().enabled = false;
        }
    }

    public override bool End
    {
        get
        {
            return base.End;
        }
    }
}
