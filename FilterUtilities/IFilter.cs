using System;

namespace mylib.FilterCriteria
{
    /// <summary>
    /// フィルターのインターフェイスです。
    /// </summary>
    /// <typeparam name="T">フィルターが受け入れる型</typeparam>
    public interface IFilter<T>
    {
        /// <summary>
        /// このフィルターがフィルター判断の基とするオブジェクト。
        /// </summary>
        public object FilterObject { get; set; }

        /// <summary>
        /// このフィルターへの適合性を判断します。
        /// </summary>
        /// <param name="item">適合するか確認する対象</param>
        /// <returns>適合する場合 true、しない場合 false</returns>
        public abstract bool Match(T item);
    }
}
