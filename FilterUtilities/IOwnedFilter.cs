using System;
using System.ComponentModel;

namespace mylib.FilterCriteria
{
    /// <summary>
    /// 階層フィルターに所持されるフィルターです。
    /// </summary>
    /// <typeparam name="T">フィルターが受け入れる型</typeparam>
    public interface IOwnedFilter<T> : IFilter<T>,  ICloneable, INotifyPropertyChanged
    {
        /// <summary>
        /// このフィルターの所有者。
        /// </summary>
        public IHierarchicalFilter<T> Owner { get; protected internal set; }

        /*
        /// <summary>
        /// このノードでの既定の適合性を判断します。
        /// </summary>
        /// <param name="item">適合するか確認する対象</param>
        /// <returns>適合する場合 true、しない場合 false</returns>
        public virtual bool Match(T item)
        {
            return Match(item, null);
        }
        bool IFilter<T>.Match(T item) => Match(item);
        */

        /// <summary>
        /// このノードでの適合性を判断します。inquirySource が null のとき、既定の動作をします。
        /// </summary>
        /// <param name="item">適合するか確認する対象</param>
        /// <param name="inquirySource">問い合わせ元</param>
        /// <returns>適合する場合 true、しない場合 false</returns>
        protected internal abstract bool Match(T item, IHierarchicalFilter<T> inquirySource);
    }
}
