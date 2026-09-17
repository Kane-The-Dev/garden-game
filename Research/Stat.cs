using System.Collections.Generic;
using UnityEngine;

// Base + flat
// Base * (1f + percentAdd1 + percentAdd2 + ...)
// Base * (1f + percentMult1) * (1f + percentMult2) * ...
public enum ModType { Flat, PercentAdd, PercentMult }

public struct Modifier // buff or nerf
{
    public object source; // where the buff came from
    public ModType type;
    public float value;
    public float expireTime; // Time.time value when it dies. -1 = permanent.

    public Modifier(object source, ModType type, float value, float duration = -1f)
    {
        this.source = source;
        this.type = type;
        this.value = value;
        this.expireTime = duration < 0f ? -1f : Time.time + duration;
    }

    public bool IsExpired => expireTime >= 0f && Time.time >= expireTime;
}

[System.Serializable]
public class Stat
{
    [SerializeField] private float _baseValue;
    public float baseValue // adjusting the public baseValue would trigger flag
    {
        get => _baseValue;
        set
        {
            _baseValue = value;
            flag = true;
        }
    }

    List<Modifier> modifiers = new List<Modifier>();
    float cachedValue;
    bool flag = true;
    float nextExpireCheck = float.MaxValue; // no checking until at least one modifier expires

    public Stat(float baseValue)
    {
        this.baseValue = baseValue;
    }

    public void AddModifier(Modifier mod)
    {
        modifiers.Add(mod);
        if (mod.expireTime >= 0f)
            nextExpireCheck = Mathf.Min(nextExpireCheck, mod.expireTime);
        flag = true;
    }

    // Remove every modifier that came from a given source
    public void RemoveModifiersFrom(object source)
    {
        int removed = modifiers.RemoveAll(m => m.source == source);
        if (removed > 0) flag = true;
    }

    // Value is only updated when it is accessed AND current time is after the expire check timemark
    public float Value
    {
        get
        {
            if (Time.time >= nextExpireCheck)
                ExpireModifiers();

            if (flag)
                Recalculate();

            return cachedValue;
        }
    }

    void ExpireModifiers()
    {
        nextExpireCheck = float.MaxValue;
        for (int i = modifiers.Count - 1; i >= 0; i--)
        {
            if (modifiers[i].IsExpired)
            {
                modifiers.RemoveAt(i);
                flag = true;
            }
            else if (modifiers[i].expireTime >= 0f)
            {
                nextExpireCheck = Mathf.Min(nextExpireCheck, modifiers[i].expireTime);
            }
        }
    }

    void Recalculate()
    {
        float flatSum = 0f;
        float percentAddSum = 0f;
        float percentMult = 1f;

        foreach (var m in modifiers)
        {
            switch (m.type)
            {
                case ModType.Flat: flatSum += m.value; break;
                case ModType.PercentAdd: percentAddSum += m.value; break;
                case ModType.PercentMult: percentMult *= 1f + m.value; break;
            }
        }

        cachedValue = (baseValue + flatSum) * (1f + percentAddSum) * percentMult;
        flag = false;
    }
}