using System.IO;
using UnityEngine;
using XLua;

namespace AbyssLibU
{

    /// <summary>
    /// XLuaのユーティリティクラスです。
    /// </summary>
    public static class XLuaUtils
    {
        /// <summary>
        /// LuaEnvインスタンス
        /// </summary>
        public static LuaEnv LE = new LuaEnv();

        /// <summary>
        /// スクリプトファイルを読み込み実行します。
        /// Resourcesフォルダ⇒外部データの順に読み込みを試みます。
        /// スクリプトファイルのテキストエンコーディングはUTF-8にしてください。
        /// </summary>
        /// <param name="path">スクリプトファイルのパスを指定します。</param>
        public static void DoScript(string path)
        {
            // TextAssetとして、Resourcesフォルダからスクリプトファイルをロードする
            TextAsset ta = Resources.Load(path) as TextAsset;
            if (ta != null)
            {
                LE.DoString(ta.text);
                return;
            }
            // テキストデータとして、外部データからスクリプトファイルをロードする
            using (StreamReader sr = new StreamReader(Application.dataPath + "/" + path, System.Text.Encoding.GetEncoding("UTF-8")))
            {
                LE.DoString(sr.ReadToEnd());
            }
        }

        /// <summary>
        /// Lua関数のデリゲートを取得します。
        /// テーブル内のLua関数も取得可能です。
        /// </summary>
        /// <typeparam name="T">デリゲート型を指定します。</typeparam>
        /// <param name="function">Lua関数名を指定します。</param>
        /// <returns>Lua関数のデリゲートを返します。</returns>
        public static T GetDelegateLuaFunction<T>(string function)
        {
            string[] str = function.Split('.');
            if (str.Length == 1)
            {
                return LE.Global.Get<T>(function);
            }
            else
            {
                LuaTable luaTable = LE.Global.Get<LuaTable>(str[0]);
                for (int i = 1; i < str.Length - 1; i++)
                {
                    luaTable = luaTable.Get<LuaTable>(str[i]);
                }
                return luaTable.Get<T>(str[str.Length - 1]);
            }
        }
    }

}
