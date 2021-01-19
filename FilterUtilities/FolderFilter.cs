using System;
using System.ComponentModel;

namespace mylib.FilterCriteria
{
    /// <summary>
    /// 子フィルターのいずれかに合致する場合に合致するフィルターです。(子の和集合的)
    /// </summary>
    /// <typeparam name="T">このフィルターが受け入れる型</typeparam>
    public class FolderFilter<T> : IOwnedFilter<T>
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
        /// 既定 (問い合わせ元が所有者の子孫でない場合) の判断を返します。
        /// </summary>
        /// <param name="item">適合するか確認する対象</param>
        /// <returns>適合する場合 true、しない場合 false</returns>
        public bool Match(T item)
        {
            return Match(item, null);
        }

        /// <summary>
        /// 問い合わせ元が所有者の子孫なら親の回答を、それ以外なら子たちの回答の OR をとり、それを親の回答と AND したものを返します。
        /// </summary>
        /// <param name="item">適合するか確認する対象</param>
        /// <param name="inquirySource">問い合わせ元</param>
        /// <returns>適合する場合 true、しない場合 false</returns>
        protected internal bool Match(T item, IHierarchicalFilter<T> inquirySource)
        {
            if (this.Owner == null) throw new InvalidOperationException("所有者が設定されていません。");

            // これがルートなら、子の OR は全集合。ルートであれば、子からの問い合わせも true であるべき。
            if (Owner.IsRoot) return true;

            // そもそも論、祖先で false になるものは false。
            // (既定の処理の一部だが、効率のため先に実施)
            // (上の行で、この所有者がルートの可能性は排除されている)
            if (!Owner.Parent.Match(item, inquirySource)) return false;

            // 既定の回答を求められておらず、かつ、子からの問い合わせの場合は、親の回答そのまま、つまり true。
            // 上の行で、親の回答が false の可能性は排除されている。
            if (inquirySource != null && Owner.IsSubFilter(inquirySource)) return true;

            // 以下、既定の戻り値の処理。(親の処理はここまでで済んでいるので子の処理のみ)
            // 子たちの OR をとる。(子は孫以下より広いので、子孫すべてまで行く必要はない)
            foreach (IHierarchicalFilter<T> child in Owner.Children)
            {
                if (!(child.OwnedFilter is ResudueFilter<T>) && child.OwnedFilter.Match(item, inquirySource))
                {
                    return true;
                }
            }
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
