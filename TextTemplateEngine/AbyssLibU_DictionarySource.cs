using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.Localization.SmartFormat;
using UnityEngine.Localization.SmartFormat.Core.Extensions;

namespace AbyssLibU
{
    /// <summary>
    /// キー参照を解決するカスタムSourceクラスです。
    /// ネスト参照（参照先にさらに{dic.key}がある場合）も深さ制限付きで解決します。
    /// </summary>
    [Serializable]
    internal sealed class AbyssLibU_DictionarySource : ISource
    {
        private static readonly Regex NestedDicRefRegex = new Regex(@"\{d\.(\w+)\}", RegexOptions.Compiled);

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
            if (_ctx != null && _ctx.TryGetMemo(Key, out var MemoTemplate))
            {
                return FormatWithArgsIfNeeded(MemoTemplate, SelectorInfo);
            }
            if (_ctx != null && !_ctx.PushKey(Key))
            {
                return $"<CYCLE:{Key}>";
            }
            try
            {
                string Template = Value;
                // まずネスト解決（{d.Key}のみをRegexで解決し、{0}等のSmartFormat書式はそのまま保持する）
                if (_ctx != null && _ctx.Depth < FuncGetMaxDepth() && Template.Contains("{d."))
                {
                    _ctx.Depth++;
                    try
                    {
                        Template = ResolveNestedDicRefs(Template, Dic);
                    }
                    finally
                    {
                        _ctx.Depth--;
                    }
                }
                _ctx?.SetMemo(Key, Template);
                return FormatWithArgsIfNeeded(Template, SelectorInfo);
            }
            finally
            {
                _ctx?.PopKey(Key);
            }
        }
        /// <summary>
        /// テンプレート内の{d.Key}参照のみをRegexで解決します。
        /// SmartFormatの書式（{0}等）には触れません。
        /// </summary>
        /// <param name="Template">テンプレート文字列を指定します。</param>
        /// <param name="Dic">辞書を指定します。</param>
        /// <returns>{d.Key}参照を解決した文字列を返します。</returns>
        private string ResolveNestedDicRefs(string Template, IReadOnlyDictionary<string, string> Dic)
        {
            return NestedDicRefRegex.Replace(Template, m =>
            {
                string NestedKey = m.Groups[1].Value;
                if (Dic == null || !Dic.TryGetValue(NestedKey, out var NestedValue))
                {
                    return $"<MISSING:{NestedKey}>";
                }
                if (_ctx.TryGetMemo(NestedKey, out var MemoValue))
                {
                    return MemoValue;
                }
                if (!_ctx.PushKey(NestedKey))
                {
                    return $"<CYCLE:{NestedKey}>";
                }
                try
                {
                    string Resolved = NestedValue;
                    if (_ctx.Depth < FuncGetMaxDepth() && Resolved.Contains("{d."))
                    {
                        _ctx.Depth++;
                        try
                        {
                            Resolved = ResolveNestedDicRefs(Resolved, Dic);
                        }
                        finally
                        {
                            _ctx.Depth--;
                        }
                    }
                    _ctx.SetMemo(NestedKey, Resolved);
                    return Resolved;
                }
                finally
                {
                    _ctx.PopKey(NestedKey);
                }
            });
        }
        /// <summary>
        /// 引数埋め込みを行います。
        /// </summary>
        /// <param name="TargetText">対象の文字列を指定します。</param>
        /// <param name="SelectorInfo">ISelectorInfoクラスを指定します。</param>
        /// <returns>引数埋め込みを行った文字列を返します。</returns>
        private string FormatWithArgsIfNeeded(string TargetText, ISelectorInfo SelectorInfo)
        {
            var Args = SelectorInfo.FormatDetails?.OriginalArgs;
            if (Args is null || Args.Count == 0)
            {
                return TargetText;
            }
            if (TargetText.IndexOf('{') < 0)
            {
                return TargetText;
            }
            return FuncGetFormatter().Format(TargetText, Args is object[] A ? A : System.Linq.Enumerable.ToArray(Args));
        }
    }
}