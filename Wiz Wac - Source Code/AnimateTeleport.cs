using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateTeleport : Cinematic
{
    public Material material;

    void Start()
    {
        Init();

        _state = State.None;

        _material = material;
    }

    void Update()
    {
        switch (_state)
        {
            case State.None:
                break;
            case State.Start:
                UpdateColorOnCar(false);
                TurnInvisible();
                break;
            case State.Middle1:
                GetComponent<Link>().Teleport();
                _state = State.Middle2;
                break;
            case State.Middle2:
                UpdateColorOnCar(true);
                TurnVisible();
                break;
            case State.Middle3:
                TurnOnGravity(true);
                TurnCinematicMaterial(false);
                _state = State.End;
                break;
            case State.End:
                break;
        }
    }

    private void TurnInvisible()
    {
        if (_allCarParts[0].GetComponent<Renderer>().material.color.a <= 0)
        {
            _state = State.Middle1;
        }
    }

    private void TurnVisible()
    {
        if (_allCarParts[0].GetComponent<Renderer>().material.color.a >= 1)
            _state = State.Middle3;
    }

    protected override void TurnOnGravity(bool enable)
    {
        base.TurnOnGravity(enable);
    }

    protected override void TurnCinematicMaterial(bool enable)
    {
        base.TurnCinematicMaterial(enable);
    }

    private void UpdateColorOnCar(bool turnOn)
    {
        Color c = _allCarParts[0].GetComponent<Renderer>().material.color;
        float decrease = Time.deltaTime;
        decrease *= turnOn ? -1 : 1;
        for (int i = 0; i < _allCarParts.Count; i++)
        {
            _allCarParts[i].GetComponent<Renderer>().material.color =
                new Color(c.r - decrease, c.g - decrease, c.b - decrease, c.a - decrease);
        }
    }

    override public void Begin()
    {
        base.Begin();
        _state = State.Start;
        TurnOnGravity(false);
        TurnCinematicMaterial(true);
    }

    public override bool End
    {
        get
        {
            return base.End;
        }
    }
}
