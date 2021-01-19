using System;
using System.ComponentModel;
using mylib.FilterCriteria;
using Xunit;

namespace FilterUtilities.Tests
{
    public class FolderFilterTests
    {
        [Fact]
        public void Filter_SetOwnedFilter_SetCorrect()
        {
            // arrange
            var hFilter = new HierarchicalFilter<object>();
            var fFilter = new FolderFilter<object>();

            // act
            hFilter.OwnedFilter = fFilter;

            // assert
            Assert.Equal(hFilter, hFilter.OwnedFilter.Owner);
            Assert.Equal(fFilter, hFilter.OwnedFilter);
        }

        [Fact]
        public void Match_CalledFromSubFilter_ReturnsParentMatchResult()
        {
            // arrange
            var childFilter = new HierarchicalFilter<string>();
            childFilter.OwnedFilter = new AnyPassFilter<string>();

            var hFilter = new HierarchicalFilter<string>();
            hFilter.OwnedFilter = new FolderFilter<string>();

            var parentFilter = new HierarchicalFilter<string>();
            parentFilter.OwnedFilter = new StringFilter()
            {
                FilterObject = "aaa"
            };

            parentFilter.Children.Add(hFilter);
            hFilter.Children.Add(childFilter);

            // act
            var actual1 = childFilter.Match("baaab");
            var actual2 = childFilter.Match("bb");


            // assert
            Assert.True(actual1);
            Assert.False(actual2);
        }

        [Fact]
        public void Match_CalledFromOthers_ReturnsParentAndChildMatchResult()
        {
            // arrange
            var childFilter1 = new HierarchicalFilter<string>();
            childFilter1.OwnedFilter = new StringFilter()
            {
                FilterObject = "aa"
            };
            var childFilter2 = new HierarchicalFilter<string>();
            childFilter2.OwnedFilter = new StringFilter()
            {
                FilterObject = "bb"
            };
            var childFilter3 = new HierarchicalFilter<string>();
            childFilter3.OwnedFilter = new StringFilter()
            {
                FilterObject = "cc"
            };

            var hFilter = new HierarchicalFilter<string>();
            hFilter.OwnedFilter = new FolderFilter<string>();

            var parentFilter = new HierarchicalFilter<string>();
            parentFilter.OwnedFilter = new StringFilter()
            {
                FilterObject = "aaa OR bbb"
            };

            parentFilter.Children.Add(hFilter);
            hFilter.Children.Add(childFilter1);
            hFilter.Children.Add(childFilter2);
            hFilter.Children.Add(childFilter3);

            // act
            var actual1 = hFilter.Match("aaa");
            var actual2 = hFilter.Match("bbb");
            var actual3 = hFilter.Match("ccc");


            // assert
            Assert.True(actual1);
            Assert.True(actual2);
            Assert.False(actual3);
        }
    }
}
