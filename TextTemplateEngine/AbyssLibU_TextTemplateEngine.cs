using System.Linq;
using System.Collections.Generic;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat;

namespace AbyssLibU
{
    /// <summary>
    /// 「キー参照 + SmartFormat」によるテンプレート文字列処理エンジンクラスです。
    /// </summary>
    public static class AbyssLibU_TextTemplateEngine
    {
        /// <summary>
        /// SmartFormatter
        /// </summary>
        private static SmartFormatter Formatter = null;
        private static AbyssLibU_DictionarySource DicSource;
        /// <summary>
        /// 辞書（言語別）
        /// </summary>
        private static Dictionary<string, Dictionary<string, string>> Dictionary = new Dictionary<string, Dictionary<string, string>>();
        /// <summary>
        /// 現在の辞書
        /// </summary>
        private static IReadOnlyDictionary<string, string> CurrentDictionary;

        /// <summary>
        /// 最大解決深度
        /// </summary>
        public static int MaxResolveDepth { get; set; } = 20;

        /// <summary>
        /// 言語リスト
        /// </summary>
        public static IReadOnlyList<string> LanguageList => Dictionary.Keys.ToList();

        /// <summary>
        /// 言語
        /// </summary>
        public static string Language
        {
            get => _Language;
            set
            {
                if (Dictionary.ContainsKey(value))
                {
                    _Language = value;
                    CurrentDictionary = Dictionary[value];
                }
            }
        }
        private static string _Language = "JP";

        /// <summary>
        /// 初期化を行います。
        /// </summary>
        private static void Init()
        {
            if (Formatter is not null)
            {
                return;
            }
            Formatter = LocalizationSettings.StringDatabase.SmartFormatter;
            DicSource = new AbyssLibU_DictionarySource(() => CurrentDictionary, () => Formatter, () => MaxResolveDepth);
            Formatter.AddExtensions(DicSource);
            if (CurrentDictionary is null)
            {
                if (Dictionary.ContainsKey(Language))
                {
                    CurrentDictionary = Dictionary[Language];
                }
                else
                {
                    CurrentDictionary = Dictionary[Language] = new Dictionary<string, string>();
                }
            }
        }

        /// <summary>
        /// 辞書にデータを追加します。
        /// </summary>
        /// <param name="Language">言語を指定します。</param>
        /// <param name="Key">キーの文字列を指定します。</param>
        /// <param name="Value">値の文字列を追加します。</param>
        public static void AddDictionaryData(string Language, string Key, string Value)
        {
            if (!Dictionary.ContainsKey(Language))
            {
                Dictionary.Add(Language, new Dictionary<string, string>());
            }
            Dictionary[Language][Key] = Value;
        }

        /// <summary>
        /// 対象の文字列に対して、「キー参照 + SmartFormat」による文字列処理を行います。
        /// </summary>
        /// <param name="Target">対象の文字列を指定します。</param>
        /// <param name="args">文字列に対する引数を指定します。</param>
        /// <returns>「キー参照 + SmartFormat」による文字列処理を行った文字列を返します。</returns>
        public static string Format(string Text, params object[] args)
        {
            if (Text is null)
            {
                return string.Empty;
            }
            if (Text.IndexOf('{') < 0)
            {
                return Text;
            }
            Init();
            AbyssLibU_ResolveContext ctx = AbyssLibU_ResolveContextPool.Rent();
            try
            {
                DicSource.SetContext(ctx);
                return Formatter.Format(Text, args);
            }
            finally
            {
                DicSource.SetContext(null);
                AbyssLibU_ResolveContextPool.Return(ctx);
            }
        }
        /// <summary>
        /// 対象の文字列に対して、「キー参照 + SmartFormat」による文字列処理を行います。
        /// </summary>
        /// <param name="Text">対象の文字列を指定します。</param>
        /// <param name="Arg0">引数0を指定します。</param>
        /// <returns>「キー参照 + SmartFormat」による文字列処理を行った文字列を返します。</returns>
#pragma warning disable IDE1006 // 命名スタイル
        public static string _FT(string Text, object Arg0) => Format(Text, Arg0);
        /// <summary>
        /// 対象の文字列に対して、「キー参照 + SmartFormat」による文字列処理を行います。
        /// </summary>
        /// <param name="Text">対象の文字列を指定します。</param>
        /// <param name="Arg0">引数0を指定します。</param>
        /// <param name="Arg1">引数1を指定します。</param>
        /// <returns>「キー参照 + SmartFormat」による文字列処理を行った文字列を返します。</returns>
        public static string _FT(string Text, object Arg0, object Arg1) => Format(Text, Arg0, Arg1);
        /// <summary>
        /// 対象の文字列に対して、「キー参照 + SmartFormat」による文字列処理を行います。
        /// </summary>
        /// <param name="Text">対象の文字列を指定します。</param>
        /// <param name="Arg0">引数0を指定します。</param>
        /// <param name="Arg1">引数1を指定します。</param>
        /// <param name="Arg2">引数2を指定します。</param>
        /// <returns>「キー参照 + SmartFormat」による文字列処理を行った文字列を返します。</returns>
        public static string _FT(string Text, object Arg0, object Arg1, object Arg2) => Format(Text, Arg0, Arg1, Arg2);
        /// <summary>
        /// 対象の文字列に対して、「キー参照 + SmartFormat」による文字列処理を行います。
        /// </summary>
        /// <param name="Text">対象の文字列を指定します。</param>
        /// <param name="Arg0">引数0を指定します。</param>
        /// <param name="Arg1">引数1を指定します。</param>
        /// <param name="Arg2">引数2を指定します。</param>
        /// <param name="Arg3">引数3を指定します。</param>
        /// <returns>「キー参照 + SmartFormat」による文字列処理を行った文字列を返します。</returns>
        public static string _FT(string Text, object Arg0, object Arg1, object Arg2, object Arg3) => Format(Text, Arg0, Arg1, Arg2, Arg3);
        /// <summary>
        /// 対象の文字列に対して、「キー参照 + SmartFormat」による文字列処理を行います。
        /// </summary>
        /// <param name="Text">対象の文字列を指定します。</param>
        /// <param name="args">文字列に対する引数を指定します。</param>
        /// <returns>「キー参照 + SmartFormat」による文字列処理を行った文字列を返します。</returns>
        public static string _FT(string Text, params object[] args) => Format(Text, args);
#pragma warning restore IDE1006 // 命名スタイル
    }
}