using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cinematic : MonoBehaviour
{
    protected Transform _player;
    protected Material _material;
    protected List<Transform> _allCarParts;
    protected List<Material> _defaultMaterials;
    protected State _state;

    //Many Middle's to leave open for different transitions during cinematics.
    protected enum State
    {
        None, Start, Middle1, Middle2, Middle3, End, Stay
    }

    virtual protected void Init()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;

        _defaultMaterials = new List<Material>();
        _allCarParts = new List<Transform>();
        _allCarParts.Add(_player);
        _defaultMaterials.Add(_player.GetComponent<Renderer>().material);
        foreach (Transform t in _player)
        {
            if (t.name == "Thruster" || t.name == "Exhaust")
                continue;
            _allCarParts.Add(t);
            _defaultMaterials.Add(t.GetComponent<Renderer>().material);
        }
    }

    virtual protected void TurnCinematicMaterial(bool enable)
    {
        if (enable)
        {
            for (int i = 0; i < _allCarParts.Count; i++)
                AssignMaterial(i, _material);

            _player.GetComponent<Link>().TurnOffFireEffects();
        }
        else
        {
            for (int i = 0; i < _allCarParts.Count; i++)
                AssignMaterial(i, _defaultMaterials[i]);
        }
    }

    private void AssignMaterial(int i, Material mat)
    {
        _allCarParts[i].GetComponent<Renderer>().material = mat;
    }

    virtual protected void TurnOnGravity(bool enable)
    {
        _player.GetComponent<Rigidbody>().useGravity = enable;
        _player.GetComponent<Rigidbody>().isKinematic = !enable;
    }

    virtual public void Begin()
    {
        if (_state != State.End)
        {
            _state = State.Start;
        }
    }

    virtual public bool End
    {
        get { return _state == State.End; }
    }

    virtual public bool Stay
    {
        get { return _state == State.Stay; }
    }
}
