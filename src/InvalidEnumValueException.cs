using System;

namespace PoshCommander
{
    public class InvalidEnumValueException<TEnum> : Exception
        where TEnum : Enum
    {
        public TEnum Value { get; set; }

        public InvalidEnumValueException(TEnum value)
            : base($"Value '{value} ({Convert.ToInt32(value)})' is not valid for type '{typeof(TEnum)}'")
        {
        }
    }
}
