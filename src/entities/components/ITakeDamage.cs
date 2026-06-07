public interface ITakeDamage
{
    float Health { set; get; }
    bool Invincible { set; get; }
    Node3D Node
    {
        get => (Node3D)this;
    }
    void TakeDamage(float amount, Vec3 knockback, Node3D source);
}
