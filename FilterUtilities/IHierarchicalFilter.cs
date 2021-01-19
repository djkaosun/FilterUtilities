using System;
using System.ComponentModel;

namespace mylib.FilterCriteria
{
    /// <summary>
    /// 階層フィルターのインターフェイス。
    /// </summary>
    /// <typeparam name="T">階層フィルターが受け入れる型</typeparam>
    public interface IHierarchicalFilter<T> : IFilter<T>, INotifyPropertyChanged
    {
        /// <summary>
        /// この階層フィルターがもつフィルターの名前。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 階層フィルターが所有するフィルター。
        /// 内部的に、Match メソッドで利用されます。
        /// 階層フィルターとしての結果ではなく、このノードのみでの適合性が必要な場合は、
        /// この Filter プロパティに設定されている IFilter の Match メソッドを使用します。
        /// </summary>
        public IOwnedFilter<T> OwnedFilter { get; set; }

        /// <summary>
        /// 親フィルター。
        /// </summary>
        public IHierarchicalFilter<T> Parent { get; }

        /// <summary>
        /// 子フィルターを入れるコレクション。
        /// </summary>
        public HierarchicalFilterChildren<T> Children { get; }

        /// <summary>
        /// ルート フィルターかどうか。
        /// </summary>
        public bool IsRoot { get; }

        /// <summary>
        /// この階層フィルターが選択状態かどうか。
        /// </summary>
        public bool IsCurrent { get; set; }

        /// <summary>
        /// この階層フィルターの子孫で、最後に選択状態になったもの。
        /// </summary>
        public IHierarchicalFilter<T> CurrentItem { get; }

        /*
        /// <summary>
        /// このノードでの階層フィルターとしての既定の適合性を判断します。
        /// </summary>
        /// <param name="item">適合するか確認する対象</param>
        /// <returns>適合する場合 true、しない場合 false</returns>
        public new virtual bool Match(T item)
        {
            return this.Match(item, null);
        }
        */

        /// <summary>
        /// このノードでの階層フィルターとしての適合性を判断します。
        /// </summary>
        /// <param name="item">適合するか確認する対象</param>
        /// <param name="inquirySource">問い合わせ元</param>
        /// <returns>適合する場合 true、しない場合 false</returns>
        protected internal bool Match(T item, IHierarchicalFilter<T> inquirySource);

        /// <summary>
        /// ある階層フィルターが、この階層フィルターの子孫かどうかチェックします。
        /// </summary>
        /// <param name="item">子孫かどうかチェックする階層フィルター。</param>
        /// <returns>子孫の場合 true、子孫ではない場合 false。</returns>
        public bool IsSubFilter(IHierarchicalFilter<T> item);

        /// <summary>
        /// ある階層フィルターが、この階層フィルターの祖先かどうかチェックします。
        /// </summary>
        /// <param name="item">祖先かどうかチェックする階層フィルター。</param>
        /// <returns>祖先の場合 true、祖先ではない場合 false。</returns>
        public bool IsSuperFilter(IHierarchicalFilter<T> item);
    }
}
