using System;
using System.Collections.Generic;
using DynamicSolver.CoreMath.Syntax.Model;

namespace DynamicSolver.CoreMath.Syntax
{
    public class BinaryOperatorsPriorityComparer : IComparer<IBinaryOperator>
    {
        private int GetPriority(IBinaryOperator op)
        {
            if (op is AssignmentBinaryOperator)
                return 0;

            if (op is AddBinaryOperator || op  is SubtractBinaryOperator)
                return 1;

            if (op is MultiplyBinaryOperator || op is DivideBinaryOperator)
                return 2;

            if (op is PowBinaryOperator)
                return 3;

            throw new ArgumentOutOfRangeException(nameof(op), op, $"Argument of type {op.GetType()} not supported.");
        }

        public int Compare(IBinaryOperator x, IBinaryOperator y)
        {
            return System.Math.Sign(GetPriority(x) - GetPriority(y));
        }
    }
}