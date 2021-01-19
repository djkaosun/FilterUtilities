using System;
using System.ComponentModel;
using mylib.FilterCriteria;
using Xunit;

namespace FilterUtilities.Tests
{
    public class ObjectFilterTests
    {
        [Fact]
        public void Filter_SetOwnedFilter_SetCorrect()
        {
            // arrange
            var hFilter = new HierarchicalFilter<object>();
            var oFilter = new ObjectFilter<object>();

            // act
            hFilter.OwnedFilter = oFilter;

            // assert
            Assert.Equal(hFilter, hFilter.OwnedFilter.Owner);
            Assert.Equal(oFilter, hFilter.OwnedFilter);
        }

        [Fact]
        public void Match_SameObject_ReturnsTrue()
        {
            // arrange
            var aObject = new Object();
            var objectFilter = new ObjectFilter<object>()
            {
                FilterObject = aObject
            };

            // act
            var actual = objectFilter.Match(aObject);

            // assert
            Assert.True(actual);
        }

        [Fact]
        public void Match_DifferentObject_ReturnsFalse()
        {
            // arrange
            var objectFilter = new ObjectFilter<object>()
            {
                FilterObject = new object()
            };

            // act
            var actual = objectFilter.Match(new Object());

            // assert
            Assert.False(actual);
        }

        [Fact]
        public void FilterString_Changed_PropertyChangedEventOccurs()
        {
            // arrange
            var objectFilter = new ObjectFilter<object>();
            string propertyName = String.Empty;
            objectFilter.PropertyChanged += (object sender, PropertyChangedEventArgs e) => { propertyName = e.PropertyName; };

            // act
            objectFilter.FilterObject = new Object();

            // assert
            Assert.Equal("Object", propertyName);
        }
    }
}
