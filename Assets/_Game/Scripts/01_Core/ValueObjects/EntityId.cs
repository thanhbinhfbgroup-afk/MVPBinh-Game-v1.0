using System;

namespace BillGameCore.Core.ValueObjects
{
    public readonly struct EntityId : IEquatable<EntityId>
    {
        private readonly Guid _value;

        private EntityId(Guid value)
        {
            _value = value;
        }

        public static EntityId Invalid => default;

        public bool IsValid => _value != Guid.Empty;

        public static EntityId New()
        {
            return new EntityId(Guid.NewGuid());
        }

        public bool Equals(EntityId other)
        {
            return _value.Equals(other._value);
        }

        public override bool Equals(object obj)
        {
            return obj is EntityId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            if (!IsValid)
            {
                return "Invalid";
            }

            var text = _value.ToString("N");
            return text.Substring(0, 8);
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