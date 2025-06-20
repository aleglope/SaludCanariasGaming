using System.Collections.Generic;
using UnityEngine;

public static class InputSimulator
{
    private static readonly HashSet<KeyCode> simulatedKeys = new HashSet<KeyCode>();

    public static void SimulateKeyDown(KeyCode key)
    {
        simulatedKeys.Add(key);
    }

    public static bool GetKeyDown(KeyCode key)
    {
        if (simulatedKeys.Contains(key))
        {
            simulatedKeys.Remove(key);
            return true;
        }

        return Input.GetKeyDown(key);
    }
}
