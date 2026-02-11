public abstract class CharacterComponents
{
    public PlayerController characterController { get; private set; }

    public virtual void Initialize(PlayerController pc)
    {
        characterController = pc;
    }

    public abstract void Update();
    public abstract void OnEnable();
    public abstract void OnDisable();
}