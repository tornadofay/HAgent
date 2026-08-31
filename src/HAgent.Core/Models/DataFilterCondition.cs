using System;

namespace HAgent.Models
{
    /// <summary>
    /// One structured field comparison. It contains no SQL, expressions, or executable code.
    /// </summary>
    public sealed class DataFilterCondition
    {
        public string Field { get; set; }
        public DataQueryOperator Operator { get; set; }
        public object Value { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Field))
                throw new ArgumentException("Filter field is required.", nameof(Field));

            if (Operator == DataQueryOperator.IsNull || Operator == DataQueryOperator.IsNotNull)
            {
                if (Value != null)
                    throw new ArgumentException("Null-check operators must not specify a value.", nameof(Value));
                return;
            }

            if (Value == null)
                throw new ArgumentException("A value is required for this filter operator.", nameof(Value));

            if (!IsSupportedScalar(Value))
                throw new ArgumentException("Filter values must be scalar values.", nameof(Value));
        }

        private static bool IsSupportedScalar(object value)
        {
            var type = value.GetType();
            return type.IsPrimitive || type.IsEnum || value is string || value is decimal ||
                   value is DateTime || value is DateTimeOffset || value is TimeSpan || value is Guid;
        }
    }
}
