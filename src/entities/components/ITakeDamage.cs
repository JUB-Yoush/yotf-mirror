namespace Yotf;

public interface ITakeDamage
{
    float Health { set; get; }
    bool Invincible { set; get; }
    bool IsDead { set; get; }
    Node3D Node
    {
        get => (Node3D)this;
    }

    MeshInstance3D Mesh
    {
        get => ((Fish)this).Mesh;
    }
    virtual void TakeDamage(float amount, Vec3 knockback, Node3D source)
    {
        if (this.Invincible)
            return;

        var charBody = this as CharacterBody3D; // default implementation assumes characterbody
        charBody!.Velocity += knockback;
        Health = Mathf.Max(0, Health - amount);
        if (Health == 0)
        {
            IsDead = true;
            OnDeath();
        }
        else
        {
            OnDamageTaken(amount, knockback, source);
        }
    }
    virtual void OnDamageTaken(float amount, Vec3 knockback, Node3D source) { }
    virtual void OnDeath()
    {
        // flip their model upside down
        Mesh.RotateY(180);
        //Mesh.GlobalRotate()
        var charBody = this as CharacterBody3D; // default implementation assumes characterbody
        charBody!
            .CreateTween()
            .AnimateProperty(charBody, CharacterBody3D.PropertyName.Velocity, Vec3.Zero, .5f);
    }
}
