using BillGameCore.Core.Combat;
using BillGameCore.Core.ValueObjects;

namespace BillGameCore.SharedPorts.Combat
{
    public interface ICombatService
    {
        /// <summary>Melee — resolves damage directly onto the receiver.</summary>
        void RequestAttack(EntityId attackerId, IDamageReceiver target, string weaponId);

        /// <summary>Ranged — spawns a projectile; damage resolved on collision.</summary>
        void RequestRangedAttack(EntityId attackerId, float dirX, float dirY, string weaponId);

        /// <summary>Called from ProjectilePresenter when the projectile hits a target.</summary>
        void ResolveProjectileHit(EntityId attackerId, IDamageReceiver target, string projectileId);
    }
}