using System;
using System.ComponentModel;

namespace mylib.FilterCriteria
{
    /// <summary>
    /// 常に適合しないフィルター。
    /// </summary>
    /// <typeparam name="T">フィルターが受け入れる型</typeparam>
    public class AnyFailFilter<T> : IOwnedFilter<T>
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
        /// 常に false を返します。
        /// </summary>
        /// <param name="item">適合するか確認する対象</param>
        /// <returns>適合する場合 true、しない場合 false。(常に false)</returns>
        public bool Match(T item)
        {
            return Match(item, null);
        }

        /// <summary>
        /// 常に false を返します。
        /// </summary>
        /// <param name="item">無視されます。</param>
        /// <param name="inquirySource">無視されます。</param>
        /// <returns>適合する場合 true、しない場合 false。(常に false)</returns>
        protected internal bool Match(T item, IHierarchicalFilter<T> inquirySource)
        {
            return false;
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
