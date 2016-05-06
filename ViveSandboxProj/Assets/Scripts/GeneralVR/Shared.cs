using UnityEngine;

public abstract class ValueIndicator : MonoBehaviour
{
    public abstract void SetValue(float value);
}

public static class Helpers
{
    public static bool RandomBool(float chance = 0.5f)
    {
        return Random.Range(0, 1) < chance;
    }
}
