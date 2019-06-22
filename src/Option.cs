using System;

namespace PoshCommander
{
    public struct Option<T> : IEquatable<Option<T>>, IEquatable<T>
    {
        public bool HasValue { get; }

        private readonly T value;
        public T Value
            => HasValue
            ? value
            : throw new InvalidOperationException("Option does not have a value");

        public Option(T value)
        {
            this.HasValue = true;
            this.value = value;
        }

        public override bool Equals(object obj)
            => (obj is Option<T> other && this.Equals(other))
            || (obj is T otherValue && this.Equals(otherValue))
            || (obj is None && !this.HasValue);

        public bool Equals(Option<T> other)
            => other.HasValue == this.HasValue
            && object.Equals(other.Value, this.Value);

        public bool Equals(T other)
            => HasValue && object.Equals(Value, other);

        public override int GetHashCode()
            => HasValue
            ? Value?.GetHashCode() ?? int.MinValue
            : 0;

        public Option<T> DefaultIfEmpty(T defaultValue)
            => HasValue
            ? this
            : new Option<T>(defaultValue);

        public Option<T> Map(Func<T, T> mapper)
            => HasValue
            ? Option.Some(mapper(Value))
            : this;

        public override string ToString()
            => HasValue
            ? Value?.ToString() ?? string.Empty
            : string.Empty;

        public static implicit operator Option<T>(None none)
            => new Option<T>();
    }

    public struct None
    {
    }

    public static class Option
    {
        public static None None
            => new None();

        public static Option<T> Some<T>(T value)
            => new Option<T>(value);
    }
}
