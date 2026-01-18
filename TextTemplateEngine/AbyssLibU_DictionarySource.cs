using System;
using System.Collections.Generic;
using UnityEngine.Localization.SmartFormat;
using UnityEngine.Localization.SmartFormat.Core.Extensions;

namespace AbyssLibU
{
    /// <summary>
    /// キー参照を解決するカスタムSourceクラスです。
    /// ネスト参照（参照先にさらに{dic.key}がある場合）も深さ制限付きで解決します。
    /// </summary>
    internal sealed class AbyssLibU_DictionarySource : ISource
    {
        private readonly Func<IReadOnlyDictionary<string, string>> FuncGetDic;
        private readonly Func<SmartFormatter> FuncGetFormatter;
        private readonly Func<int> FuncGetMaxDepth;
        private AbyssLibU_ResolveContext _ctx = null;

        /// <summary>
        /// デフォルトコンストラクタです。
        /// </summary>
        /// <param name="FuncGetDic">辞書取得関数を指定します。</param>
        /// <param name="FuncGetFormatter">SmartFormatter取得関数を指定します。</param>
        /// <param name="FuncGetMaxDepth">最大の深さを指定します。</param>
        public AbyssLibU_DictionarySource(Func<IReadOnlyDictionary<string, string>> FuncGetDic, Func<SmartFormatter> FuncGetFormatter, Func<int> FuncGetMaxDepth)
        {
            this.FuncGetDic = FuncGetDic;
            this.FuncGetFormatter = FuncGetFormatter;
            this.FuncGetMaxDepth = FuncGetMaxDepth;
        }

        /// <summary>
        /// 解決済みコンテキスト情報を設定します。
        /// </summary>
        /// <param name="ctx">解決済みコンテキスト情報を指定します。</param>
        public void SetContext(AbyssLibU_ResolveContext ctx) => _ctx = ctx;

        /// <summary>
        /// ISelectorInfo.CurrentValueに基づいてISelectorInfoを評価します。
        /// </summary>
        /// <param name="SelectorInfo">ISelectorInfoクラスを指定します。</param>
        /// <returns>評価できない場合はfalseを、そうでない場合は結果を設定しTrueを返します。</returns>
        public bool TryEvaluateSelector(ISelectorInfo SelectorInfo)
        {
            if (SelectorInfo.SelectorText == "d")
            {
                IReadOnlyDictionary<string, string> Dic = FuncGetDic();
                SelectorInfo.Result = Dic;
                return true;
            }
            if (SelectorInfo.CurrentValue is IReadOnlyDictionary<string, string> Dict)
            {
                var Key = SelectorInfo.SelectorText;
                var Resolved = ResolveKey(Key, Dict, SelectorInfo);
                SelectorInfo.Result = Resolved;
                return true;
            }
            return false;
        }

        /// <summary>
        /// キー参照を解決します。
        /// </summary>
        /// <param name="Key">キーを指定します。</param>
        /// <param name="Dic">辞書を指定します。</param>
        /// <param name="SelectorInfo">ISelectorInfoクラスを指定します。</param>
        /// <returns>キー参照解決後の文字列を返します。</returns>
        private string ResolveKey(string Key, IReadOnlyDictionary<string, string> Dic, ISelectorInfo SelectorInfo)
        {
            if (Dic == null || !Dic.TryGetValue(Key, out var Value))
            {
                return $"<MISSING:{Key}>";
            }
            if (_ctx != null && _ctx.TryGetMemo(Key, out var Memo))
            {
                return Memo;
            }
            if (_ctx != null && !_ctx.PushKey(Key))
            {
                return $"<CYCLE:{Key}>";
            }
            try
            {
                string Result = Value;
                // ネスト解決：結果に {d. が含まれていたら、同じargsで再フォーマット
                if (_ctx != null && _ctx.Depth < FuncGetMaxDepth() && Result.Contains("{d."))
                {
                    _ctx.Depth++;
                    try
                    {
                        IList<object> args = SelectorInfo.FormatDetails?.OriginalArgs;
                        Result = FuncGetFormatter().Format(Result, args ?? Array.Empty<object>());
                    }
                    finally
                    {
                        _ctx.Depth--;
                    }
                }
                _ctx?.SetMemo(Key, Result);
                return Result;
            }
            finally
            {
                _ctx?.PopKey(Key);
            }
        }
    }
}