using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AnimateEnterShrine : Cinematic
{
    public Material material;

    void Start ()
    {
        Init();

        _state = State.None;

        _material = material;
    }
    
    void Update ()
    {
        switch (_state)
        {
            case State.None:
                break;
            case State.Middle1:
                UpdateColorOnCar();
                TurnInvisible();
                break;
            case State.Stay:
                break;
        }
    }

    void FixedUpdate()
    {
        switch(_state)
        {
            //Move to center.
            case State.Start:
                Vector3 upwardPosition = transform.position + (Vector3.up * 2);
                _player.position += (upwardPosition - _player.position).normalized * Time.deltaTime * 2;
                if (Vector3.Distance(_player.position, upwardPosition) <= 1)
                    _state = State.Middle1;
                break;
        }
    }

    private void TurnInvisible()
    {
        if (_allCarParts[0].GetComponent<Renderer>().material.color.a <= 0)
        {
            _state = State.Stay;
            StartCoroutine(DelayLevelChange());
        }
    }

    private IEnumerator DelayLevelChange()
    {
        yield return new WaitForSeconds(1);
        GameplayHandler.TRAVELED = true;

        EnterShrine shrineScript = GetComponent<EnterShrine>();
        if (!shrineScript.overworld)
        {
            foreach (Transform t in transform)
            {
                if (t.name == "Restart")
                {
                    EnterShrine.LOAD_POSITION = t.position;
                    break;
                }
            }
        }
        else
        {
            //Boss is a shrine; this skips boss shrine.
            if (!GameplayHandler.IS_BOSS)
            {
                for (int i = 0; i < Fuel.FINISHED_SHRINES.Count; i++)
                {
                    if (Fuel.FINISHED_SHRINES[i] == SceneManager.GetActiveScene().name)
                        break;
                    else if (Fuel.FINISHED_SHRINES[i] == "")
                    {
                        Fuel.FINISHED_SHRINES[i] = SceneManager.GetActiveScene().name;
                        break;
                    }
                }
            }
        }

        _player.GetComponent<Buff>().SaveStats();

        //Get checkpoint for overworld.
        GameplayHandler.LOAD_TEXT.gameObject.SetActive(true);

        SceneManager.LoadScene(shrineScript.overworld ? GameplayHandler.OVERWORLDLEVEL : shrineScript.shrineName);
    }

    protected override void TurnOnGravity(bool enable)
    {
        base.TurnOnGravity(enable);
    }

    protected override void TurnCinematicMaterial(bool enable)
    {
        base.TurnCinematicMaterial(enable);
    }

    private void UpdateColorOnCar()
    {
        Color c = _allCarParts[0].GetComponent<Renderer>().material.color;
        float decrease = Time.deltaTime;
        for (int i = 0; i < _allCarParts.Count; i++)
        {
            _allCarParts[i].GetComponent<Renderer>().material.color =
                new Color(c.r - decrease, c.g - decrease, c.b - decrease, c.a - decrease);
        }
    }

    override public void Begin()
    {
        if (_state != State.End)
        {
            _state = State.Start;
            TurnOnGravity(false);
            GetComponent<Collider>().enabled = false;
            TurnCinematicMaterial(true);

        }
    }

    public override bool Stay
    {
        get { return base.Stay; }
    }
}
