using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace mylib.FilterCriteria
{
    /// <summary>
    /// 文字列用のフィルター。FilterString に設定できるフィルターは Google 的な書式です。
    /// </summary>
    public class StringFilter : IOwnedFilter<string>
    {
        /// <summary>
        /// このフィルターの所有者。
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public IHierarchicalFilter<string> Owner { get; protected internal set; }
        IHierarchicalFilter<string> IOwnedFilter<string>.Owner { get { return Owner; } set { Owner = value; } }

        private IExpressionElement FilterTree;
        private string _FilterString;
        /// <summary>
        /// フィルターを示す文字列。
        /// </summary>
        public object FilterObject {
            get { return this._FilterString; }
            set
            {
                this._FilterString = value.ToString();

                if (String.IsNullOrEmpty(_FilterString))
                {
                    this.FilterTree = new OperandElement();
                }
                else
                {
                    List<IExpressionElement> elements = tokenize(_FilterString);
//System.Console.WriteLine("================================================================");
//foreach (IExpressionElement elem in elements) System.Console.WriteLine(elem);
//System.Console.WriteLine("================================================================");
                    this.FilterTree = parse(elements);
                }

                if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("FilterString"));
            }
        }

        /// <summary>
        /// プロパティが変更された場合に発生するイベント。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// コンストラクター。
        /// </summary>
        public StringFilter()
        {
            this.FilterTree = new OperandElement();
            this.FilterObject = String.Empty;
        }

        /// <summary>
        /// 文字列を分解, 分析し、評価式エレメント ツリーを生成します。
        /// </summary>
        /// <param name="filterString">フィルターを表す文字列。Google 検索的な書式です。</param>
        /// <returns>評価式エレメント ツリーのルート エレメント。このオブジェクトの Match メソッドを利用することで、ある文字列の、この評価式への適合性を判断できます。</returns>
        public static IExpressionElement Parse(string filterString)
        {
            return parse(tokenize(filterString));
        }

        #region privateMethod
        private static List<IExpressionElement> tokenize(string filterString)
        {
            List<IExpressionElement> result = new List<IExpressionElement>();

            bool isSpaceAtPrev = true;
            bool isSpaceOAtPrev = false;
            bool isSpaceORAtPrev = false;
            bool isOrTokenAtPrev = false;
            bool isNotTokenAtPrev = false;
            bool isOperandAtPrev = false;
            bool isMinusAtPrev = false;
            bool isQuoted = false;
            string token = String.Empty;
            int nest = 0;
            foreach (char c in filterString.ToCharArray())
            {
                if (isQuoted)
                {
                    if (c == '"')
                    {
                        if (isOrTokenAtPrev)
                        {
                            result.Add(new OrOperatorElement());
                        }
                        else if (isOperandAtPrev)
                        {
                            result.Add(new AndOperatorElement());
                        }

                        if (isNotTokenAtPrev)
                        {
                            result.Add(new NotOperatorElement());
                            isNotTokenAtPrev = false;
                        }

                        result.Add(new OperandElement(token, true));
                        token = String.Empty;

                        isSpaceAtPrev = true;
                        isQuoted = false;
                        isOperandAtPrev = true;
                    }
                    else
                    {
                        token += c;
                    }
                }
                else if (isMinusAtPrev)
                {
                    // "～" でなく、かつ、「 -」の後の場合
                    switch (c)
                    {
                        case ' ':
                            if (isOrTokenAtPrev)
                            {
                                result.Add(new OrOperatorElement());
                                isOrTokenAtPrev = false;
                            }
                            else if (isOperandAtPrev)
                            {
                                result.Add(new AndOperatorElement());
                            }

                            result.Add(new OperandElement(token));
                            token = String.Empty;
                            isSpaceAtPrev = true;
                            isOperandAtPrev = true;
                            break;
                        case '"':
                            if (isOrTokenAtPrev)
                            {
                                result.Add(new OrOperatorElement());
                                isOrTokenAtPrev = false;
                            }
                            else if (isOperandAtPrev)
                            {
                                result.Add(new AndOperatorElement());
                            }

                            result.Add(new NotOperatorElement());
                            token = String.Empty;
                            isQuoted = true;
                            isOperandAtPrev = false; // この時点で演算子を吐き出しているため
                            break;
                        case '(':
                            if (isOrTokenAtPrev)
                            {
                                result.Add(new OrOperatorElement());
                                isOrTokenAtPrev = false;
                            }
                            else if (isOperandAtPrev)
                            {
                                result.Add(new AndOperatorElement());
                            }

                            result.Add(new NotOperatorElement());
                            result.Add(new NestBeginOperatorElement());
                            nest++;
                            token = String.Empty;

                            isSpaceAtPrev = true;
                            isOperandAtPrev = false;
                            break;
                        case ')':
                            if (isOrTokenAtPrev)
                            {
                                result.Add(new OrOperatorElement());
                                isOrTokenAtPrev = false;
                            }
                            else if (isOperandAtPrev)
                            {
                                result.Add(new AndOperatorElement());
                            }

                            if (isNotTokenAtPrev)
                            {
                                result.Add(new NotOperatorElement());
                                isNotTokenAtPrev = false;
                            }

                            result.Add(new OperandElement(token));
                            token = String.Empty;

                            result.Add(new NestEndOperatorElement());
                            nest--;
                            
                            isSpaceAtPrev = true;
                            isOperandAtPrev = true;
                            break;
                        default:
                            token = String.Empty + c;
                            isNotTokenAtPrev = true;
                            break;
                    }
                    isMinusAtPrev = false;
                }
                else
                {
                    if (isSpaceAtPrev)
                    {
                        // "～" でなく、かつ、空白の直後の場合
                        switch (c)
                        {
                            case ' ':
                                // 「 」は読み飛ばす
                                break;
                            case '"':
                                isSpaceAtPrev = false; // いらないかも
                                isQuoted = true;
                                break;
                            case '-':
                                token += c;
                                isSpaceAtPrev = false;
                                isMinusAtPrev = true;
                                break;
                            case '(':
                                if (isOrTokenAtPrev)
                                {
                                    result.Add(new OrOperatorElement());
                                    isOrTokenAtPrev = false;
                                }
                                else if (isOperandAtPrev)
                                {
                                    result.Add(new AndOperatorElement());
                                }
                                result.Add(new NestBeginOperatorElement());
                                nest++;
                                isOperandAtPrev = false;
                                break;
                            case ')':
                                if (isOrTokenAtPrev)
                                {
                                    if (isOperandAtPrev) result.Add(new AndOperatorElement());
                                    result.Add(new OperandElement("OR"));
                                    isOrTokenAtPrev = false;
                                }
                                result.Add(new NestEndOperatorElement());
                                nest--;
                                isOperandAtPrev = true;
                                break;
                            case 'O':
                                if (isOperandAtPrev) isSpaceOAtPrev = true;
                                token += c;
                                isSpaceAtPrev = false;
                                break;
                            default:
                                token += c;
                                isSpaceAtPrev = false;
                                break;
                        }
                    }
                    else if (isSpaceOAtPrev)
                    {
                        // "～" でなく、かつ、OR 演算子の可能性のある「 O」の後の場合
                        switch (c)
                        {
                            case ' ':
                                if (isOperandAtPrev) result.Add(new AndOperatorElement());
                                result.Add(new OperandElement(token));
                                token = String.Empty;

                                isSpaceAtPrev = true;
                                isOperandAtPrev = true;
                                break;
                            case '"':
                                if (isOperandAtPrev) result.Add(new AndOperatorElement());
                                result.Add(new OperandElement(token));
                                token = String.Empty;

                                isQuoted = true;
                                isOperandAtPrev = true;
                                break;
                            case '(':
                                if (isOperandAtPrev) result.Add(new AndOperatorElement());
                                result.Add(new OperandElement(token));
                                token = String.Empty;

                                result.Add(new NestBeginOperatorElement());
                                nest++;

                                isSpaceAtPrev = true;
                                isOperandAtPrev = false;
                                break;
                            case ')':
                                if (isOperandAtPrev) result.Add(new AndOperatorElement());
                                result.Add(new OperandElement(token));
                                token = String.Empty;

                                result.Add(new NestEndOperatorElement());
                                nest--;

                                isSpaceAtPrev = true;
                                isOperandAtPrev = true;
                                break;
                            case 'R':
                                token += c;
                                isSpaceORAtPrev = true;
                                break;
                            default:
                                token += c;
                                break;
                        }
                        isSpaceOAtPrev = false;
                    }
                    else if (isSpaceORAtPrev)
                    {
                        // "～" でなく、かつ、OR 演算子の可能性のある「 OR」の後の場合
                        switch (c)
                        {
                            case ' ':
                                token = String.Empty;
                                isSpaceAtPrev = true;
                                isOrTokenAtPrev = true;
                                isOperandAtPrev = false;
                                break;
                            case '"':
                                token = String.Empty;
                                isQuoted = true;
                                isOrTokenAtPrev = true;
                                isOperandAtPrev = false;
                                break;
                            case '(':
                                result.Add(new OrOperatorElement());
                                token = String.Empty;

                                result.Add(new NestBeginOperatorElement());
                                nest++;

                                isSpaceAtPrev = true;
                                isOperandAtPrev = false;
                                break;
                            case ')':
                                if (isOperandAtPrev) result.Add(new AndOperatorElement());
                                result.Add(new OperandElement(token));
                                token = String.Empty;

                                result.Add(new NestEndOperatorElement());
                                nest--;

                                isSpaceAtPrev = true;
                                isOperandAtPrev = true;
                                break;
                            default:
                                token += c;
                                break;
                        }
                        isSpaceORAtPrev = false;
                    }
                    else
                    {
                        // OR にも NOT にも関係ない、かつ、空白の直後でもない場合
                        switch (c)
                        {
                            case ' ':
                                if (isOrTokenAtPrev)
                                {
                                    result.Add(new OrOperatorElement());
                                    isOrTokenAtPrev = false;
                                }
                                else if (isOperandAtPrev)
                                {
                                    result.Add(new AndOperatorElement());
                                }

                                if (isNotTokenAtPrev)
                                {
                                    result.Add(new NotOperatorElement());
                                    isNotTokenAtPrev = false;
                                }

                                result.Add(new OperandElement(token));
                                token = String.Empty;

                                isSpaceAtPrev = true;
                                isOperandAtPrev = true;
                                break;
                            case '"':
                                if (isOrTokenAtPrev)
                                {
                                    result.Add(new OrOperatorElement());
                                    isOrTokenAtPrev = false;
                                }
                                else if (isOperandAtPrev)
                                {
                                    result.Add(new AndOperatorElement());
                                }

                                if (isNotTokenAtPrev)
                                {
                                    result.Add(new NotOperatorElement());
                                    isNotTokenAtPrev = false;
                                }

                                result.Add(new OperandElement(token));
                                token = String.Empty;

                                isQuoted = true;
                                isOperandAtPrev = true; // いらないかも
                                break;
                            case '(':
                                if (isOrTokenAtPrev)
                                {
                                    result.Add(new OrOperatorElement());
                                    isOrTokenAtPrev = false;
                                }
                                else if (isOperandAtPrev)
                                {
                                    result.Add(new AndOperatorElement());
                                }

                                if (isNotTokenAtPrev)
                                {
                                    result.Add(new NotOperatorElement());
                                    isNotTokenAtPrev = false;
                                }

                                result.Add(new OperandElement(token));
                                token = String.Empty;

                                result.Add(new AndOperatorElement());
                                result.Add(new NestBeginOperatorElement());
                                nest++;

                                isSpaceAtPrev = true;
                                isOperandAtPrev = false;
                                break;
                            case ')':
                                if (isOrTokenAtPrev)
                                {
                                    result.Add(new OrOperatorElement());
                                    isOrTokenAtPrev = false;
                                }
                                else if (isOperandAtPrev)
                                {
                                    result.Add(new AndOperatorElement());
                                }

                                if (isNotTokenAtPrev)
                                {
                                    result.Add(new NotOperatorElement());
                                    isNotTokenAtPrev = false;
                                }

                                result.Add(new OperandElement(token));
                                token = String.Empty;

                                result.Add(new NestEndOperatorElement());
                                nest--;

                                isSpaceAtPrev = true;
                                isOperandAtPrev = true;
                                break;
                            default:
                                token += c;
                                break;
                        }
                    }
                }
            }

            // 最後のトークンの吐き出し
            if (token != String.Empty)
            {
                if (isOrTokenAtPrev)
                {
                    result.Add(new OrOperatorElement());
                }
                else if (isOperandAtPrev)
                {
                    result.Add(new AndOperatorElement());
                }

                if (isNotTokenAtPrev)
                {
                    result.Add(new NotOperatorElement());
                    isNotTokenAtPrev = false;
                }

                if (isQuoted)
                {
                    // "～" が閉じられなかった場合の辻褄合わせ
                    result.Add(new OperandElement(token, true));
                }
                else
                {
                    result.Add(new OperandElement(token));
                }
            }

            // 括弧の辻褄合わせ
            for (int i = 0; i < nest; i++)
            {
                result.Add(new NestEndOperatorElement());
            }

            for (int i = 0; i > nest; i--)
            {
                result.Insert(0, new NestBeginOperatorElement());
            }

            // 「()」については間に空文字列があるとする。恒真になる。
            bool isNestBeginAtPrev = false;
            for (int i = 0; i < result.Count; i++)
            {
                if (result[i] is NestBeginOperatorElement)
                {
                    isNestBeginAtPrev = true;
                }
                else
                {
                    if (isNestBeginAtPrev && result[i] is NestEndOperatorElement)
                    {
                        result.Insert(i, new OperandElement());
                    }
                    isNestBeginAtPrev = false;
                }
            }

            return result;
        }

        private static IExpressionElement parse(List<IExpressionElement> elements)
        {
            // 要素がないなら空の項を返す
            if (elements.Count == 0) return new OperandElement();

            // 配列化
            IExpressionElement[] elemArray = elements.ToArray();

            // 左項の括弧の除去
            int startIndex = 0;
            int endIndex = elemArray.Length;
            while (isNested(elemArray, startIndex, endIndex))
            {
                startIndex++;
                endIndex--;
            }

            // 括弧をとると要素がなくなるなら空の項を返す
            if (startIndex == endIndex) return new OperandElement();

            // 再帰的に処理
            return parseRecurse(elemArray, startIndex, endIndex);
        }

        private static IExpressionElement parseRecurse(IExpressionElement[] elemArray, int startIndex, int endIndex)
        {
            // 演算子の位置の取得
            int pos = getOperatorPos(elemArray, startIndex, endIndex);

            if (pos < 0)
            {
                if ((endIndex - startIndex) != 1) throw new ArgumentException(
                        "構文解析に失敗しました。(演算子の位置取得失敗)"
                        + "pos: " + pos
                        + "/ startIndex: " + startIndex
                        + " / endIndex: " + endIndex
                        );
                return elemArray[startIndex];
            }

            int leftEnd = pos;
            int rightStart = pos + 1;
/*
Console.Write(elemArray[pos].GetType().Name);
Console.Write(": ");
Console.Write(pos);
Console.Write(" / left: ");
Console.Write(startIndex);
Console.Write(" ～ ");
Console.Write(leftEnd);
Console.Write(" / right: ");
Console.Write(rightStart);
Console.Write(" ～ ");
Console.Write(endIndex);
Console.WriteLine();
*/
            // 左項の処理
            if (((OperatorElement)elemArray[pos]).Operator == OperatorElement.OperatorType.Not)
            {
                if (startIndex != leftEnd) throw new ArgumentException(
                        "構文解析に失敗しました。(NOT に左項がある)"
                        + "pos: " + pos
                        + "/ startIndex: " + startIndex
                        + " / endIndex: " + endIndex
                        );
            }
            else
            {
                // 左項の括弧の除去
                while (isNested(elemArray, startIndex, leftEnd))
                {
                    startIndex++;
                    leftEnd--;
                }
                // 左項に結び付け
                if (startIndex == leftEnd) throw new ArgumentException(
                        "構文解析に失敗しました。(左項がない)"
                        + "pos: " + pos
                        + "/ startIndex: " + startIndex
                        + " / endIndex: " + endIndex
                        );
                ((OperatorElement)elemArray[pos]).Left = parseRecurse(elemArray, startIndex, leftEnd);
            }

            // 右項の処理
            // 右項の括弧の除去
            while (isNested(elemArray, rightStart, endIndex))
            {
                rightStart++;
                endIndex--;
            }
            // 右項に結び付け
            if (rightStart == endIndex) throw new ArgumentException(
                    "構文解析に失敗しました。(右項がない)"
                        + "pos: " + pos
                        + "/ startIndex: " + startIndex
                        + " / endIndex: " + endIndex
                    );
            ((OperatorElement)elemArray[pos]).Right = parseRecurse(elemArray, rightStart, endIndex);

            return elemArray[pos];
        }

        private static bool isNested(IExpressionElement[] elemArray, int startIndex, int endIndex)
        {
            // 最初と最後のいずれかが項なら false
            if (elemArray[startIndex].Type == ExpressionElementType.Operand || elemArray[endIndex - 1].Type == ExpressionElementType.Operand)
            {
                return false;
            }

            // 最初が「(」でないか、または、最後が「)」でないなら false
            if (((OperatorElement)elemArray[startIndex]).Operator != OperatorElement.OperatorType.NestBegin || ((OperatorElement)elemArray[endIndex - 1]).Operator != OperatorElement.OperatorType.NestEnd)
            {
                return false;
            }

            // 最初と最後をとってみて、ネストに不具合があったら false
            int nest = 0;
            for (int i = startIndex + 1; i < endIndex - 1; i++)
            {
                if (elemArray[i].Type == ExpressionElementType.Operator)
                {
                    if (((OperatorElement)elemArray[i]).Operator == OperatorElement.OperatorType.NestBegin)
                    {
                        nest++;
                    }
                    else if (((OperatorElement)elemArray[i]).Operator == OperatorElement.OperatorType.NestEnd)
                    {
                        nest--;
                    }

                    if (nest < 0) return false;
                }
            }

            // すべてのチェックを抜けたら true
            return true;
        }

        private static int getOperatorPos(IExpressionElement[] elemArray, int startIndex, int endIndex)
        {
//System.Windows.MessageBox.Show("引数 // Start: " + startIndex + " / End: " + endIndex);
            IExpressionElement elem;
            int pos = -1;
            int nest = 0;
            int priority = Int32.MaxValue;
            int lowestPriority = Int32.MinValue;
            for (int i = startIndex; i < endIndex; i++)
            {
                elem = elemArray[i];
                
                if (elem.Type == ExpressionElementType.Operator)
                {
                    if (((OperatorElement)elem).Operator == OperatorElement.OperatorType.NestBegin)
                    {
                        nest++;
                    }
                    else if (((OperatorElement)elem).Operator == OperatorElement.OperatorType.NestEnd)
                    {
                        nest--;
                    }
                    else
                    {
                        priority = ((OperatorElement)elem).Priority;

//System.Windows.MessageBox.Show("" + elem + " // " + priority + "/" + lowestPriority);
                        if (nest == 0 && priority >= lowestPriority)
                        {
                            lowestPriority = priority;
                            pos = i;
                        }
                    }
                }
            }

//System.Windows.MessageBox.Show("戻り値　// " + pos + " / " + elemArray[(pos == -1)?startIndex:pos]);
            return pos;
        }
        #endregion privateMethod

        /// <summary>
        /// このオブジェクトをディープ コピーします。
        /// </summary>
        /// <returns>コピーされたオブジェクト</returns>
        public Object Clone()
        {
            StringFilter result = (StringFilter)this.MemberwiseClone();
            if(this.FilterTree != null) result.FilterTree = (IExpressionElement)this.FilterTree.Clone();
            return result;
        }

        /// <summary>
        /// 文字列がこのフィルターに合致するかを判断します。
        /// </summary>
        /// <param name="item">文字列。</param>
        public bool Match(string item)
        {
            return Match(item, null);
        }

        /// <summary>
        /// 文字列がこのフィルターに合致するかを判断します。要求元に関わらず同じ結果を返すため、inquirySource は無視されます。
        /// </summary>
        /// <param name="item">文字列。</param>
        /// <param name="inquirySource">この要求の要求元。(無視されます)</param>
        /// <returns></returns>
        protected internal bool Match(string item, IHierarchicalFilter<string> inquirySource)
        {
            return FilterTree.Match(item);
        }
        bool IOwnedFilter<string>.Match(string item, IHierarchicalFilter<string> inquirySource) => Match(item, null);

        #region InternalClass
        /// <summary>
        /// 評価式のエレメントです。
        /// </summary>
        public interface IExpressionElement : ICloneable, IFilter<string>
        {
            /// <summary>
            /// この評価式エレメントのタイプ。演算子 (Operator) もしくは被演算子 (Operand) です。
            /// </summary>
            ExpressionElementType Type { get; }

            /// <summary>
            /// オブジェクトを文字列にします。
            /// </summary>
            /// <returns>このオブジェクトを示す文字列。</returns>
            string ToString();

            /// <summary>
            /// この評価式エレメントの優先度。数値が小さいほど優先度が高いと判断されます。
            /// </summary>
            int Priority { get; }
        }

        /// <summary>
        /// 評価式エレメントのタイプです。
        /// </summary>
        public enum ExpressionElementType
        {
            /// <summary>
            /// 演算子。
            /// </summary>
            Operator,

            /// <summary>
            /// 被演算子。
            /// </summary>
            Operand
        }

        /// <summary>
        /// 演算子エレメントです。
        /// </summary>
        [Serializable]
        public abstract class OperatorElement : IFilter<string>, IExpressionElement
        {
            /// <summary>
            /// このフィルターがフィルター判断の基とするオブジェクト。このクラスでは無視され、動作は変わりません。
            /// </summary>
            public object FilterObject { get; set; }

            /// <summary>
            /// この評価式エレメントのタイプ。演算子 (Operator) です。
            /// </summary>
            public ExpressionElementType Type { get; set; }

            /// <summary>
            /// この演算子エレメントのタイプ。And, Or, Not, NestBegin, NestEnd のいずれかです。
            /// </summary>
            public OperatorElement.OperatorType Operator { get; set; }

            /// <summary>
            /// この評価式エレメントの優先度。数値が小さいほど優先度が高いと判断されます。
            /// </summary>
            public int Priority { get; set; }

            /// <summary>
            /// この演算子エレメントの右側の評価式エレメント。
            /// </summary>
            public IExpressionElement Right { get; set; }

            /// <summary>
            /// この演算子エレメントの左側の評価式エレメント。
            /// </summary>
            public IExpressionElement Left { get; set; }

            /// <summary>
            /// 演算子エレメントのタイプ。
            /// </summary>
            public enum OperatorType {
                /// <summary>
                /// 論理和。
                /// </summary>
                And,
                
                /// <summary>
                /// 論理積。
                /// </summary>
                Or,

                /// <summary>
                /// 否定。
                /// </summary>
                Not,

                /// <summary>
                /// 括弧始まり。
                /// </summary>
                NestBegin,

                /// <summary>
                /// 括弧終わり。
                /// </summary>
                NestEnd
            }

            /// <summary>
            /// コンストラクター。
            /// </summary>
            public OperatorElement()
            {
                this.Type = ExpressionElementType.Operator;
                this.Priority = Int32.MaxValue;
            }

            /// <summary>
            /// このオブジェクトを示す文字列を取得します。
            /// </summary>
            /// <returns>このオブジェクトを示す文字列。</returns>
            public new string ToString()
            {
                return base.GetType().Name + " | L[ " + this.Left + " ] | R[ " + this.Right + " ]";
            }

            /// <summary>
            /// 評価式に合致するかを判断します。このメソッドを実行することにより、この評価式エレメント配下の Match メソッドも評価されます。
            /// </summary>
            /// <param name="item">判断する対象の文字列</param>
            /// <returns>合致する場合 true。それ以外の場合は false。</returns>
            public abstract bool Match(string item);

            /// <summary>
            /// このオブジェクトのコピーを生成します。右側エレメントおよび左側エレメントの Clone() メソッドも内部的に呼び出します。(ディープコピー)
            /// </summary>
            /// <returns>このオブジェクトを示す文字列。</returns>
            public Object Clone()
            {
                OperatorElement result = (OperatorElement)this.MemberwiseClone();
                if (this.Left != null) result.Left = (IExpressionElement)this.Left.Clone();
                if (this.Right != null) result.Right = (IExpressionElement)this.Right.Clone();
                return result;
            }
        }

        /// <summary>
        /// 論理和エレメント。
        /// </summary>
        [Serializable]
        public class AndOperatorElement : OperatorElement
        {
            /// <summary>
            /// コンストラクター。
            /// </summary>
            public AndOperatorElement() : base()
            {
                base.Operator = OperatorElement.OperatorType.And;
                base.Priority = 2;
            }

            /// <summary>
            /// 評価式に合致するかを判断します。このメソッドを実行することにより、この評価式エレメント配下の Match メソッドも評価されます。
            /// </summary>
            /// <param name="item">判断する対象の文字列</param>
            /// <returns>合致する場合 true。それ以外の場合は false。</returns>
            public override bool Match(string item)
            {
                if (base.Right == null || base.Left == null) return true;
                return base.Right.Match(item) && base.Left.Match(item);
            }
        }

        /// <summary>
        /// 論理和エレメント。
        /// </summary>
        [Serializable]
        public class OrOperatorElement : OperatorElement
        {
            /// <summary>
            /// コンストラクター。
            /// </summary>
            public OrOperatorElement()
                : base()
            {
                base.Operator = OperatorElement.OperatorType.Or;
                base.Priority = 3;
            }

            /// <summary>
            /// 評価式に合致するかを判断します。このメソッドを実行することにより、この評価式エレメント配下の Match メソッドも評価されます。
            /// </summary>
            /// <param name="item">判断する対象の文字列</param>
            /// <returns>合致する場合 true。それ以外の場合は false。</returns>
            public override bool Match(string item)
            {
                if (base.Right == null || base.Left == null) return false;
                return base.Right.Match(item) || base.Left.Match(item);
            }
        }

        /// <summary>
        /// 否定エレメント。
        /// </summary>
        [Serializable]
        public class NotOperatorElement : OperatorElement
        {
            /// <summary>
            /// コンストラクター。
            /// </summary>
            public NotOperatorElement()
                : base()
            {
                base.Operator = OperatorElement.OperatorType.Not;
                base.Priority = 1;
            }

            /// <summary>
            /// 評価式に合致するかを判断します。このメソッドを実行することにより、この評価式エレメント配下の Match メソッドも評価されます。
            /// </summary>
            /// <param name="item">判断する対象の文字列</param>
            /// <returns>合致する場合 true。それ以外の場合は false。</returns>
            public override bool Match(string item)
            {
                if (base.Right == null)
                {
                    if (base.Left == null) return false;
                    else base.Right = base.Left;
                }
                else
                {
                    base.Left = Right;
                }
                return !base.Right.Match(item);
            }

            /// <summary>
            /// このオブジェクトを示す文字列を取得します。
            /// </summary>
            /// <returns>このオブジェクトを示す文字列。</returns>
            public new string ToString()
            {
                if (base.Left != null && base.Right == null)
                {
                    base.Right = base.Left;
                }
                else
                {
                    base.Left = base.Right;
                }
                return base.GetType().Name + " | L[ " + this.Left + " ]";
            }
        }

        /// <summary>
        /// 括弧始まりエレメント。
        /// </summary>
        [Serializable]
        public class NestBeginOperatorElement : OperatorElement
        {
            /// <summary>
            /// コンストラクター。
            /// </summary>
            public NestBeginOperatorElement()
                : base()
            {
                base.Operator = OperatorElement.OperatorType.NestBegin;
            }

            /// <summary>
            /// 評価式に合致するかを判断します。このメソッドを実行することにより、この評価式エレメント配下の Match メソッドも評価されます。
            /// </summary>
            /// <param name="item">判断する対象の文字列</param>
            /// <returns>合致する場合 true。それ以外の場合は false。</returns>
            public override bool Match(string item)
            {
                throw new InvalidCastException();
            }

            /// <summary>
            /// このオブジェクトを示す文字列を取得します。
            /// </summary>
            /// <returns>このオブジェクトを示す文字列。</returns>
            public new string ToString()
            {
                return base.GetType().Name;
            }
        }

        /// <summary>
        /// 括弧終わりエレメント。
        /// </summary>
        [Serializable]
        public class NestEndOperatorElement : OperatorElement
        {
            /// <summary>
            /// コンストラクター。
            /// </summary>
            public NestEndOperatorElement()
                : base()
            {
                base.Operator = OperatorElement.OperatorType.NestEnd;
            }

            /// <summary>
            /// 評価式に合致するかを判断します。このメソッドを実行することにより、この評価式エレメント配下の Match メソッドも評価されます。
            /// </summary>
            /// <param name="item">判断する対象の文字列</param>
            /// <returns>合致する場合 true。それ以外の場合は false。</returns>
            public override bool Match(string item)
            {
                throw new InvalidCastException();
            }

            /// <summary>
            /// このオブジェクトを示す文字列を取得します。
            /// </summary>
            /// <returns>このオブジェクトを示す文字列。</returns>
            public new string ToString()
            {
                return base.GetType().Name;
            }
        }

        /// <summary>
        /// 被演算子エレメント。
        /// </summary>
        [Serializable]
        public class OperandElement : IFilter<string>, IExpressionElement
        {
            /// <summary>
            /// このフィルターがフィルター判断の基とするオブジェクト。このクラスでは無視され、動作は変わりません。
            /// </summary>
            public object FilterObject { get; set; }

            /// <summary>
            /// この評価式エレメントのタイプ。被演算子 (Operand) です。
            /// </summary>
            public ExpressionElementType Type { get; set; }

            /// <summary>
            /// この評価式エレメントの優先度。最高の優先度 (Int32.MinValue) です。
            /// </summary>
            public int Priority { get; set; }

            /// <summary>
            /// この被演算子エレメントの内容。
            /// </summary>
            public string Keyword { get; set; }

            /// <summary>
            /// この被演算子エレメントが大文字小文字を区別するか。区別する場合は true。区別しない場合 false。
            /// </summary>
            public bool CaseSense { get; set; }

            /// <summary>
            /// コンストラクター。
            /// </summary>
            public OperandElement() : this(String.Empty) { }

            /// <summary>
            /// コンストラクター。
            /// </summary>
            /// <param name="keyword"></param>
            public OperandElement(string keyword) : this(keyword, false) { }

            /// <summary>
            /// コンストラクター。
            /// </summary>
            /// <param name="keyword"></param>
            /// <param name="casesense"></param>
            public OperandElement(string keyword, bool casesense)
            {
                this.Type = ExpressionElementType.Operand;
                this.Priority = Int32.MinValue;
                this.Keyword = keyword;
                this.CaseSense = casesense;
            }

            /// <summary>
            /// このオブジェクトを示す文字列を取得します。
            /// </summary>
            /// <returns>このオブジェクトを示す文字列。</returns>
            public new string ToString()
            {
                return this.Keyword + " | " + this.CaseSense + " (" + base.GetType().Name + ")";
            }

            /// <summary>
            /// このオブジェクトのコピーを生成します。(オブジェクト内の全てが値型のため、MemberwiseClone)
            /// </summary>
            /// <returns>このオブジェクトを示す文字列。</returns>
            public Object Clone()
            {
                return (OperandElement)this.MemberwiseClone();
            }

            /// <summary>
            /// 文字列がこのフィルターに合致するかを判断します。
            /// </summary>
            /// <param name="item">文字列。</param>
            /// <returns>このフィルターに合致する場合 true。それ以外の場合は false。</returns>
            public bool Match(string item)
            {
                if (String.IsNullOrEmpty(this.Keyword)) return true;
                if (this.CaseSense) return item.Contains(this.Keyword);
                else return item.ToLower().Contains(this.Keyword.ToLower());
            }
        }

        #region FilterClass
        private interface IFilterNode
        {
            IFilterNode ParentNode { get; set; }
            bool IsRootNode { get; }
            bool Match(string item);
        }

        private class AndNode : IFilterNode
        {
            private List<IFilterNode> NodeList;
            public IFilterNode ParentNode { get; set; }
            public bool IsRootNode
            {
                get
                {
                    return this.ParentNode == null;
                }
            }

            internal AndNode()
            {
                this.ParentNode = null;
                this.NodeList = new List<IFilterNode>();
            }

            public bool Match(string item)
            {
                foreach (IFilterNode node in this.NodeList)
                {
                    if (!node.Match(item)) return false;
                }
                return true;
            }

            internal void AddNode(IFilterNode node)
            {
                this.NodeList.Add(node);
                node.ParentNode = this;
            }
        }

        private class OrNode : IFilterNode
        {
            private List<IFilterNode> NodeList;
            public IFilterNode ParentNode { get; set; }
            public bool IsRootNode
            {
                get
                {
                    return this.ParentNode == null;
                }
            }

            internal OrNode()
            {
                this.ParentNode = null;
                this.NodeList = new List<IFilterNode>();
            }

            public bool Match(string item)
            {
                if (this.NodeList.Count == 0) return true;
                foreach (IFilterNode node in this.NodeList)
                {
                    if (node.Match(item)) return true;
                }
                return false;
            }

            internal void AddNode(IFilterNode node)
            {
                this.NodeList.Add(node);
                node.ParentNode = this;
            }
        }

        private class NotNode : IFilterNode
        {
            private IFilterNode ChildNode;
            public IFilterNode ParentNode { get; set; }
            public bool IsRootNode
            {
                get
                {
                    return this.ParentNode == null;
                }
            }

            internal NotNode()
            {
                this.ParentNode = null;
                this.ChildNode = null;
            }

            public bool Match(string item)
            {
                if (this.ChildNode == null) return true;
                return !this.ChildNode.Match(item);
            }

            internal void SetChildNode(IFilterNode node)
            {
                this.ChildNode = node;
                node.ParentNode = this;
            }
        }

        private class LeafNode : IFilterNode
        {
            private string keyword = String.Empty;
            public string Keyword
            {
                get
                {
                    return this.keyword;
                }
                set
                {
                    if (value == null) this.keyword = String.Empty;
                    this.keyword = value;
                }
            }
            public bool CaseSense { get; set; }
            public IFilterNode ParentNode { get; set; }
            public bool IsRootNode
            {
                get
                {
                    return this.ParentNode == null;
                }
            }

            internal LeafNode()
            {
                this.ParentNode = null;
                this.CaseSense = false;
            }

            public bool Match(string item)
            {
                if (this.keyword == String.Empty) return true;
                if (this.CaseSense) return item.Contains(this.keyword);
                else return item.ToLower().Contains(this.keyword.ToLower());
            }
        }
        #endregion FilterClass
        #endregion InternalClass
    }
}
