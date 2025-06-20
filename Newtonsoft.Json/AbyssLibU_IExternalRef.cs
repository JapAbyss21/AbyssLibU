namespace AbyssLibU
{
#pragma warning disable IDE1006 // 命名スタイル
    /// <summary>
    /// 外部参照用インターフェース
    /// </summary>
    /// <typeparam name="T">外部参照する型。AbyssLibU_IExternalRefRootインターフェースが必要です。</typeparam>
    public interface AbyssLibU_IExternalRef<T> where T : AbyssLibU_IExternalRefRoot
    {
        /// <summary>
        /// 外部参照を設定します。
        /// </summary>
        /// <param name="ExRefRoot">外部参照を指定します。</param>
        public void SetExternalRef(T ExRefRoot);
    }

    /// <summary>
    /// 外部参照用インターフェース（外部参照が2つの場合）
    /// </summary>
    /// <typeparam name="T">外部参照する型。AbyssLibU_IExternalRefRootインターフェースが必要です。</typeparam>
    /// <typeparam name="T2">外部参照する型（その2）</typeparam>
    public interface AbyssLibU_IExternalRefs<T, T2> where T : AbyssLibU_IExternalRefRoot
    {
        /// <summary>
        /// 外部参照を設定します。
        /// </summary>
        /// <param name="ExRefRoot">外部参照（その1）を指定します。</param>
        /// <param name="ExRef2">外部参照（その2）を指定します。</param>
        public void SetExternalRefs(T ExRefRoot, T2 ExRef2);
    }

    /// <summary>
    /// 外部参照用インターフェース（外部参照が2つの場合）
    /// </summary>
    /// <typeparam name="T">外部参照する型。AbyssLibU_IExternalRefRootインターフェースが必要です。</typeparam>
    /// <typeparam name="T2">外部参照する型（その2）</typeparam>
    /// <typeparam name="T3">外部参照する型（その3）</typeparam>
    public interface AbyssLibU_IExternalRefs<T, T2, T3> where T : AbyssLibU_IExternalRefRoot
    {
        /// <summary>
        /// 外部参照を設定します。
        /// </summary>
        /// <param name="ExRefRoot">外部参照（その1）を指定します。</param>
        /// <param name="ExRef2">外部参照（その2）を指定します。</param>
        /// <param name="ExRef3">外部参照（その3）を指定します。</param>
        public void SetExternalRefs(T ExRefRoot, T2 ExRef2, T3 ExRef3);
    }
#pragma warning restore IDE1006 // 命名スタイル
}