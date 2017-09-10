﻿using System;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model
{
    public class AddOperation : IValueSource
    {
        [NotNull]
        public IValueSource Left { get; }

        [NotNull]
        public IValueSource Right { get; }

        public AddOperation([NotNull] IValueSource left, [NotNull] IValueSource right)
        {
            if (right == null) throw new ArgumentNullException(nameof(right));

            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right;
        }

        public T Accept<T>([NotNull] ISemanticVisitor<T> visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            return visitor.Visit(this);
        }
    }
}