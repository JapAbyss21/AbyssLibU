using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace AbyssLibU
{
    /// <summary>
    /// シリアライズ可能なSortedSetです。
    /// ジェネリック型はシリアライズ不可のため継承で隠蔽してください。
    /// </summary>
    /// <typeparam name="T">セット内の要素の型</typeparam>
    [Serializable]
    public abstract class SerializableSortedSet<T> : ISerializationCallbackReceiver
    {
        /// <summary>
        /// SortedSet本体
        /// </summary>
        [NonSerialized]public SortedSet<T> SortedSet = new SortedSet<T>();
#pragma warning disable CA2235 // Mark all non-serializable fields
        /// <summary>
        /// Json文字列変換用
        /// </summary>
        [SerializeField]private List<T> JsonValues;
#pragma warning restore CA2235 // Mark all non-serializable fields

        /// <summary>
        /// シリアライズ前コールバックメソッド
        /// </summary>
        public void OnBeforeSerialize()
        {
            JsonValues = SortedSet.ToList();
        }
        /// <summary>
        /// デシリアライズ後コールバックメソッド
        /// </summary>
        public void OnAfterDeserialize()
        {
            SortedSet = new SortedSet<T>();
            foreach (T e in JsonValues)
            {
                SortedSet.Add(e);
            }
        }
    }

    /// <summary>
    /// シリアライズ可能なDictionaryです。
    /// ジェネリック型はシリアライズ不可のため継承で隠蔽してください。
    /// </summary>
    /// <typeparam name="TKey">ディクショナリ内のキーの型</typeparam>
    /// <typeparam name="TValue">ディクショナリ内の値の型</typeparam>
    [Serializable]
    public abstract class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
    {
        /// <summary>
        /// Dictionary本体
        /// </summary>
        [NonSerialized]public Dictionary<TKey, TValue> Dictionary = new Dictionary<TKey, TValue>();
#pragma warning disable CA2235 // Mark all non-serializable fields
        /// <summary>
        /// Json文字列変換用（キー）
        /// </summary>
        [SerializeField]private List<TKey> Keys;
        /// <summary>
        /// Json文字列変換用（値）
        /// </summary>
        [SerializeField]private List<TValue> Values;
#pragma warning restore CA2235 // Mark all non-serializable fields

        /// <summary>
        /// シリアライズ前コールバックメソッド
        /// </summary>
        public void OnBeforeSerialize()
        {
            Keys = new List<TKey>(Dictionary.Keys);
            Values = new List<TValue>(Dictionary.Values);
        }
        /// <summary>
        /// デシリアライズ後コールバックメソッド
        /// </summary>
        public void OnAfterDeserialize()
        {
            var count = Math.Min(Keys.Count, Values.Count);
            Dictionary = new Dictionary<TKey, TValue>(count);
            for (int i = 0; i < count; i++)
            {
                Dictionary.Add(Keys[i], Values[i]);
            }
        }
    }
}