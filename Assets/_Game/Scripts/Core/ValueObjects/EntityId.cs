namespace BillGameCore.Core.ValueObjects
{
    // ADR-04: Dùng Guid — duy nhất tuyệt đối, không cần static counter,
    //         an toàn khi spawn song song.
    // R18: EntityId.New() CHỈ được gọi từ class Spawner.
    public readonly struct EntityId : System.IEquatable<EntityId>
    {
        public static readonly EntityId Invalid = new EntityId(System.Guid.Empty);

        /// <summary>R18: chỉ gọi từ Spawner.Spawn().</summary>
        public static EntityId New() => new EntityId(System.Guid.NewGuid());

        private EntityId(System.Guid value) { Value = value; }

        public System.Guid Value   { get; }
        public bool        IsValid => Value != System.Guid.Empty;

        public bool   Equals(EntityId other)    => Value == other.Value;
        public override bool Equals(object obj) => obj is EntityId e && Equals(e);
        public override int  GetHashCode()      => Value.GetHashCode();
        public override string ToString()       => Value.ToString("N").Substring(0, 8);

        public static bool operator ==(EntityId a, EntityId b) =>  a.Equals(b);
        public static bool operator !=(EntityId a, EntityId b) => !a.Equals(b);
    }
}