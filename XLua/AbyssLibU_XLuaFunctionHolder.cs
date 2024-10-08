﻿using System;
using System.Collections.Generic;

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
            if (LuaFuncName is null)
            {
                return null;
            }
            if (!CachedLuaFunction.ContainsKey(LuaFuncName))
            {
                T NewFunc = XLuaUtils.GetDelegateLuaFunction<T>(LuaFuncName);
                if (NewFunc is not null)
                {
                    CachedLuaFunction.Add(LuaFuncName, NewFunc);
                }
            }
            return CachedLuaFunction[LuaFuncName];
        }
    }
}