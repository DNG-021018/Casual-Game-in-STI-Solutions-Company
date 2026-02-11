using UnityEngine;

public static class ValidationUtils
{
    public static void CheckNull<T>(T obj, string errorMessage)
    {
        if (obj == null)
        {
            Debug.LogError(errorMessage);
        }
    }
}
