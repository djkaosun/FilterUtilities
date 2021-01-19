using System;
using System.ComponentModel;

namespace mylib.FilterCriteria
{
    /// <summary>
    /// 設定したオブジェクトと一致する (Equals メソッドが True) か判断するフィルター。
    /// </summary>
    /// <typeparam name="T">フィルターが受け入れる型</typeparam>
    public class ObjectFilter<T> : IOwnedFilter<T>
    {
        /// <summary>
        /// このフィルターの所有者。
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public IHierarchicalFilter<T> Owner { get; protected internal set; }
        IHierarchicalFilter<T> IOwnedFilter<T>.Owner { get { return Owner; } set { Owner = value; } }

        /// <summary>
        /// プロパティが変更された場合に発生するイベント。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private T _Object;
        /// <summary>
        /// 判断の基となるオブジェクト。
        /// </summary>
        public object FilterObject
        {
            get
            {
                return _Object;
            }
            set
            {
                if (value is T castedvalue)
                {
                    _Object = castedvalue;
                    if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("Object"));
                }
                else
                {
                    throw new InvalidOperationException("invalid operation", new InvalidCastException(value.GetType().FullName + " to " + typeof(T).FullName));
                }
            }
        }

        /// <summary>
        /// オブジェクトがこのフィルターに合致するかを判断します。
        /// </summary>
        /// <param name="item">文字列。</param>
        /// <returns>Object に設定されたオブジェクトと item が一致する場合 true。それ以外の場合は false。</returns>
        public bool Match(T item)
        {
            return Match(item, null);
        }

        /// <summary>
        /// オブジェクトがこのフィルターに合致するかを判断します。要求元に関わらず同じ結果を返すため、inquirySource は無視されます。
        /// </summary>
        /// <param name="item">文字列。</param>
        /// <param name="inquirySource">この要求の要求元。(無視されます)</param>
        /// <returns>Object に設定されたオブジェクトと item が一致する場合 true。それ以外の場合は false。</returns>
        protected internal bool Match(T item, IHierarchicalFilter<T> inquirySource)
        {
            if (this.FilterObject != null)
            {
                return this.FilterObject.Equals(item);
            }
            else if (item != null)
            {
                return item.Equals(this.FilterObject);
            }
            else
            {
                return true; // 両方 null
            }
        }
        bool IOwnedFilter<T>.Match(T item, IHierarchicalFilter<T> inquirySource) => Match(item, inquirySource);

        /// <summary>
        /// このオブジェクトのコピーを返します。(shallow copy)
        /// </summary>
        /// <returns>このオブジェクトのコピー。(shallow copy)</returns>
        object ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
