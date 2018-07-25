using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Buff : MonoBehaviour
{
    public int activeMinutes;
    [Tooltip("0: speed, 1: jump, 2: climb, 3: fuel")]
    public List<Image> buffIcons = new List<Image>(4) { null, null, null, null };
    [Tooltip("0: speed, 1: jump, 2: climb, 4: fuel")]
    public List<float> buffValues = new List<float>(4) { 40, 35, 10, 3 };
    [Tooltip("0: speed, 1: jump, 2: climb, 4: fuel")]
    public List<bool> debugEnableBuff = new List<bool>(4) { false, false, false, false };

    private Link _playerScript;
    private Text _buffTimer;
    private bool _active, _redText;
    private string _currentText;
    private float _timer;
    private List<Transform> _buffUnlocks;

    /// <summary>
    /// 0: speed, 1: jump, 2: climb
    /// </summary>
    private List<float> _defaultValues;
    /// <summary>
    /// 0: speed, 1: jump, 2: climb, 3: fuel
    /// </summary>
    private List<string> _names;

    private static float TIMER;
    [Tooltip("0: speed, 1: jump, 2: climb, 3: fuel")]
    private static List<bool> ENABLE_BUFF;

    void Start ()
    {
        _playerScript = GetComponent<Link>();
        
        _buffTimer = GameObject.Find("BuffTimer").GetComponent<Text>();
        
        for (int i = 0; i < buffIcons.Count; i++)
            buffIcons[i].gameObject.SetActive(false);

        _defaultValues = new List<float>();
        _defaultValues.Add(_playerScript.maxSpeedDrive);
        _defaultValues.Add(_playerScript.jumpForceMultiplied);
        _defaultValues.Add(_playerScript.maxSpeedClimb);

        _names = new List<string>();
        _names.Add("Max speed increased");
        _names.Add("Jump force increased");
        _names.Add("Max climb speed increased");
        _names.Add("Fuel consumption decreased");

        _active = false;
        _buffTimer.text = "";

        _buffUnlocks = new List<Transform>();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Animate"))
        {
            if (go.name.Contains("Buff"))
                _buffUnlocks.Add(go.transform);
        }

        if (ENABLE_BUFF == null)
        {
            ENABLE_BUFF = new List<bool>();
            for (int i = 0; i < 4; i++)
                ENABLE_BUFF.Add(false);
        }

        StartCoroutine(DelaySetBuffAtStart());
        
        _redText = false;
    }

    private IEnumerator DelaySetBuffAtStart()
    {
        yield return new WaitForEndOfFrame();

        if (ENABLE_BUFF[0] || debugEnableBuff[0])
            EnableBuffSpeed(true);
        else if (ENABLE_BUFF[1] || debugEnableBuff[1])
            EnableBuffJump(true);
        else if (ENABLE_BUFF[2] || debugEnableBuff[2])
            EnableBuffClimb(true);
        else if (ENABLE_BUFF[3] || debugEnableBuff[3])
            EnableBuffFuel(true);
    }
    
    void Update ()
    {
        if(_active)
        {
            _timer -= Time.deltaTime;
            FormatBuffText();
            HandleTimer();
        }
    }

    private void HandleTimer()
    {
        if (_timer <= 20)
            CriticalRedText();
        if (_timer < 0)
        {
            ResetBuff();
            CheckBuffUnlocks();
        }
    }

    private void FormatBuffText()
    {
        string minutes = Mathf.Floor(_timer / 60).ToString("0");
        string seconds = Mathf.Floor(_timer % 60).ToString("00");
        _buffTimer.text = _currentText + "\n" + minutes + ":" + seconds;
    }

    private void CriticalRedText()
    {
        float speed = -0.01f;
        if (_redText)
            speed *= -1;

        _buffTimer.color += new Color(0, speed, speed);

        Color c = _buffTimer.color;
        c.g = Mathf.Clamp(c.g, 0, 1);
        c.b = Mathf.Clamp(c.b, 0, 1);
        _buffTimer.color = c;

        if (c.b == 0)
            _redText = true;
        else if (c.b == 1)
            _redText = false;
    }

    private void StartBuff()
    {
        _active = true;
        _timer = TIMER == 0 ? 60 * activeMinutes : TIMER;
        _buffTimer.color = Color.white;
        CheckBuffUnlocks();

        if(!GameplayHandler.IS_BOSS)
            Tutorial.GetInstance.PlayTutorial("Buff");
    }

    private void TurnOffPreviousBuff()
    {
        if (GetCurrentBuff < 0)
            ResetBuff();
    }

    public void ResetBuff()
    {
        switch(GetCurrentBuff)
        {
            case 0:
                EnableBuffSpeed(false);
                break;
            case 1:
                EnableBuffJump(false);
                break;
            case 2:
                EnableBuffClimb(false);
                break;
            case 3:
                EnableBuffFuel(false);
                break;
        }
    }

    private void TurnOffGUI()
    {
        _active = false;
        _buffTimer.text = "";
        _redText = false;
        _currentText = "";
        TIMER = 0;
    }

    private void StartOrEndBuff(bool enable)
    {
        if (enable)
            StartBuff();
        else
            TurnOffGUI();
    }

    public void CheckBuffUnlocks()
    {
        int activeBuff = GetCurrentBuff;
        if (activeBuff == -1)
        {
            for (int i = 0; i < _buffUnlocks.Count; i++)
            {
                if (!_buffUnlocks[i].gameObject.activeSelf)
                    _buffUnlocks[i].gameObject.SetActive(true);
            }
            return;
        }

        for (int i = 0; i < _buffUnlocks.Count; i++)
        {
            if ((int)_buffUnlocks[i].GetComponent<UnlockBuff>().buff == activeBuff)
                _buffUnlocks[i].gameObject.SetActive(false);
            else
            {
                if (!_buffUnlocks[i].gameObject.activeSelf)
                    _buffUnlocks[i].gameObject.SetActive(true);
            }
        }
    }

    public void EnableBuffSpeed(bool enable)
    {
        TurnOffPreviousBuff();

        _playerScript.maxSpeedDrive = enable ? buffValues[0] : _defaultValues[0];
        _currentText = _names[0];
        buffIcons[0].gameObject.SetActive(enable);
        StartOrEndBuff(enable);
    }

    public void EnableBuffJump(bool enable)
    {
        TurnOffPreviousBuff();

        _playerScript.jumpForceMultiplied = enable ? buffValues[1] : _defaultValues[1];
        _currentText = _names[1];
        buffIcons[1].gameObject.SetActive(enable);
        StartOrEndBuff(enable);
    }

    public void EnableBuffClimb(bool enable)
    {
        TurnOffPreviousBuff();

        _playerScript.maxSpeedClimb = enable ? buffValues[2] : _defaultValues[2];
        _currentText = _names[2];
        buffIcons[2].gameObject.SetActive(enable);
        StartOrEndBuff(enable);
    }

    public void EnableBuffFuel(bool enable)
    {
        TurnOffPreviousBuff();

        if(enable)
            GetComponent<Fuel>().SetFullFuel();
        _currentText = _names[3];
        buffIcons[3].gameObject.SetActive(enable);
        StartOrEndBuff(enable);
    }

    public void SaveStats()
    {
        for (int i = 0; i < ENABLE_BUFF.Count; i++)
            ENABLE_BUFF[i] = false;
        for(int i = 0; i < buffIcons.Count; i++)
        {
            if (buffIcons[i].gameObject.activeSelf)
            {
                ENABLE_BUFF[i] = true;
                break;
            }
        }

        TIMER = _timer;
    }

    /// <summary>
    /// 0: speed, 1: jump, 2: climb, 3: fuel
    /// </summary>
    private bool GetActiveBuff(int i)
    {
        return buffIcons[i].gameObject.activeSelf;
    }

    /// <summary>
    /// -1: no buff, 0: speed, 1: jump, 2: climb, 3: fuel
    /// </summary>
    private int GetCurrentBuff
    {
        get
        {
            int active = -1;
            for(int i = 0; i < buffIcons.Count; i++)
            {
                if(buffIcons[i].gameObject.activeSelf)
                {
                    active = i;
                    break;
                }

            }
            return active;
        }
    }

    public bool BuffedJumpForce
    {
        get { return buffIcons[1].gameObject.activeSelf; }
    }

    public bool BuffedFuelSpend
    {
        get { return buffIcons[3].gameObject.activeSelf; }
    }

    public bool NotFuelBuff
    {
        get { return GetCurrentBuff != -1 && GetCurrentBuff != 3; }
    }
}
