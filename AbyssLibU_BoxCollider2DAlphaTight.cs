using System.Collections.Generic;
using UnityEngine;

namespace AbyssLibU
{
    /// <summary>
    /// スプライトの透過度からタイトな矩形を生成してコライダーの判定に適用するコンポーネントクラスです。
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer)), RequireComponent(typeof(BoxCollider2D))]
    public sealed class AbyssLibU_BoxCollider2DAlphaTight : MonoBehaviour
    {
        [Range(0, 255)] public byte AlphaThreshold = 8;
        [Range(0, 4)] public int MarginPixels = 1;
        [Tooltip("falseにするとSprite.rectを使用（性能重視）")]
        public bool UseAlphaTight = true;
        //コンポーネント
        private SpriteRenderer Renderer = null;
        private BoxCollider2D Collider = null;
        //キャッシュ
        private static readonly Dictionary<(int TextureID, int x, int y, int w, int h, int Threshold), RectInt> AABBCache = new Dictionary<(int TextureID, int x, int y, int w, int h, int Threshold), RectInt>();
        private static readonly Dictionary<int, Texture2D> ReadableCache = new Dictionary<int, Texture2D>();

        // ==========================================================
        /// <summary>
        /// 有効時処理です。
        /// </summary>
        void OnEnable() => UpdateCollider();
        /// <summary>
        /// フレーム毎処理です。
        /// </summary>
        void LateUpdate() => UpdateCollider();
        /// <summary>
        /// コライダーの判定を更新します。
        /// </summary>
        private void UpdateCollider()
        {
            if (!Renderer)
            {
                Renderer = GetComponent<SpriteRenderer>();
            }
            if (!Collider)
            {
                Collider = GetComponent<BoxCollider2D>();
            }
            if (!Renderer || !Collider)
            {
                return;
            }
            if (!Renderer.sprite)
            {
                return;
            }
            if (!UseAlphaTight)
            {
                ApplySpriteRect();
                return;
            }
            ApplyAlphaTight();
        }
        /// <summary>
        /// 透過度を適用したタイトな矩形を適用します。
        /// </summary>
        private void ApplyAlphaTight()
        {
            Sprite Sprite = Renderer.sprite;
            Texture2D Texture = Sprite.texture;
            if (!Texture)
            {
                ApplySpriteRect();
                return;
            }
            Rect SpriteRect = Sprite.rect;
            int TextureID = Texture.GetInstanceID();
            int Threshold = AlphaThreshold;
            var Key = (TextureID, (int)SpriteRect.x, (int)SpriteRect.y, (int)SpriteRect.width, (int)SpriteRect.height, Threshold);
            if (!AABBCache.TryGetValue(Key, out RectInt Cached))
            {
                //読み取り可能なTexture2Dを取得（圧縮解除含む）
                Texture2D Readable = EnsureReadableCopy(Texture);
                Cached = GetAABB(Readable, SpriteRect, (byte)Threshold);
                AABBCache[Key] = Cached;
            }
            //空ならフォールバック
            if (Cached.width <= 0 || Cached.height <= 0)
            {
                ApplySpriteRect();
                return;
            }
            //マージンを適用
            if (MarginPixels > 0)
            {
                int W = Mathf.RoundToInt(SpriteRect.width);
                int H = Mathf.RoundToInt(SpriteRect.height);
                int MinX = Mathf.Max(0, Cached.xMin - MarginPixels);
                int MinY = Mathf.Max(0, Cached.yMin - MarginPixels);
                int MaxX = Mathf.Min(W - 1, Cached.xMax + MarginPixels);
                int MaxY = Mathf.Min(H - 1, Cached.yMax + MarginPixels);
                Cached = new RectInt(MinX, MinY, MaxX - MinX + 1, MaxY - MinY + 1);
            }
            //Pivot+PPU換算
            float PPU = Sprite.pixelsPerUnit > 0 ? Sprite.pixelsPerUnit : 100f;
            Vector2 Pivot = Sprite.pivot;
            float MinX2 = (Cached.xMin - Pivot.x) / PPU;
            float MaxX2 = (Cached.xMax - Pivot.x) / PPU;
            float MinY2 = (Cached.yMin - Pivot.y) / PPU;
            float MaxY2 = (Cached.yMax - Pivot.y) / PPU;
            ApplyFlipAndAssign(MinX2, MaxX2, MinY2, MaxY2);
        }
        /// <summary>
        /// スプライトの矩形に設定を適用します。
        /// </summary>
        private void ApplySpriteRect()
        {
            Sprite Sprite = Renderer.sprite;
            float PPU = Sprite.pixelsPerUnit > 0 ? Sprite.pixelsPerUnit : 100f;
            Rect Rect = Sprite.rect;
            Vector2 Pivot = Sprite.pivot;
            float MinX = -Pivot.x / PPU;
            float MaxX = (Rect.width - Pivot.x) / PPU;
            float MinY = -Pivot.y / PPU;
            float MaxY = (Rect.height - Pivot.y) / PPU;
            ApplyFlipAndAssign(MinX, MaxX, MinY, MaxY);
        }
        /// <summary>
        /// 反転設定をコライダーに適用します。
        /// </summary>
        /// <param name="MinX">最小Xを指定します。</param>
        /// <param name="MaxX">最大Xを指定します。</param>
        /// <param name="MinY">最小Yを指定します。</param>
        /// <param name="MaxY">最大Yを指定します。</param>
        private void ApplyFlipAndAssign(float MinX, float MaxX, float MinY, float MaxY)
        {
            bool FlipX = Renderer.flipX, FlipY = Renderer.flipY;
            Vector3 LossyScale = Renderer.transform.lossyScale;
            if (LossyScale.x < 0f)
            {
                FlipX = !FlipX;
            }
            if (LossyScale.y < 0f)
            {
                FlipY = !FlipY;
            }
            if (FlipX)
            {
                float nxmin = -MaxX;
                float nxmax = -MinX;
                MinX = nxmin;
                MaxX = nxmax;
            }
            if (FlipY)
            {
                float nymin = -MaxY;
                float nymax = -MinY;
                MinY = nymin;
                MaxY = nymax;
            }
            Vector2 Center = new((MinX + MaxX) * 0.5f, (MinY + MaxY) * 0.5f);
            Vector2 Size = new(Mathf.Abs(MaxX - MinX), Mathf.Abs(MaxY - MinY));
            Collider.size = Size;
            Collider.offset = Center;
        }
        /// <summary>
        /// 透過度の閾値を下回らないピクセルの矩形を取得します。
        /// </summary>
        /// <param name="Texture2D">取得元のTexture2Dを指定します。</param>
        /// <param name="SpriteRect">取得元のRectを指定します。</param>
        /// <param name="Threshold">透過度の閾値を指定します。</param>
        /// <returns>透過度の閾値を下回らないピクセルの矩形を返します。</returns>
        private static RectInt GetAABB(Texture2D Texture2D, Rect SpriteRect, byte Threshold)
        {
            int X = Mathf.FloorToInt(SpriteRect.x);
            int Y = Mathf.FloorToInt(SpriteRect.y);
            int W = Mathf.RoundToInt(SpriteRect.width);
            int H = Mathf.RoundToInt(SpriteRect.height);
            Color32[] All = Texture2D.GetPixels32();
            int Stride = Texture2D.width;
            int MinX = W, MinY = H, MaxX = -1, MaxY = -1;
            for (int yy = 0; yy < H; yy++)
            {
                int BaseIdx = (Y + yy) * Stride + X;
                for (int xx = 0; xx < W; xx++)
                {
                    byte A = All[BaseIdx + xx].a;
                    if ((Threshold == 0 && A > 0) || (Threshold > 0 && A >= Threshold))
                    {
                        if (xx < MinX)
                        {
                            MinX = xx;
                        }
                        if (yy < MinY)
                        {
                            MinY = yy;
                        }
                        if (xx > MaxX)
                        {
                            MaxX = xx;
                        }
                        if (yy > MaxY)
                        {
                            MaxY = yy;
                        }
                    }
                }
            }
            if (MaxX < 0)
            {
                return new RectInt(0, 0, 0, 0);
            }
            return new RectInt(MinX, MinY, MaxX - MinX + 1, MaxY - MinY + 1);
        }
        /// <summary>
        /// 読み取り可能なTexture2Dを取得します。
        /// </summary>
        /// <param name="Src">取得元のTexture2Dを指定します。</param>
        /// <returns>読み取り可能なTexture2Dを返します。</returns>
        private static Texture2D EnsureReadableCopy(Texture2D Src)
        {
            int ID = Src.GetInstanceID();
            if (ReadableCache.TryGetValue(ID, out Texture2D Cached))
            {
                return Cached;
            }
            Texture2D Result = SpriteUtils.EnsureReadableCopy(Src);
            ReadableCache[ID] = Result;
            return Result;
        }
    }
}