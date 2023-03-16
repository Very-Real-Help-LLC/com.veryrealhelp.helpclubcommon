using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformSpecificObject : MonoBehaviour
{
    [Flags] public enum Platform { None = 0, XR = 1, PC = 1<<1, Touch = 1<<2 }

    public Platform platforms;

    public static event Action<PlatformSpecificObject> OnAwake;

    private void Awake()
    {
        OnAwake?.Invoke(this);
    }
}
