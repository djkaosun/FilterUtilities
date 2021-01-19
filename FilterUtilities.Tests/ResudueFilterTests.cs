using System;
using System.ComponentModel;
using mylib.FilterCriteria;
using Xunit;

namespace FilterUtilities.Tests
{
    public class ResudueFilterTests
    {
        [Fact]
        public void Filter_SetOwnedFilter_SetCorrect()
        {
            // arrange
            var hFilter = new HierarchicalFilter<object>();
            var rFilter = new ResudueFilter<object>();

            // act
            hFilter.OwnedFilter = rFilter;

            // assert
            Assert.Equal(hFilter, hFilter.OwnedFilter.Owner);
            Assert.Equal(rFilter, hFilter.OwnedFilter);
        }

        [Fact]
        public void Match_PassBrotherFilter_ReturnsFalse()
        {
            // arrange
            var hFilter = new HierarchicalFilter<string>()
            {
                OwnedFilter = new ResudueFilter<string>()
            };

            var broFilter1 = new HierarchicalFilter<string>()
            {
                OwnedFilter = new StringFilter() { FilterObject = "aaa" }
            };

            var broFilter2 = new HierarchicalFilter<string>()
            {
                OwnedFilter = new StringFilter() { FilterObject = "bbb" }
            };

            var broFilter3 = new HierarchicalFilter<string>()
            {
                OwnedFilter = new ResudueFilter<string>()
            };

            var parentFilter = new HierarchicalFilter<string>()
            {
                OwnedFilter = new StringFilter() { FilterObject = "a OR c" }
            };

            parentFilter.Children.Add(hFilter);
            parentFilter.Children.Add(broFilter1);
            parentFilter.Children.Add(broFilter2);
            parentFilter.Children.Add(broFilter3);

            // act
            var actual = hFilter.Match("aaa");

            // assert
            Assert.False(actual);
        }

        [Fact]
        public void Match_FailBrotherFilterButParentFailToo_ReturnsFalse()
        {
            // arrange
            var hFilter = new HierarchicalFilter<string>()
            {
                OwnedFilter = new ResudueFilter<string>()
            };

            var broFilter1 = new HierarchicalFilter<string>()
            {
                OwnedFilter = new StringFilter() { FilterObject = "aaa" }
            };

            var broFilter2 = new HierarchicalFilter<string>()
            {
                OwnedFilter = new StringFilter() { FilterObject = "bbb" }
            };

            var broFilter3 = new HierarchicalFilter<string>()
            {
                OwnedFilter = new ResudueFilter<string>()
            };

            var parentFilter = new HierarchicalFilter<string>()
            {
                OwnedFilter = new StringFilter() { FilterObject = "a OR c" }
            };

            parentFilter.Children.Add(hFilter);
            parentFilter.Children.Add(broFilter1);
            parentFilter.Children.Add(broFilter2);
            parentFilter.Children.Add(broFilter3);

            // act
            var actual = hFilter.Match("bbb");

            // assert
            Assert.False(actual);
        }

        [Fact]
        public void Match_FailBrotherFilterAndParentPass_ReturnsTrue()
        {
            // arrange
            var hFilter = new HierarchicalFilter<string>()
            {
                OwnedFilter = new ResudueFilter<string>()
            };

            var broFilter1 = new HierarchicalFilter<string>()
            {
                OwnedFilter = new StringFilter() { FilterObject = "aaa" }
            };

            var broFilter2 = new HierarchicalFilter<string>()
            {
                OwnedFilter = new StringFilter() { FilterObject = "bbb" }
            };

            var broFilter3 = new HierarchicalFilter<string>()
            {
                OwnedFilter = new ResudueFilter<string>()
            };

            var parentFilter = new HierarchicalFilter<string>()
            {
                OwnedFilter = new StringFilter() { FilterObject = "a OR c" }
            };

            parentFilter.Children.Add(hFilter);
            parentFilter.Children.Add(broFilter1);
            parentFilter.Children.Add(broFilter2);
            parentFilter.Children.Add(broFilter3);

            // act
            var actual = hFilter.Match("ccc");

            // assert
            Assert.True(actual);
        }
    }
}
