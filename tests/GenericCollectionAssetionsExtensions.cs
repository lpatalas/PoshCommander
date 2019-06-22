using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Collections;

namespace PoshCommander.Tests
{
    public static class GenericCollectionAssetionsExtensions
    {
        public static AndConstraint<GenericCollectionAssertions<T>> BeInStrictOrder<T>(
            this GenericCollectionAssertions<T> assertions,
            params T[] expectation)
            => assertions.BeEquivalentTo(expectation, opt => opt.WithoutStrictOrdering());

        public static AndConstraint<GenericCollectionAssertions<T>> BeInStrictOrder<T>(
            this GenericCollectionAssertions<T> assertions,
            IEnumerable<T> expectation)
            => assertions.BeEquivalentTo(expectation, opt => opt.WithoutStrictOrdering());
    }
}
