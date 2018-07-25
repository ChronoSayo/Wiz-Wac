using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RuneHandler : MonoBehaviour
{
    public Transform mainCamera;
    public RectTransform cooldown;
    public Transform roundBomb;
    public Transform squareBomb;
    public Transform magnesisEffect;
    public Transform stasisArrow;
    public Transform ramp;
    public Material stasisMaterial;
    public Image roundBombIcon;
    public Image squareBombIcon;
    public Image magnesisIcon;
    public Image stasisIcon;
    public Image cryonisIcon;
    public Image magnesisCrosshair;
    public float bombCooldown;
    public float stasisCooldownAdditive;

    private GameObject _runeUnlocked;
    private Rune _currentRune;
    private bool _unlocking, _switching;
    private List<Rune> _runes;
    private List<KeyCode> _runeKeys;

    public static List<bool> UNLOCKED_RUNES;
    public static bool RUNES_UNLOCKED;

    void Start ()
    {
        _unlocking = _switching = false;

        _runes = new List<Rune>();
        _runes.Add(gameObject.AddComponent<RemoteBomb>());
        _runes.Add(gameObject.AddComponent<RemoteBomb>());
        _runes.Add(gameObject.AddComponent<Magnesis>());
        _runes.Add(gameObject.AddComponent<Stasis>());
        _runes.Add(gameObject.AddComponent<Cryonis>());

        if (UNLOCKED_RUNES == null)
        {
            UNLOCKED_RUNES = new List<bool>();
            for (int i = 0; i < 5; i++)
                UNLOCKED_RUNES.Add(false);
        }
        else
        {
            if (!RUNES_UNLOCKED)
                CheckAllRunesUnlocked();
        }

        AssignKeys();

        //Debug.
        bool enableAll = false;
        if(enableAll)
        {
            for (int i = 0; i < _runes.Count; i++)
                UNLOCKED_RUNES[i] = true;
        }

        HandleSetRune();

        _currentRune = null;
    }

    private void CheckUnlockedRunes()
    {
        if (UNLOCKED_RUNES == null)
        {
            UNLOCKED_RUNES = new List<bool>();
            for (int i = 0; i < _runes.Count; i++)
                UNLOCKED_RUNES.Add(false);
        }
    }

    private void AssignKeys()
    {
        _runeKeys = new List<KeyCode>();
        _runeKeys.Add(KeyCode.Alpha1);
        _runeKeys.Add(KeyCode.Alpha2);
        _runeKeys.Add(KeyCode.Alpha3);
        _runeKeys.Add(KeyCode.Alpha4);
        _runeKeys.Add(KeyCode.Alpha5);
    }
    
    private void HandleSetRune()
    {
        int count = 0;
        SetRune((RemoteBomb)(_runes[count]), roundBombIcon, count, cooldown.gameObject, bombCooldown);
        ((RemoteBomb)(_runes[count])).SetBomb(roundBomb.gameObject);

        count++;
        SetRune((RemoteBomb)(_runes[count]), squareBombIcon, count, cooldown.gameObject, bombCooldown);
        ((RemoteBomb)(_runes[count])).SetBomb(squareBomb.gameObject);

        count++;
        SetRune((Magnesis)(_runes[count]), magnesisIcon, count);
        ((Magnesis)(_runes[count])).SetContactEffect(magnesisEffect.gameObject);
        ((Magnesis)(_runes[count])).SetCrosshair(magnesisCrosshair);

        count++;
        SetRune((Stasis)(_runes[count]), stasisIcon, count, cooldown.gameObject, stasisCooldownAdditive);
        ((Stasis)(_runes[count])).SetStasisMaterial(stasisMaterial);
        ((Stasis)(_runes[count])).SetStasisArrowObject(stasisArrow.gameObject);

        count++;
        SetRune((Cryonis)(_runes[count]), cryonisIcon, count);
        ((Cryonis)(_runes[count])).SetRamp(ramp.gameObject);
    }

    private void SetRune(Rune rune, Image icon, int count, GameObject cooldownObject = null, float cooldownTime = 0)
    {
        rune.SetMainCamera(mainCamera);
        rune.SetIcon(icon);
        rune.EnableKey = _runeKeys[count];
        rune.Unlocked = UNLOCKED_RUNES[count];

        if (cooldownObject)
        {
            RectTransform cooldownRect = cooldownObject.GetComponent<RectTransform>();
            RectTransform cd = Instantiate(cooldownObject, cooldownRect.position, cooldownRect.rotation).GetComponent<RectTransform>();
            cd.SetParent(mainCamera.GetChild(0));
            cd.localScale = cooldown.localScale;
            rune.SetCooldownMeter(cd);

            rune.CooldownTime = cooldownTime;
        }
    }
    
    void Update ()
    {
        if (_unlocking)
            UnlockingRune();
        else
        {
            Link carScript = GetComponent<Link>();
            if (!carScript.ControlCar || carScript.IsDead)
            {
                if (_currentRune)
                {
                    _currentRune.Disable();
                        _currentRune = null;
                }
                return;
            }
            EnableRune();

            if (_currentRune)
                _currentRune.Run();
        }

    }

    private void EnableRune()
    {
        ControllerRuneSwitch();

        for (int i = 0; i < _runes.Count; i++)
            DetectKey(i);
    }

    private void ControllerRuneSwitch()
    {
        if (PlayerInput.RuneSwitchRight && !_switching)
        {
            _switching = true;
            int i = GetCurrentRuneIndex();
            i++;
            if (i == _runes.Count)
                return;
            bool found = true;
            while (!_runes[i].Unlocked)
            {
                i++;
                if (i == _runes.Count)
                {
                    found = false;
                    break;
                }
            }
            if(found)
                _currentRune = CurrentRune(_runes[i]);
        }
        else if (PlayerInput.RuneSwitchLeft && !_switching)
        {
            _switching = true;

            int i = GetCurrentRuneIndex();
            i--;
            bool availableRune = i > -1;
            if (availableRune)
            {
                bool found = true;
                while (!_runes[i].Unlocked)
                {
                    i--;
                    if (i < 0)
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                    _currentRune = CurrentRune(_runes[i]);
                else
                    EmptySlot();
            }
            else
                EmptySlot();
        }
        else if (PlayerInput.TriggerNeutral)
            _switching = false;
    }

    private void EmptySlot()
    {
        if (_currentRune)
            _currentRune.Disable();
        _currentRune = null;
    }

    private int GetCurrentRuneIndex()
    {
        int current = -1;
        for(int i = 0; i < _runes.Count; i++)
        {
            if(_currentRune == _runes[i])
            {
                current = i;
                break;
            }
        }
        return current;
    }

    private void DetectKey(int i)
    {
        if (Input.GetKeyDown(_runeKeys[i]) && _runes[i].Unlocked)
            _currentRune = CurrentRune(_runes[i]);
    }

    private Rune CurrentRune(Rune newRune)
    {
        if (!_currentRune)
        {
            _currentRune = newRune;
            _currentRune.EnableRune();
        }
        else if (_currentRune != newRune)
        {
            _currentRune.Disable();
            _currentRune = newRune;
            _currentRune.EnableRune();
        }

        return _currentRune;
    }

    private void UnlockingRune()
    {
        if(GetComponent<Link>().ControlCar)
        {
            _unlocking = false;

            int rune = (int)(_runeUnlocked.GetComponent<UnlockRune>().rune) + 1;
            _runes[rune].Unlocked = true;
            UNLOCKED_RUNES[rune] = true;
            if (_runeUnlocked.GetComponent<UnlockRune>().rune == 0)
            {
                _runes[0].Unlocked = true;
                UNLOCKED_RUNES[0] = true;
            }
            _runeUnlocked.GetComponent<UnlockRune>().TurnOffIcon();

            GetComponent<Link>().ForceShowRunesHelp();

            Tutorial.GetInstance.PlayTutorial("DisableRuneHelp");
        }
    }

    private void CheckAllRunesUnlocked()
    {
        RUNES_UNLOCKED = true;
        foreach (bool b in UNLOCKED_RUNES)
        {
            if (!b)
            {
                RUNES_UNLOCKED = false;
                break;
            }
        }
        if (RUNES_UNLOCKED)
        {
            ShowMessage.GetInstance.PlayMessage(
                "You have proved your worth. Fuel tank has been extended.\n\n Now, go forth, " +
                "and find the remaining Shrines in the land of Hyoctrule.\n\nThen, come back to this plateau to fulfill your purpose.", 20);
        }
    }

    public List<Rune> GetRunes
    {
        get { return _runes; }
    }

    void OnTriggerEnter(Collider col)
    {
        if(col.name.Contains("Rune"))
        {
            _unlocking = true;
            _runeUnlocked = col.gameObject;
            if (_currentRune)
                _currentRune.Disable();
        }
        if(col.tag == "GasStation")
        {
            foreach (Rune r in _runes)
            {
                if (r.IsCooldown)
                    Tutorial.GetInstance.PlayTutorial("RuneRecovery");
                r.FinishCooldown = true;
            }
        }
    }
}
