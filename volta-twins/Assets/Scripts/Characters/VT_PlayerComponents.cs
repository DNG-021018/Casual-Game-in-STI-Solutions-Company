namespace VoltaTwins
{
    public abstract class VT_PlayerComponents
    {
        public VT_PlayerController controller { get; private set; }

        public virtual void Initialized(VT_PlayerController controller)
        {
            this.controller = controller;
        }

        public virtual void PlayerOnEnable() { }
        public virtual void PlayerOnDisable() { }
        public virtual void PlayerStart() { }
        public virtual void PlayerUpdate() { }
        public virtual void PlayerFixedUpdate() { }
    }
}
