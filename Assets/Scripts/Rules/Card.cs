using System;

namespace MagicMayhem.Rules
{
    /// <summary>One spell card: an element and a strength from 0 to 9.</summary>
    public readonly struct Card : IEquatable<Card>
    {
        public const int MinValue = 0;
        public const int MaxValue = 9;

        public Element Element { get; }
        public int Value { get; }

        public Card(Element element, int value)
        {
            if (!Enum.IsDefined(typeof(Element), element))
                throw new ArgumentOutOfRangeException(nameof(element));
            if (value < MinValue || value > MaxValue)
                throw new ArgumentOutOfRangeException(nameof(value), $"A card's value is {MinValue} to {MaxValue}.");
            Element = element;
            Value = value;
        }

        public bool Equals(Card other) => Element == other.Element && Value == other.Value;
        public override bool Equals(object obj) => obj is Card other && Equals(other);
        public override int GetHashCode() => (int)Element * 10 + Value;
        public override string ToString() => $"{Element} {Value}";
    }
}
