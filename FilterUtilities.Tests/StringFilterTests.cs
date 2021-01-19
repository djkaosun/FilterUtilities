using System;
using System.ComponentModel;
using mylib.FilterCriteria;
using Xunit;

namespace FilterUtilities.Tests
{
    public class StringFilterTests
    {
        [Fact]
        public void Filter_SetOwnedFilter_SetCorrect()
        {
            // arrange
            var hFilter = new HierarchicalFilter<string>();
            var sFilter = new StringFilter();

            // act
            hFilter.OwnedFilter = sFilter;

            // assert
            Assert.Equal(hFilter, hFilter.OwnedFilter.Owner);
            Assert.Equal(sFilter, hFilter.OwnedFilter);
        }

        [Fact]
        public void Match_AndMatch_ReturnsTrue()
        {
            // arrange
            var stringFilter = new StringFilter()
            {
                FilterObject = "aaa bbb"
            };
            var item = "caaabbbc";

            // act
            var actual = stringFilter.Match(item);

            // assert
            Assert.True(actual);
        }

        [Fact]
        public void Match_AndUnmatch_ReturnsFalse()
        {
            // arrange
            var stringFilter = new StringFilter()
            {
                FilterObject = "aaa bbb"
            };
            var item = "cbbbc";

            // act
            var actual = stringFilter.Match(item);

            // assert
            Assert.False(actual);
        }

        [Fact]
        public void Match_OrMatch_ReturnsTrue()
        {
            // arrange
            var stringFilter = new StringFilter()
            {
                FilterObject = "aaa OR bbb"
            };
            var item = "cbbbc";

            // act
            var actual = stringFilter.Match(item);

            // assert
            Assert.True(actual);
        }

        [Fact]
        public void Match_OrUnmatch_ReturnsFalse()
        {
            // arrange
            var stringFilter = new StringFilter()
            {
                FilterObject = "aaa OR bbb"
            };
            var item = "caabbc";

            // act
            var actual = stringFilter.Match(item);

            // assert
            Assert.False(actual);
        }

        [Fact]
        public void Match_NotMatch_ReturnsFalse()
        {
            // arrange
            var stringFilter = new StringFilter()
            {
                FilterObject = "-aaa"
            };
            var item = "aaa";

            // act
            var actual = stringFilter.Match(item);

            // assert
            Assert.False(actual);
        }

        [Fact]
        public void Match_NotUnatch_ReturnsTrue()
        {
            // arrange
            var stringFilter = new StringFilter()
            {
                FilterObject = "-aaa"
            };
            var item = "bbb";

            // act
            var actual = stringFilter.Match(item);

            // assert
            Assert.True(actual);
        }

        [Fact]
        public void Match_ContainsAndOr_PreffersAnd()
        {
            // arrange
            var stringFilter = new StringFilter()
            {
                FilterObject = "aaa OR bbb ccc"
            };
            var item = "aaa";

            // act
            var actual = stringFilter.Match(item);

            // assert
            Assert.True(actual);
        }

        [Fact]
        public void Match_ContainsNest_PreffersNest()
        {
            // arrange
            var stringFilter = new StringFilter()
            {
                FilterObject = "(aaa OR bbb) ccc"
            };
            var item = "aaa";

            // act
            var actual = stringFilter.Match(item);

            // assert
            Assert.False(actual);
        }

        [Fact]
        public void Match_ContainsQoute_TreatedAsStringMatch()
        {
            // arrange
            var stringFilter = new StringFilter()
            {
                FilterObject = "\"aaa OR bbb ccc\""
            };
            var item = "aaa OR bbb ccc";

            // act
            var actual = stringFilter.Match(item);

            // assert
            Assert.True(actual);
        }

        [Fact]
        public void Match_ContainsQoute_TreatedAsStringUnmatch()
        {
            // arrange
            var stringFilter = new StringFilter()
            {
                FilterObject = "\"aaa OR bbb ccc\""
            };
            var item = "aaa";

            // act
            var actual = stringFilter.Match(item);

            // assert
            Assert.False(actual);
        }

        [Fact]
        public void Match_EndsWithSpaceChar_Ignore()
        {
            // arrange
            var stringFilter = new StringFilter()
            {
                FilterObject = "aaa "
            };
            var item = "bbb";

            // act
            var actual = stringFilter.Match(item);

            // assert
            Assert.False(actual);
        }

        [Fact]
        public void Match_MultipleCriteriaMatch_ReturnTrue()
        {
            // arrange
            var stringFilter = new StringFilter()
            {
                FilterObject = "(\"aaa\" OR \"bbb ccc\")((ddd)(eee OR fff))"
            };
            var item1 = "aaadddfff";
            var item2 = "bbb cccddd";

            // act
            var actual1 = stringFilter.Match(item1);
            var actual2 = stringFilter.Match(item2);

            // assert
            Assert.True(actual1);
            Assert.False(actual2);
        }

        [Fact]
        public void FilterString_Changed_PropertyChangedEventOccurs()
        {
            // arrange
            var stringFilter = new StringFilter();
            string propertyName = String.Empty;
            stringFilter.PropertyChanged += (object sender, PropertyChangedEventArgs e) => { propertyName = e.PropertyName; };

            // act
            stringFilter.FilterObject = String.Empty;

            // assert
            Assert.Equal("FilterString", propertyName);
        }
    }
}
