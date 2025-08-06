using System.Collections.Generic;

namespace AbyssLibU
{
    /// <summary>
    /// 複数の値を持つ辞書クラスです。
    /// </summary>
    public class AbyssLibU_MultiValueDictionary<TKey, TValue> : Dictionary<TKey, List<TValue>>
    {
        /// <summary>
        /// 辞書に値を追加します。
        /// </summary>
        /// <param name="Key">キーを指定します。</param>
        /// <param name="Value">値を指定します。</param>
        public void Add(TKey Key, TValue Value)
        {
            if (!ContainsKey(Key))
            {
                this[Key] = new List<TValue>();
            }
            this[Key].Add(Value);
        }
        /// <summary>
        /// 辞書に複数の値を追加します。
        /// </summary>
        /// <param name="Key">キーを指定します。</param>
        /// <param name="Values">値の列挙可能なコレクションを指定します。</param>
        public void AddValues(TKey Key, IEnumerable<TValue> Values)
        {
            if (!ContainsKey(Key))
            {
                this[Key] = new List<TValue>();
            }
            this[Key].AddRange(Values);
        }
        /// <summary>
        /// 辞書から値を削除します。
        /// 値を持たなくなった場合、キーごと削除されます。
        /// </summary>
        /// <param name="Key">キーを指定します。</param>
        /// <param name="Value">値を指定します。</param>
        public void Remove(TKey Key, TValue Value)
        {
            if (ContainsKey(Key))
            {
                this[Key].Remove(Value);
                if (this[Key].Count == 0)
                {
                    base.Remove(Key);
                }
            }
        }
        /// <summary>
        /// 辞書が値を持っているかどうかを確認します。
        /// </summary>
        /// <param name="Value">値を指定します。</param>
        /// <returns>値を持っている場合はtrueを、そうでない場合はfalseを返します。</returns>
        public bool ContainsValue(TValue Value)
        {
            foreach (KeyValuePair<TKey, List<TValue>> kvp in this)
            {
                if (kvp.Value.Contains(Value))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 指定したキーに関連付けられている値を取得します。
        /// </summary>
        /// <param name="Key">キーを指定します。</param>
        /// <param name="Values">キーに関連付けられている値が存在する場合は値を格納し、そうでない場合はnullを格納します。</param>
        /// <returns>指定したキーに関連付けられている値が存在する場合はtrueを、そうでない場合はfalseを返します。</returns>
        public new bool TryGetValue(TKey Key, out List<TValue> Values)
        {
            if (ContainsKey(Key))
            {
                Values = this[Key];
                return true;
            }
            Values = null;
            return false;
        }
        /// <summary>
        /// 辞書のキーに対応する値のリストを取得または設定します。
        /// </summary>
        /// <param name="Key">キーを指定します。</param>
        /// <returns>辞書のキーに対応する値のリストを返します。</returns>
        public new List<TValue> this[TKey Key]
        {
            get => ContainsKey(Key) ? base[Key] : null;
            set => base[Key] = value;
        }
    }
}