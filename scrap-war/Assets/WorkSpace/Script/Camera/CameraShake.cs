using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("Assign Cinemachine FreeLook here")]
    public CinemachineFreeLook cinemachineFreeLook;

    private float shakeTimer;
    private float shakeTimerTotal;
    private float startingIntensity;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShakeCamera(float intensity, float time)
    {
        if (cinemachineFreeLook == null)
        {
            Debug.LogError("CameraShake: CinemachineFreeLook is not assigned.");
            return;
        }

        for (int i = 0; i < 3; i++)
        {
            CinemachineBasicMultiChannelPerlin perlin = GetPerlinFromRig(i);
            if (perlin != null)
            {
                perlin.m_AmplitudeGain = intensity;
            }
        }

        startingIntensity = intensity;
        shakeTimer = shakeTimerTotal = time;
    }

    void Update()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            float currentIntensity = Mathf.Lerp(startingIntensity, 0f, 1 - (shakeTimer / shakeTimerTotal));

            for (int i = 0; i < 3; i++)
            {
                CinemachineBasicMultiChannelPerlin perlin = GetPerlinFromRig(i);
                if (perlin != null)
                {
                    perlin.m_AmplitudeGain = currentIntensity;
                }
            }
        }
    }

    private CinemachineBasicMultiChannelPerlin GetPerlinFromRig(int rigIndex)
    {
        if (cinemachineFreeLook == null) return null;

        CinemachineVirtualCamera rig = cinemachineFreeLook.GetRig(rigIndex);
        return rig?.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }
}
