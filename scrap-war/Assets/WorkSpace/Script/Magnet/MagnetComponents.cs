using UnityEngine;

public abstract class MagnetComponent
{
    public MagnetController magnetController { get; private set; }

    public virtual void Initialize(MagnetController controller)
    {
        magnetController = controller;
    }

    public abstract void Update();
    public abstract void OnTriggerStay(Collider other);
    public abstract void OnTriggerExit(Collider other);
}