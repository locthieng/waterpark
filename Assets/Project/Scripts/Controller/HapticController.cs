using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HapticType
{
    Warning,
    Failure,
    Success,
    Light,
    Medium,
    Heavy,
    Default,
    Vibrate,
    Selection
}

public static class HapticController
{
    public static void TriggerHaptic(HapticType type)
    {
        if (!GlobalController.IsHapticOn) return;
        switch (type)
        {
            case HapticType.Warning:
                Taptic.Warning();
                break;
            case HapticType.Failure:
                Taptic.Failure();
                break;
            case HapticType.Success:
                Taptic.Success();
                break;
            case HapticType.Light:
                Taptic.Light();
                break;
            case HapticType.Medium:
                Taptic.Medium();
                break;
            case HapticType.Heavy:
                Taptic.Heavy();
                break;
            case HapticType.Default:
                Taptic.Default();
                break;
            case HapticType.Vibrate:
                Taptic.Vibrate();
                break;
            case HapticType.Selection:
                Taptic.Selection();
                break;
            default:
                break;
        }
    }
}
