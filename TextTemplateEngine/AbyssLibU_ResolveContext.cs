using System.Collections.Generic;
using UnityEngine.Pool;

namespace AbyssLibU
{
    /// <summary>
    /// 解決済みコンテキスト情報を管理するクラスです。
    /// </summary>
    internal sealed class AbyssLibU_ResolveContext
    {
        private readonly Dictionary<string, string> Memo = new Dictionary<string, string>(256);
        private readonly HashSet<string> Stack = new HashSet<string>();
        internal int Depth;

        /// <summary>
        /// 初期化します。
        /// </summary>
        public void Reset()
        {
            Memo.Clear();
            Stack.Clear();
            Depth = 0;
        }

        public bool TryGetMemo(string Key, out string Value) => Memo.TryGetValue(Key, out Value);
        public void SetMemo(string Key, string Value) => Memo[Key] = Value;
        public bool PushKey(string Key) => Stack.Add(Key);
        public void PopKey(string Key) => Stack.Remove(Key);
    }

    /// <summary>
    /// 解決済みコンテキスト情報管理クラスのプールです。
    /// </summary>
    internal static class AbyssLibU_ResolveContextPool
    {
        private static readonly ObjectPool<AbyssLibU_ResolveContext> Pool = new ObjectPool<AbyssLibU_ResolveContext>(
            createFunc: () => new AbyssLibU_ResolveContext(),
            actionOnGet: ctx => ctx.Reset(),
            actionOnRelease: ctx => ctx.Reset(),
            actionOnDestroy: null,
            collectionCheck: false,
            defaultCapacity: 4,
            maxSize: 64
            );

        /// <summary>
        /// プールから解決済みコンテキスト情報管理クラスを取得します。
        /// </summary>
        /// <returns>解決済みコンテキスト情報管理クラスを返します。</returns>
        public static AbyssLibU_ResolveContext Rent() => Pool.Get();
        /// <summary>
        /// プールから取得した解決済みコンテキスト情報管理クラスを返却します。
        /// </summary>
        /// <param name="ctx">解決済みコンテキスト情報管理クラスを指定します。</param>
        public static void Return(AbyssLibU_ResolveContext ctx) => Pool.Release(ctx);
    }
}