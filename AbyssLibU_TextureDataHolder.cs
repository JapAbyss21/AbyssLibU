using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace AbyssLibU
{
    /// <summary>
    /// テクスチャデータの読み込みやキャッシュの作成を行うクラスです。
    /// </summary>
    public class TextureDataHolder
    {
        /// <summary>
        /// キャッシュされたテクスチャデータ
        /// </summary>
        private Dictionary<string, Texture2D> CachedTextureData = new Dictionary<string, Texture2D>();

        /// <summary>
        /// テクスチャデータを取得します。
        /// テクスチャデータの読み込みはResourcesフォルダ⇒外部データの順に試みます。
        /// 外部データ読み込み時はラップモード=固定、フィルターモード=ポイントを設定します。
        /// </summary>
        /// <param name="path">テクスチャデータのパスを指定します。</param>
        /// <returns>テクスチャデータを返します。</returns>
        public Texture2D GetTexture2D(string path)
        {
            if (!CachedTextureData.ContainsKey(path))
            {
                Texture2D texture = Resources.Load<Texture2D>(path);
                if (texture == null)
                {
                    byte[] result;
                    using (BinaryReader bin = new BinaryReader(new FileStream(Application.dataPath + "/" + path, FileMode.Open)))
                    {
                        result = bin.ReadBytes((int)bin.BaseStream.Length);
                    }
                    texture = new Texture2D(1, 1);
                    texture.LoadImage(result);
                    texture.wrapMode = TextureWrapMode.Clamp;
                    texture.filterMode = FilterMode.Point;
                }
                CachedTextureData.Add(path, texture);
            }
            return CachedTextureData[path];
        }

        /// <summary>
        /// スプライトデータを取得します。
        /// テクスチャデータの読み込みはResourcesフォルダ⇒外部データの順に試みます。
        /// 外部データ読み込み時はラップモード=固定、フィルターモード=ポイントを設定します。
        /// </summary>
        /// <param name="path">テクスチャデータのパスを指定します。</param>
        /// <param name="rect">テクスチャのRect領域を指定します。</param>
        /// <param name="pivot">Rectに対するピボット地点の相対位置を指定します。</param>
        /// <returns>スプライトデータを返します。</returns>
        public Sprite GetSprite(string path, Rect? rect = null, Vector2? pivot = null)
        {
            Texture2D texture = GetTexture2D(path);
            return Sprite.Create(texture, rect != null ? new Rect(rect.Value.x, texture.height - rect.Value.y - rect.Value.height, rect.Value.width, rect.Value.height) :
                new Rect(0, 0, texture.width, texture.height), pivot ?? new Vector2(0.5f, 0.5f));
        }
    }
}