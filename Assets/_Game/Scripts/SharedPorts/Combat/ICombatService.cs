using BillGameCore.Core.Combat;
using BillGameCore.Core.ValueObjects;

namespace BillGameCore.SharedPorts.Combat
{
    public interface ICombatService
    {
        /// <summary>Cận chiến — xử lý damage trực tiếp lên receiver.</summary>
        void RequestAttack(EntityId attackerId, IDamageReceiver target, string weaponId);

        /// <summary>Tầm xa — spawn projectile; damage tính khi va chạm.</summary>
        void RequestRangedAttack(EntityId attackerId, float dirX, float dirY, string weaponId);

        /// <summary>Gọi từ ProjectilePresenter khi projectile trúng mục tiêu.</summary>
        void ResolveProjectileHit(EntityId attackerId, IDamageReceiver target, string projectileId);
    }
}