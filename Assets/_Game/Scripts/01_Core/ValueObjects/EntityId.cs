using System;

namespace BillGameCore.Core.ValueObjects
{
    public readonly struct EntityId : IEquatable<EntityId>
    {
        public static EntityId Empty => new EntityId(Guid.Empty);

        public Guid Value { get; }

        private EntityId(Guid value)
        {
            Value = value;
        }

        public static EntityId New()
        {
            return new EntityId(Guid.NewGuid());
        }

        public bool Equals(EntityId other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is EntityId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static bool operator ==(EntityId left, EntityId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EntityId left, EntityId right)
        {
            return !left.Equals(right);
        }
    }
}