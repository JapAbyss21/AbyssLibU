using System.Collections.Generic;
using UnityEngine;

namespace AbyssLibU
{
    /// <summary>
    /// Transform配下のオブジェクト参照を取得・キャッシュする辞書型ラッパークラスです。
    /// 初回アクセス時にFindし、以後はキャッシュされたGameObjectを返します。
    /// </summary>
    public class AbyssLibU_LazyRefDictionary
    {
        /// <summary>
        /// 検索の起点となるTransform
        /// </summary>
        private readonly Transform Root;
        /// <summary>
        /// キャッシュ
        /// </summary>
        private readonly Dictionary<string, GameObject> Cache = new Dictionary<string, GameObject>();

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="Root">検索の起点となるTransformを指定します。</param>
        public AbyssLibU_LazyRefDictionary(Transform Root) => this.Root = Root;

        /// <summary>
        /// 指定したキーに対応するオブジェクト参照を取得します。
        /// 初回アクセス時にFindし、以降はキャッシュされた参照を返します。
        /// </summary>
        /// <param name="Key">検索の起点となるTransformからの相対パスを指定します。</param>
        /// <returns>オブジェクト参照を返します。</returns>
        public GameObject this[string Key]
        {
            get
            {
                if (Cache.TryGetValue(Key, out GameObject Value))
                {
                    return Value;
                }
                Transform Found = Root.Find(Key);
                if (Found == null)
                {
                    Debug.LogError($"[AbyssLibU_LazyRefDictionary] Key not found: '{Key}' under '{Root.name}'");
                    return null;
                }
                Cache[Key] = Found.gameObject;
                return Found.gameObject;
            }
        }
    }
}