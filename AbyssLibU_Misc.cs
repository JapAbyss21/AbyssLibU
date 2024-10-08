﻿using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AbyssLibU
{
    /// <summary>
    /// 文字列操作のユーティリティクラスです。
    /// </summary>
    public static class StringUtils
    {
        /// <summary>
        /// 文字列にリッチテキストタグを追加します。
        /// </summary>
        /// <param name="Text">追加対象の文字列を指定します。</param>
        /// <param name="Tag">タグ名を指定します。</param>
        /// <param name="Value">設定値を指定します。</param>
        /// <returns>リッチテキストタグを追加した文字列を返します。</returns>
        public static string AddRichTextTag(string Text, string Tag, string Value = "")
        {
            string Head = "<" + Tag + (Value == "" ? ">" : "=" + Value + ">");
            string End = "</" + Tag + ">";
            return Head + Text + End;
        }
    }
    /// <summary>
    /// Spriteのユーティリティクラスです。
    /// </summary>
    public static class SpriteUtils
    {
        /// <summary>
        /// Resourcesフォルダからテクスチャデータを読み込みます。
        /// </summary>
        /// <param name="path">テクスチャデータのパスを指定します。</param>
        /// <param name="rect">テクスチャのRect領域を指定します。</param>
        /// <param name="pivot">Rectに対するピボット地点の相対位置を指定します。</param>
        /// <returns>Spriteクラスを返します。</returns>
        public static Sprite LoadResources(string path, Rect? rect = null, Vector2? pivot = null)
        {
            Texture2D texture = Resources.Load<Texture2D>(path);
            if (texture is null)
            {
                throw new FileNotFoundException("Could not find resource \"" + path + "\"");
            }
            return Sprite.Create(texture, rect is not null ? new Rect(rect.Value.x, texture.height - rect.Value.y - rect.Value.height, rect.Value.width, rect.Value.height) :
                new Rect(0, 0, texture.width, texture.height), pivot ?? new Vector2(0.5f, 0.5f));
        }
        /// <summary>
        /// 外部データからテクスチャデータを読み込みます。
        /// 読み込み時にラップモード=固定、フィルターモード=ポイントを設定します。
        /// </summary>
        /// <param name="path">テクスチャデータのパスを指定します。</param>
        /// <param name="rect">テクスチャのRect領域を指定します。</param>
        /// <param name="pivot">Rectに対するピボット地点の相対位置を指定します。</param>
        /// <returns>Spriteクラスを返します。</returns>
        public static Sprite LoadExternalData(string path, Rect? rect = null, Vector2? pivot = null)
        {
            byte[] result;
            using (BinaryReader bin = new BinaryReader(new FileStream(Application.dataPath + "/" + path, FileMode.Open)))
            {
                result = bin.ReadBytes((int)bin.BaseStream.Length);
            }
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(result);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Point;
            return Sprite.Create(texture, rect is not null ? new Rect(rect.Value.x, texture.height - rect.Value.y - rect.Value.height, rect.Value.width, rect.Value.height) :
                new Rect(0, 0, texture.width, texture.height), pivot ?? new Vector2(0.5f, 0.5f));
        }
        /// <summary>
        /// テクスチャデータを読み込みます。
        /// Resourcesフォルダ⇒外部データの順に読み込みを試みます。
        /// </summary>
        /// <param name="path">テクスチャデータのパスを指定します。</param>
        /// <param name="rect">テクスチャのRect領域を指定します。</param>
        /// <param name="pivot">Rectに対するピボット地点の相対位置を指定します。</param>
        /// <returns>Spriteクラスを返します。</returns>
        public static Sprite Load(string path, Rect? rect = null, Vector2? pivot = null)
        {
            try
            {
                return LoadResources(path, rect, pivot);
            }
            catch (FileNotFoundException)
            {
                return LoadExternalData(path, rect, pivot);
            }
        }
    }

    /// <summary>
    /// Imageのユーティリティクラスです。
    /// </summary>
    public static class ImageUtils
    {
        /// <summary>
        /// ImageクラスにSpriteクラスを設定します。
        /// </summary>
        /// <param name="name">Imageクラスの名前を指定します。</param>
        /// <param name="sprite">設定するSpriteクラスを指定します。</param>
        public static void SetSprite(string name, Sprite sprite)
        {
            try
            {
                GameObject.Find(name).GetComponent<Image>().sprite = sprite;
            }
            catch (System.NullReferenceException)
            {
                throw new System.NullReferenceException("Could not find component \"" + name + "\"");
            }
        }
        /// <summary>
        /// ImageクラスからRectTransformクラスを取得します。
        /// </summary>
        /// <param name="name">Imageクラスの名前を指定します。</param>
        /// <returns>RectTransformクラスを返します。</returns>
        public static RectTransform GetRectTransform(string name)
        {
            try
            {
                return GameObject.Find(name).GetComponent<Image>().GetComponent<RectTransform>();
            }
            catch (System.NullReferenceException)
            {
                throw new System.NullReferenceException("Could not find component \"" + name + "\"");
            }
        }
    }

    /// <summary>
    /// Buttonのユーティリティクラスです。
    /// </summary>
    public static class ButtonUtils
    {
        /// <summary>
        /// Buttonクラスのプレハブをインスタンス化します。
        /// </summary>
        /// <param name="name">プレハブ名を指定します。</param>
        /// <returns>Buttonクラスのインスタンスを返します。</returns>
        public static Button Instantiate(string name)
        {
            return UnityEngine.Object.Instantiate(Resources.Load<Button>(name));
        }
        /// <summary>
        /// ButtonクラスからTextクラスを取得します。
        /// </summary>
        /// <param name="button">Buttonクラスを指定します。</param>
        /// <returns>Textクラスを返します。</returns>
        public static Text GetButtonText(Button button)
        {
            return button.GetComponentInChildren<Text>();
        }
    }

    /// <summary>
    /// Dictionaryの拡張メソッド
    /// </summary>
    public static class DictionaryExtension
    {
        public static bool Remove<TKey, TValue>(this Dictionary<TKey, TValue> Source, Func<TKey, bool> Cond)
        {
            bool Result = false;
            IEnumerable<TKey> Keys = Source.Keys.Where(Cond).ToArray();
            foreach (var Item in Keys)
            {
                Result |= Source.Remove(Item);
            }
            return Result;
        }
    }
}