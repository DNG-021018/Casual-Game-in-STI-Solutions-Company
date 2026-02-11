using System.Collections.Generic;
using UnityEngine;

public class MagnetController : MonoBehaviour
{
    [Header("Magnet Data")]
    public MagnetData magnetData;

    [Header("References")]
    public Transform itemsHolder;

    [Space(10)]
    [Header("UI")]
    public PullButton pullButton;
    public ShootButton shootButton;

    [SerializeField] private Magnet MagnetComponent;
    [SerializeField] private MagnetSound MagnetSound;
    [SerializeField] private MagnetEffect EffectComponent;

    public Magnet _magnetComponent => MagnetComponent;
    public MagnetEffect _effectComponent => EffectComponent;
    public MagnetSound _soundComponent => MagnetSound;

    private List<MagnetComponent> _components = new List<MagnetComponent>();

    void Awake()
    {
        _components.Clear();
        _components.Add(MagnetComponent);
        _components.Add(EffectComponent);
        _components.Add(MagnetSound);

        foreach (MagnetComponent c in _components)
        {
            c.Initialize(this);
        }
    }

    void Start()
    {
        ValidationUtils.CheckNull(pullButton, $"[MagnetController] ---> shootButton is null");
        ValidationUtils.CheckNull(shootButton, $"[MagnetController] ---> shootButton is null");
    }

    void Update()
    {
        foreach (MagnetComponent c in _components)
        {
            c.Update();
        }

        bool hasItem = itemsHolder.childCount != 0;
        pullButton.gameObject.SetActive(!hasItem);
        shootButton.gameObject.SetActive(hasItem);

        if (hasItem)
        {
            _magnetComponent.SetPullState(false);
            _effectComponent.StopPullEffect();
        }
    }

    void OnTriggerStay(Collider other)
    {
        foreach (MagnetComponent c in _components)
        {
            if (c is Magnet magnet)
            {
                magnet.OnTriggerStay(other);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        foreach (MagnetComponent c in _components)
        {
            if (c is Magnet pull)
            {
                pull.OnTriggerExit(other);
            }
        }
    }
}
