using System;

namespace Core.Interfaces
{
    public interface IDamageable
    {
        bool IsAlive { get; }
        void TakeDamage(float damage);
        event Action OnDeath;
        event Action<float> OnHealthChanged;
    }
}