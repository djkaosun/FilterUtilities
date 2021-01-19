using System;
using System.ComponentModel;

namespace mylib.FilterCriteria
{
    /// <summary>
    /// 所有者である階層フィルターの兄弟フィルター (親の別の子) のいずれにも適合しない対象を抽出します。
    /// </summary>
    /// <typeparam name="T">このフィルターが受け入れる型</typeparam>
    public class ResudueFilter<T> : IOwnedFilter<T>
    {
        /// <summary>
        /// このフィルターの所有者。
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public IHierarchicalFilter<T> Owner { get; protected internal set; }
        IHierarchicalFilter<T> IOwnedFilter<T>.Owner { get { return Owner; } set { Owner = value; } }

        /// <summary>
        /// このフィルターがフィルター判断の基とするオブジェクト。このクラスでは無視され、動作は変わりません。
        /// </summary>
        public object FilterObject { get; set; }

        /// <summary>
        /// プロパティが変更された場合に発生するイベント。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 所有者である階層フィルターの兄弟フィルターのいずれにも適合しないか判断します。
        /// </summary>
        /// <param name="item">適合するか確認する対象</param>
        /// <returns>適合する場合 true、しない場合 false</returns>
        public bool Match(T item)
        {
            return Match(item, null);
        }

        /// <summary>
        /// 所有者である階層フィルターの兄弟フィルターのいずれにも適合しないか判断します。
        /// </summary>
        /// <param name="item">適合するか確認する対象</param>
        /// <param name="inquirySource">問い合わせ元</param>
        /// <returns>適合する場合 true、しない場合 false</returns>
        protected internal bool Match(T item, IHierarchicalFilter<T> inquirySource)
        {
            if (Owner == null) throw new InvalidOperationException("所有者が設定されていません。");

            if (Owner.Parent == null) throw new InvalidOperationException("親のいない HierarchicalFilter に所有されています。");

            // 親にマッチしなければ false
            if (!Owner.Parent.Match(item, inquirySource)) return false;

            // 兄弟 (親の、自分以外の子) にマッチすれば false
            foreach (IHierarchicalFilter<T> bros in Owner.Parent.Children)
            {
                if (!(bros.OwnedFilter is ResudueFilter<T>) && bros.OwnedFilter.Match(item, inquirySource)) return false;
            }

            // ここまでたどり着いたら ture
            return true;
        }
        bool IOwnedFilter<T>.Match(T item, IHierarchicalFilter<T> inquirySource) => Match(item, inquirySource);

        /// <summary>
        /// このオブジェクトのコピーを返します。
        /// </summary>
        /// <returns>このオブジェクトのコピー。</returns>
        object ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
