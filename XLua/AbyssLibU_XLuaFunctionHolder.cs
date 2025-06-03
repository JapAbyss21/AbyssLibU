using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbyssLibU
{
    /// <summary>
    /// Lua関数の読み込みやキャッシュの作成を行うクラスです。
    /// </summary>
    /// <typeparam name="T">デリゲート型</typeparam>
    public class XLuaFunctionHolder<T> where T : Delegate
    {
        /// <summary>
        /// キャッシュされたLua関数
        /// </summary>
        protected Dictionary<string, T> CachedLuaFunction = new Dictionary<string, T>();

        /// <summary>
        /// Lua関数を取得します。
        /// </summary>
        /// <param name="LuaFuncName">Lua関数名を指定します。</param>
        /// <returns>Lua関数を返します。</returns>
        public T GetLuaFunction(string LuaFuncName)
        {
            if (string.IsNullOrEmpty(LuaFuncName))
            {
                return null;
            }
            if (CachedLuaFunction.TryGetValue(LuaFuncName, out T Result))
            {
                return Result;
            }
            Result = XLuaUtils.GetDelegateLuaFunction<T>(LuaFuncName);
            if (Result is not null)
            {
                CachedLuaFunction[LuaFuncName] = Result;
                return Result;
            }
            Debug.LogWarning($"[XLua] Lua function not resolved: {LuaFuncName}");
            return null;
        }
    }
}