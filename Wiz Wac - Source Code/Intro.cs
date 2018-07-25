using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intro : Cinematic
{
    public Material material;

    void Start ()
    {
        Init();

        _material = material;
    }

    void Update ()
    {
        switch(_state)
        {
            case State.None:
                break;
            case State.Start:
                TurnOnGravity(false);
                TurnCinematicMaterial(true);

                SetInvisible();

                _state = State.Middle1;
                break;
            case State.Middle1:
                UpdateColorOnCar();
                HandleReturn();
                break;
            case State.End:
                break;
        }
    }

    private void HandleReturn()
    {
        if (_allCarParts[0].GetComponent<Renderer>().material.color.a >= 1)
        {
            _state = State.End;
            TurnCinematicMaterial(false);
            GameplayHandler.TRAVELED = false;
            TurnOnGravity(true);
        }
    }

    private void SetInvisible()
    {
        for (int i = 0; i < _allCarParts.Count; i++)
            _allCarParts[i].GetComponent<Renderer>().material.color = new Color(0, 0, 0, 0);
    }

    private void UpdateColorOnCar()
    {
        Color c = _allCarParts[0].GetComponent<Renderer>().material.color;
        float increase = Time.deltaTime / 2;
        for (int i = 0; i < _allCarParts.Count; i++)
        {
            _allCarParts[i].GetComponent<Renderer>().material.color =
                new Color(c.r + increase, c.g + (increase / 2), c.b + increase, c.a + increase);
        }
    }

    protected override void TurnCinematicMaterial(bool enable)
    {
        base.TurnCinematicMaterial(enable);
    }

    protected override void TurnOnGravity(bool enable)
    {
        base.TurnOnGravity(enable);
    }

    public override void Begin()
    {
        base.Begin();
    }

    public override bool End
    {
        get
        {
            return base.End;
        }
    }
}
