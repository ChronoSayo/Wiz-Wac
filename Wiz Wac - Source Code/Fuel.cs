using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fuel : MonoBehaviour
{
    public RectTransform fuelMeter;

    private Image _fuelMeterImage;
    private Image _fuelMeterGlowImage;
    private float _fuelSpending, _scale, _y;
    private bool _infiniteFuel, _glowLit;

    private static bool _START_GAME_FUEL;
    private static float _CURRENT_FUEL;
    
    public static List<string> FINISHED_SHRINES;

    void Start ()
    {
        _scale = 100000 * (10 * (RuneHandler.RUNES_UNLOCKED ? 2 : 1));

        float x = fuelMeter.parent.localScale.x;
        fuelMeter.parent.localScale = new Vector3(RuneHandler.RUNES_UNLOCKED ? x : x / 2, fuelMeter.parent.localScale.y);

        _y = fuelMeter.localScale.y;

        _fuelMeterImage = fuelMeter.GetComponent<Image>();

        CheckFinishedShrines();

        CheckIfInfiniteFuel();
        //Debug.
        //_infiniteFuel = true;

        _fuelMeterGlowImage = GameObject.Find("FuelBGGlow").GetComponent<Image>();
        _fuelMeterGlowImage.gameObject.SetActive(_infiniteFuel);

        CheckInfiniteFuelGUI();
        _glowLit = false;

        InfiniteFuelMessage();
        BossEncounterMessage();

        ChangeMeterColor();
    }

    private void CheckFinishedShrines()
    {
        if (FINISHED_SHRINES == null)
        {
            FINISHED_SHRINES = new List<string>();
            for (int i = 0; i < 8; i++)
                FINISHED_SHRINES.Add("");
        }
    }

    private void CheckIfInfiniteFuel()
    {
        _infiniteFuel = true;
        foreach (string s in FINISHED_SHRINES)
        {
            if (s == "")
            {
                _infiniteFuel = false;
                break;
            }
        }
    }

    private void CheckInfiniteFuelGUI()
    {
        if (!_infiniteFuel)
        {
            if (!_START_GAME_FUEL)
                _START_GAME_FUEL = true;
            else
                fuelMeter.localScale = new Vector3(_CURRENT_FUEL, _y);
        }
    }

    private void InfiniteFuelMessage()
    {
        if (_infiniteFuel)
        {
            ShowMessage.GetInstance.PlayMessage(
                "The Shrines of Hyoctrule have been completed. \n\nYou have been granted endless fuel tank.\n\n" +
                "Return to whence you came to defeat the evil lingering inside.", 15);
            _fuelMeterImage.color = new Color(0, 0, 1);
        }
    }

    private void BossEncounterMessage()
    {
        if (GameplayHandler.IS_BOSS)
        {
            if (_infiniteFuel)
            {
                ShowMessage.GetInstance.PlayMessage(
                    "Find a path into the heart of the beast in order to defeat it.\n\nGood luck...", 10);
            }
            else
            {
                ShowMessage.GetInstance.PlayMessage(
                    "You are still too weak to fight this Demon.\n\nI advise you to return here when the " +
                    "time is right.", 15);
            }
        }
    }

    void Update()
    {
        if (_infiniteFuel)
            Blink();
    }

    private void Blink()
    {
        float speed = 0.008f;
        if (!_glowLit)
            speed *= -1;

        _fuelMeterGlowImage.color += new Color(0, 0, 0, speed);

        Color c = _fuelMeterGlowImage.color;
        c.a = Mathf.Clamp(c.a, 0, 1);
        _fuelMeterGlowImage.color = c;

        if (c.a <= 0)
            _glowLit = true;
        if (c.a >= 1)
            _glowLit = false;
    }

    private void Limits()
    {
        if (fuelMeter.localScale.x < 0)
            fuelMeter.localScale = new Vector3(0, _y);
        if (fuelMeter.localScale.x > 1)
            fuelMeter.localScale = new Vector3(1, _y);

        _CURRENT_FUEL = fuelMeter.localScale.x;
    }

    public void Spend(float cost)
    {
        _fuelSpending = _infiniteFuel ? 0 : cost / _scale;
        if(_fuelSpending != 0)
            Decrease();
    }

    public void AddSpending(float extra)
    {
        extra = _infiniteFuel ? 0 : extra;
        Spend(_fuelSpending + extra);
    }

    private void Decrease()
    {
        Buff buff = GetComponent<Buff>();
        float fuelSpending = buff.BuffedFuelSpend ? _fuelSpending / buff.buffValues[3] : _fuelSpending;
        fuelMeter.localScale -= new Vector3(fuelSpending, 0);
        ChangeMeterColor();
        
        if (buff.NotFuelBuff && fuelMeter.localScale.x < 0.5f)
            Tutorial.GetInstance.PlayTutorial("BuffFuel");
        if (fuelMeter.localScale.x < 0.35f)
            Tutorial.GetInstance.PlayTutorial("LowFuel");
        if (fuelMeter.localScale.x <= 0)
            Tutorial.GetInstance.PlayTutorial("NoFuel");
        Limits();
    }

    private void Increase()
    {
        fuelMeter.localScale += new Vector3(_fuelSpending, 0);
        ChangeMeterColor();
        Limits();
    }

    private void ChangeMeterColor()
    {
        if (_infiniteFuel)
            _fuelMeterImage.color = new Color(0, 0, 1);
        else
        {
            float x = fuelMeter.localScale.x;
            _fuelMeterImage.color = new Color(1 - x, x, 0);
        }
    }

    private void Refuel(bool enable)
    {
        _fuelSpending = enabled ? 0.0005f : 0;
        Increase();
    }

    public void SetFullFuel()
    {
        fuelMeter.localScale = new Vector3(1, _y);
        ChangeMeterColor();
    }

    public bool NoFuel
    {
        get { return fuelMeter.localScale.x <= 0; }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "GasStation")
            Tutorial.GetInstance.PlayTutorial(col.tag);
    }

    void OnTriggerStay(Collider col)
    {
        if(col.tag == "GasStation" && !_infiniteFuel)
            Refuel(true);
    }

    void OnTriggerExit(Collider col)
    {
        if (col.tag == "GasStation" && !_infiniteFuel)
            Refuel(false);
    }
}
