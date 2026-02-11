using UnityEngine;

public abstract class DragonComponents
{
    public DragonController dragonController { get; private set; }

    public virtual void Initialize(DragonController dc)
    {
        dragonController = dc;
    }

    public abstract void Start();
    public abstract void Update();
    public abstract void DrawGizmos();
}
