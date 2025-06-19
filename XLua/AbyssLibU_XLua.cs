using System;
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
        public static LuaEnv LE { get; } = new LuaEnv();

        /// <summary>
        /// スクリプトファイルを読み込み実行します。
        /// Resourcesフォルダ⇒外部データの順に読み込みを試みます。
        /// スクリプトファイルのテキストエンコーディングはUTF-8にしてください。
        /// </summary>
        /// <param name="path">スクリプトファイルのパスを指定します。</param>
        public static void DoScript(string path)
        {
            try
            {
                // TextAssetとして、Resourcesフォルダからスクリプトファイルをロードする
                TextAsset ta = Resources.Load(path) as TextAsset;
                if (ta is not null)
                {
                    LE.DoString(ta.text, path);
                }
                // テキストデータとして、外部データからスクリプトファイルをロードする
                else
                {
                    LE.DoString(File.ReadAllText(Application.dataPath + "/" + path, System.Text.Encoding.GetEncoding("UTF-8")), path);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[XLua] An error occurred while running Lua script (File: {path})\n{ex}");
                throw;
            }
        }

        /// <summary>
        /// Lua関数のデリゲートを取得します。
        /// テーブル内のLua関数も取得可能です。
        /// </summary>
        /// <typeparam name="T">デリゲート型を指定します。</typeparam>
        /// <param name="LuaFuncName">Lua関数名を指定します。</param>
        /// <returns>Lua関数のデリゲートを返します。</returns>
        public static T GetDelegateLuaFunction<T>(string LuaFuncName) where T : Delegate
        {
            try
            {
                string[] str = LuaFuncName.Split('.');
                if (str.Length == 1)
                {
                    T Result = LE.Global.Get<T>(LuaFuncName);
                    if (Result is null)
                    {
                        Debug.LogWarning($"[XLua] Lua global function not found: {LuaFuncName}");
                    }
                    return Result;
                }
                else
                {
                    LuaTable luaTable = LE.Global.Get<LuaTable>(str[0]);
                    if (luaTable is null)
                    {
                        Debug.LogWarning($"[XLua] Lua table not found: {str[0]} (while resolving {LuaFuncName})");
                        return null;
                    }
                    for (int i = 1; i < str.Length - 1; i++)
                    {
                        luaTable = luaTable.Get<LuaTable>(str[i]);
                        if (luaTable is null)
                        {
                            Debug.LogWarning($"[XLua] Lua nested table not found: {str[i]} (while resolving {LuaFuncName})");
                            return null;
                        }
                    }
                    T Result = luaTable.Get<T>(str[str.Length - 1]);
                    if (Result is null)
                    {
                        Debug.LogWarning($"[XLua] Lua function not found in table: {str[str.Length - 1]} (while resolving {LuaFuncName})");
                    }
                    return Result;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[XLua] Error resolving Lua function '{LuaFuncName}': {e.Message}");
                return null;
            }
        }
    }
}