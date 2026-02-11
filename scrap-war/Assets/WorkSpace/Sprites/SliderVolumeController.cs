
using UnityEngine;
using UnityEngine.UI;

public class SliderVolumeController : MonoBehaviour
{
    public Slider volumeSlider;
    public GameObject segmentPrefab;
    public Transform segmentGroup;

    public int segmentCount = 20;

    private Image[] segmentImages;

    private void Start()
    {
        GenerateSegments();
        UpdateSegments(volumeSlider.value);
        volumeSlider.onValueChanged.AddListener(UpdateSegments);
    }

    void GenerateSegments()
    {
        segmentImages = new Image[segmentCount];
        for (int i = 0; i < segmentCount; i++)
        {
            GameObject seg = Instantiate(segmentPrefab, segmentGroup);
            segmentImages[i] = seg.GetComponent<Image>();
        }
    }

    void UpdateSegments(float value)
    {
        int activeSegments = Mathf.RoundToInt(value * segmentCount);

        for (int i = 0; i < segmentCount; i++)
        {
            if (i < activeSegments)
            {
                float percent = (float)i / segmentCount;
                if (percent < 0.3f)
                    segmentImages[i].color = Color.red;
                else if (percent < 0.6f)
                    segmentImages[i].color = new Color(1f, 0.65f, 0f); // yellow-orange
                else
                    segmentImages[i].color = Color.green;
            }
            else
            {
                segmentImages[i].color = Color.black * 0.4f;
            }
        }
    }
}
