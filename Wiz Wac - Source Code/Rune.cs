using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Rune : MonoBehaviour
{
    protected Transform _mainCamera;
    protected KeyCode _enableKey;
    protected Image _icon;
    protected RectTransform _cooldownMeter;
    protected float _cooldownY, _cooldownX, _defaultCooldownX;
    protected float _cooldownTime;
    protected bool _unlocked, _finishCooldown;
    
    virtual public void Run()
    {

    }

    virtual public void EnableRune()
    {
        _icon.enabled = true;
    }

    virtual public void Disable()
    {
        _icon.enabled = false;
    }

    virtual public void SetMainCamera(Transform camera)
    {
        _mainCamera = camera;
    }

    virtual public void SetIcon(Image icon)
    {
        _icon = icon;
        _icon.enabled = false;
    }

    virtual public void SetCooldownMeter(RectTransform cooldown)
    {
        _cooldownMeter = cooldown;
        _cooldownMeter.gameObject.SetActive(false);
        _cooldownY = _cooldownMeter.localScale.y;
        _defaultCooldownX = _cooldownX = _cooldownMeter.localScale.x;
    }

    virtual public float CooldownTime
    {
        set { _cooldownTime = value; }
        get { return _cooldownTime; }
    }

    virtual public KeyCode EnableKey
    {
        set { _enableKey = value; }
        get { return _enableKey; }
    }

    virtual public bool Unlocked
    {
        set { _unlocked = value; }
        get { return _unlocked; }
    }

    virtual public bool IsCooldown
    {
        get { return false; }
    }

    virtual public bool FinishCooldown
    {
        set { _finishCooldown = value; }
        get { return _finishCooldown; }
    }

    virtual public bool Active
    {
        get { return _icon.enabled; }
    }

    virtual protected bool UseRuneButton
    {
        get { return PlayerInput.UseRune; }
    }

    virtual protected bool AltRuneButton
    {
        get { return PlayerInput.AltRune; }
    }
}
