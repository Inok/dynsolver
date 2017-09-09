using System;
using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Syntax
{
    public sealed class FunctionCall : IFunctionCall, IEquatable<FunctionCall>
    {
        public string FunctionName { get; }

        public IExpression Argument { get; set; }

        public FunctionCall(string functionName, [NotNull] IExpression argument)
        {
            if (string.IsNullOrEmpty(functionName)) throw new ArgumentException("Argument is null or empty", nameof(functionName));
            if (argument == null) throw new ArgumentNullException(nameof(argument));

            FunctionName = functionName;
            Argument = argument;
        }

        public override string ToString()
        {
            return $"{FunctionName}({Argument})";
        }

        public bool Equals(FunctionCall other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(FunctionName, other.FunctionName) && Argument.Equals(other.Argument);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is FunctionCall && Equals((FunctionCall) obj);
        }

        public bool Equals(IExpression other)
        {
            return this.Equals((object)other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (FunctionName.GetHashCode()*397) ^ Argument.GetHashCode();
            }
        }
    }
}