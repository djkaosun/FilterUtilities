using System;
using System.Collections.ObjectModel;

namespace mylib.FilterCriteria
{
    /// <summary>
    /// ある階層フィルターの子フィルターを入れます。
    /// </summary>
    /// <typeparam name="T">階層フィルターが受け入れる型</typeparam>
    public class HierarchicalFilterChildren<T> : ObservableCollection<IHierarchicalFilter<T>>
    {
    }
}
