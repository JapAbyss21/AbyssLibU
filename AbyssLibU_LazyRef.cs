using UnityEngine;

namespace AbyssLibU
{
    /// <summary>
    /// Transform配下のオブジェクト参照を取得・キャッシュするラッパークラスです。
    /// 初回アクセス時にFindし、以後はキャッシュされたGameObjectまたはComponentを返します。
    /// </summary>
    /// <typeparam name="T">参照するオブジェクトの型（GameObjectまたはComponent）を指定します。</typeparam>
    public class AbyssLibU_LazyRef<T> where T : Object
    {
        /// <summary>
        /// 検索の起点となるTransform
        /// </summary>
        private readonly Transform Root;
        /// <summary>
        /// 検索の起点となるTransformからの相対パス
        /// </summary>
        private readonly string Path;
        /// <summary>
        /// キャッシュ
        /// </summary>
        private T Cache;

        /// <summary>
        /// デフォルトコンストラクタ（禁止）です。
        /// </summary>
        private AbyssLibU_LazyRef() { }
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="Root">検索の起点となるTransformを指定します。</param>
        /// <param name="Path">検索の起点となるTransformからの相対パス（例："UI/Button"）を指定します。</param>
        public AbyssLibU_LazyRef(Transform Root, string Path)
        {
            this.Root = Root;
            this.Path = Path;
        }

        /// <summary>
        /// オブジェクト参照を取得します。
        /// 初回アクセス時にFindし、以降はキャッシュされた参照を返します。
        /// </summary>
        public T Ref
        {
            get
            {
                if (Cache is not null)
                {
                    return Cache;
                }
                else
                {
                    if (typeof(T) == typeof(GameObject))
                    {
                        return Cache = Root.Find(Path).gameObject as T;
                    }
                    else
                    {
                        return Cache = Root.Find(Path).GetComponent<T>();
                    }
                }
            }
        }
    }
}