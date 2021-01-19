using System;
using System.ComponentModel;
using mylib.FilterCriteria;
using Xunit;

namespace FilterUtilities.Tests
{
    public class HierarchicalFilterTests
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
            Assert.Equal(oFilter, hFilter.OwnedFilter);
            Assert.Equal(hFilter, oFilter.Owner);
        }

        [Fact]
        public void Filter_Change_DeleteOwnerOfOldFilter()
        {
            // arrange
            var hFilter = new HierarchicalFilter<object>();
            var oFilter = new ObjectFilter<object>();
            hFilter.OwnedFilter = oFilter;

            // act
            hFilter.OwnedFilter = new ObjectFilter<object>();

            // assert
            Assert.Null(oFilter.Owner);
        }

        [Fact]
        public void Children_AddHierarchicalFilter_SetCollectParent()
        {
            // arrange
            var parentFilter = new HierarchicalFilter<object>();
            var childFilter = new HierarchicalFilter<object>();

            // act
            parentFilter.Children.Add(childFilter);

            // assert
            Assert.Equal(parentFilter, childFilter.Parent);
        }

        [Fact]
        public void Children_RemoveHierarchicalFilter_DeleteParent()
        {
            // arrange
            var parentFilter = new HierarchicalFilter<object>();
            var childFilter = new HierarchicalFilter<object>();
            parentFilter.Children.Add(childFilter);

            // act
            parentFilter.Children.Remove(childFilter);

            // assert
            Assert.Null(childFilter.Parent);
        }

        [Fact]
        public void IsRoot_CreateNewHierarchicalFilter_IsRoot()
        {
            // arrange

            // act
            var hFilter = new HierarchicalFilter<object>();

            // assert
            Assert.True(hFilter.IsRoot);
        }

        [Fact]
        public void Children_AddHierarchicalFilter_IsNotRoot()
        {
            // arrange
            var parentFilter = new HierarchicalFilter<object>();
            var childFilter = new HierarchicalFilter<object>();
            parentFilter.Children.Add(childFilter);

            // act
            parentFilter.Children.Add(childFilter);

            // assert
            Assert.False(childFilter.IsRoot);
        }

        [Fact]
        public void Children_RemoveHierarchicalFilter_IsRoot()
        {
            // arrange
            var parentFilter = new HierarchicalFilter<object>();
            var childFilter = new HierarchicalFilter<object>();
            parentFilter.Children.Add(childFilter);

            // act
            parentFilter.Children.Remove(childFilter);

            // assert
            Assert.True(childFilter.IsRoot);
        }

        [Fact]
        public void Children_ChangeCurrentlyOfChildFilter_ChangeCurrentItem()
        {
            // arrange
            var parentFilter = new HierarchicalFilter<object>();
            var childFilter = new HierarchicalFilter<object>();
            parentFilter.Children.Add(childFilter);

            // act
            childFilter.IsCurrent = true;

            // assert
            Assert.Equal(childFilter, parentFilter.CurrentItem);
        }

        [Fact]
        public void Children_ChangeCurrentlyOfGrandChildFilter_ChangeCurrentItem()
        {
            // arrange
            var parentFilter = new HierarchicalFilter<object>();
            var childFilter = new HierarchicalFilter<object>();
            var gchildFilter = new HierarchicalFilter<object>();
            parentFilter.Children.Add(childFilter);
            childFilter.Children.Add(gchildFilter);

            // act
            gchildFilter.IsCurrent = true;

            // assert
            Assert.Equal(gchildFilter, parentFilter.CurrentItem);
        }

        [Fact]
        public void Match_CallMyMatchMethod_CallAncestorMethod()
        {
            // arrange
            string rcvItem = null;
            IHierarchicalFilter<string> rcvSource = null;
            var filterMock = new MockFilter<string>();
            filterMock.MatchMethodEvent += (string item, IHierarchicalFilter<string> inquirySource) =>
            {
                rcvItem = item;
                rcvSource = inquirySource;
                return true;
            };
            var hFilter = new HierarchicalFilter<string>()
            {
                OwnedFilter = filterMock
            };

            string parentRcvItem = null;
            IHierarchicalFilter<string> parentRcvSource = null;
            var parentFilterMock = new MockFilter<string>();
            parentFilterMock.MatchMethodEvent += (string item, IHierarchicalFilter<string> inquirySource) =>
            {
                parentRcvItem = item;
                parentRcvSource = inquirySource;
                return true;
            };
            var parentFilter = new HierarchicalFilter<string>()
            {
                OwnedFilter = parentFilterMock
            };

            string rootRcvItem = null;
            IHierarchicalFilter<string> rootRcvSource = null;
            var rootFilterMock = new MockFilter<string>();
            rootFilterMock.MatchMethodEvent += (string item, IHierarchicalFilter<string> inquirySource) =>
            {
                rootRcvItem = item;
                rootRcvSource = inquirySource;
                return true;
            };
            var rootFilter = new HierarchicalFilter<string>()
            {
                OwnedFilter = rootFilterMock
            };

            rootFilter.Children.Add(parentFilter);
            parentFilter.Children.Add(hFilter);

            // act
            hFilter.Match("test");

            // assert
            Assert.Equal("test", rcvItem);
            Assert.Equal("test", parentRcvItem);
            Assert.Equal("test", rootRcvItem);
            Assert.Equal(hFilter, rcvSource);
            Assert.Equal(hFilter, parentRcvSource);
            Assert.Equal(hFilter, rootRcvSource);
        }

        [Fact]
        public void Match_CallMyMatchMethod_ReturnsAllAncestorPassed()
        {
            // arrange
            var hFilter = new HierarchicalFilter<string>() {
                OwnedFilter = new StringFilter() { FilterObject = "aaa" }
            };
            var parentFilter = new HierarchicalFilter<string>()
            {
                OwnedFilter = new StringFilter() { FilterObject = "aa" }
            };
            var gparentFilter = new HierarchicalFilter<string>()
            {
                OwnedFilter = new StringFilter() { FilterObject = "a" }
            };
            gparentFilter.Children.Add(parentFilter);
            parentFilter.Children.Add(hFilter);

            // act
            var actual1 = hFilter.Match("aaa");
            var actual2 = hFilter.Match("aa");
            var actual3 = parentFilter.Match("aa");
            var actual4 = hFilter.Match("b");

            // assert
            Assert.True(actual1);
            Assert.False(actual2);
            Assert.True(actual3);
            Assert.False(actual4);
        }

        [Fact]
        public void CheckFilterHierarche_Check_ReturnsCorrectResult()
        {
            // arrange
            var gchildFilter = new HierarchicalFilter<object>();
            var childFilter = new HierarchicalFilter<object>();
            var hFilter = new HierarchicalFilter<object>();
            var parentFilter = new HierarchicalFilter<object>();
            var gparentFilter = new HierarchicalFilter<object>();

            gparentFilter.Children.Add(parentFilter);
            parentFilter.Children.Add(hFilter);
            hFilter.Children.Add(childFilter);
            childFilter.Children.Add(gchildFilter);

            // act

            // assert
            Assert.True(hFilter.IsSubFilter(gchildFilter));
            Assert.True(hFilter.IsSubFilter(childFilter));
            Assert.False(hFilter.IsSubFilter(hFilter));
            Assert.False(hFilter.IsSubFilter(parentFilter));
            Assert.False(hFilter.IsSubFilter(gparentFilter));

            Assert.False(hFilter.IsSuperFilter(gchildFilter));
            Assert.False(hFilter.IsSuperFilter(childFilter));
            Assert.False(hFilter.IsSuperFilter(hFilter));
            Assert.True(hFilter.IsSuperFilter(parentFilter));
            Assert.True(hFilter.IsSuperFilter(gparentFilter));
        }

        [Fact]
        public void Children_AddParent_ThrowsInvalidOperationException()
        {
            // arrange
            var hFilter = new HierarchicalFilter<object>();
            var parentFilter = new HierarchicalFilter<object>();
            var gparentFilter = new HierarchicalFilter<object>();

            gparentFilter.Children.Add(parentFilter);
            parentFilter.Children.Add(hFilter);

            // act

            // assert
            Assert.Throws<InvalidOperationException>(() => { hFilter.Children.Add(gparentFilter); });
        }

        [Fact]
        public void Name_Changed_PropertyChangedEventOccurs()
        {
            // arrange
            var hFilter = new HierarchicalFilter<object>();
            string propertyName = String.Empty;
            hFilter.PropertyChanged += (object sender, PropertyChangedEventArgs e) => { propertyName = e.PropertyName; };

            // act
            hFilter.Name = String.Empty;

            // assert
            Assert.Equal("Name", propertyName);
        }

        [Fact]
        public void Filter_Changed_PropertyChangedEventOccurs()
        {
            // arrange
            var hFilter = new HierarchicalFilter<object>();
            string propertyName = String.Empty;
            hFilter.PropertyChanged += (object sender, PropertyChangedEventArgs e) => { propertyName = e.PropertyName; };

            // act
            hFilter.OwnedFilter = new ObjectFilter<object>();

            // assert
            Assert.Equal("Filter", propertyName);
        }

        [Fact]
        public void Parent_Changed_PropertyChangedEventOccurs()
        {
            // arrange
            var hFilter = new HierarchicalFilter<object>();
            var parentFilter = new HierarchicalFilter<object>();
            string propertyName = String.Empty;
            hFilter.PropertyChanged += (object sender, PropertyChangedEventArgs e) => { propertyName = e.PropertyName; };

            // act
            parentFilter.Children.Add(hFilter);

            // assert
            Assert.Equal("Parent", propertyName);
        }

        [Fact]
        public void IsCurrent_Changed_PropertyChangedEventOccurs()
        {
            // arrange
            var hFilter = new HierarchicalFilter<object>();
            string propertyName = null;
            hFilter.PropertyChanged += (object sender, PropertyChangedEventArgs e) => {
                if (propertyName == null) propertyName = e.PropertyName;
            };

            // act
            hFilter.IsCurrent = true;

            // assert
            Assert.Equal("IsCurrent", propertyName);
        }

        [Fact]
        public void CurrentItem_Changed_PropertyChangedEventOccurs()
        {
            // arrange
            var hFilter = new HierarchicalFilter<object>();
            var childFilter = new HierarchicalFilter<object>();
            string propertyName = null;
            hFilter.PropertyChanged += (object sender, PropertyChangedEventArgs e) => { propertyName = e.PropertyName; };

            // act
            hFilter.Children.Add(childFilter);
            childFilter.IsCurrent = true;

            // assert
            Assert.Equal("CurrentItem", propertyName);
        }

        public class MockFilter<T> : IOwnedFilter<T>
        {
            //public IHierarchicalFilter<T> RecievedInquirySource { get; private set; }
            //public T RecievedItem { get; private set; }
            public delegate bool MatchFunction(T item, IHierarchicalFilter<T> inquirySource);
            public event MatchFunction MatchMethodEvent;
            public IHierarchicalFilter<T> Owner { get; protected internal set; }
            IHierarchicalFilter<T> IOwnedFilter<T>.Owner { get { return Owner; } set { Owner = value; } }
            public object FilterObject { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;
            event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
            {
                add
                {
                    this.PropertyChanged += value;
                }

                remove
                {
                    this.PropertyChanged -= value;
                }
            }

            object ICloneable.Clone()
            {
                throw new NotImplementedException();
            }

            public bool Match(T item)
            {
                return Match(item, null);
            }
            bool IFilter<T>.Match(T item) => Match(item);

            public bool Match(T item, IHierarchicalFilter<T> inquirySource)
            {
                return this.MatchMethodEvent(item, inquirySource);
            }
            bool IOwnedFilter<T>.Match(T item, IHierarchicalFilter<T> inquirySource) => Match(item, inquirySource);
        }
    }
}
