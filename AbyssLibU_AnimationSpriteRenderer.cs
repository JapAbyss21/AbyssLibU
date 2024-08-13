using System.Diagnostics;
using UnityEngine;

namespace AbyssLibU
{
    /// <summary>
    /// アニメーションの描画を行います。
    /// </summary>
    public class AbyssLibU_AnimationSpriteRenderer : MonoBehaviour, IAnimationPlayer
    {
        /// <summary>
        /// アニメーション
        /// </summary>
        private Animation _Animation;
        /// <summary>
        /// テクスチャデータの管理用クラス
        /// </summary>
        private TextureDataHolder textureDataHolder;
        /// <summary>
        /// アニメーション終了時に自動で初期化するか
        /// </summary>
        private bool isInit = false;
        /// <summary>
        /// アニメーション終了時に自動で破棄するか
        /// </summary>
        private bool isAutoKill = true;
        /// <summary>
        /// アニメーションを再生中か
        /// </summary>
        public bool IsPlaying { get; private set; } = false;
        /// <summary>
        /// イメージの現行インデックス
        /// </summary>
        private int ImageIdx = 0;
        /// <summary>
        /// 内部タイマー
        /// </summary>
        private readonly Stopwatch Timer = new Stopwatch();
        /// <summary>
        /// 描画先
        /// </summary>
        public SpriteRenderer SpriteRenderer => _SpriteRenderer != null ? _SpriteRenderer : _SpriteRenderer = GetComponent<SpriteRenderer>();
        private SpriteRenderer _SpriteRenderer = null;
        /// <summary>
        /// スプライトのPivot
        /// </summary>
        private Vector2? Pivot = null;
        /// <summary>
        /// スプライトのサイズ
        /// </summary>
        private Vector2 SpriteSize;
        /// <summary>
        /// 1ユニットのピクセル数
        /// </summary>
        private const int PixelPerUnit = 100;
        /// <summary>
        /// 描画先のアルファ
        /// </summary>
        public float SpriteA
        {
            get
            {
                return SpriteRenderer.color.a;
            }
            set
            {
                Color NewColor = SpriteRenderer.color;
                NewColor.a = value;
                SpriteRenderer.color = NewColor;
            }
        }

        /// <summary>
        /// 同期モードか
        /// </summary>
        private bool IsSynchronousMode = false;
        /// <summary>
        /// 同期元のアニメーション描画クラス
        /// </summary>
        private AbyssLibU_AnimationSpriteRenderer SynchronousSource = null;

        /// <summary>
        /// アニメーションの初期化を行います。
        /// </summary>
        /// <param name="Animation">アニメーションを指定します。</param>
        /// <param name="textureDataHolder">テクスチャデータの管理用クラスを指定します。</param>
        /// <param name="isInit">アニメーション終了時に自動で初期化するかを指定します。</param>
        /// <param name="isAutoKill">アニメーション終了時に自動で破棄するかを指定します。</param>
        /// <param name="pivot">スプライトに対するピボット地点の相対位置を指定します。</param>
        /// <returns>自分自身のインスタンスを返します。</returns>
        public AbyssLibU_AnimationSpriteRenderer Init(Animation Animation, TextureDataHolder textureDataHolder, bool isInit = false, bool isAutoKill = true, Vector2? pivot = null)
        {
            _Animation = Animation;
            this.textureDataHolder = textureDataHolder;
            this.isInit = isInit;
            this.isAutoKill = isAutoKill;
            Pivot = pivot;
            SpriteSize = new Vector2(GetComponent<RectTransform>().sizeDelta.x * PixelPerUnit, GetComponent<RectTransform>().sizeDelta.y * PixelPerUnit);
            return Init();
        }
        /// <summary>
        /// アニメーションの初期化を行います。
        /// </summary>
        /// <returns>自分自身のインスタンスを返します。</returns>
        public AbyssLibU_AnimationSpriteRenderer Init()
        {
            SpriteRenderer.enabled = false;
            IsPlaying = false;
            ImageIdx = 0;
            Timer.Stop();
            return this;
        }

        /// <summary>
        /// <para>アニメーションの初期化を同期モードで行います。</para>
        /// <para>同期モード：同期元と完全に同一のアニメーションを描画します。独立要素は色、再生/停止だけです。</para>
        /// </summary>
        /// <param name="SynchronousSource">同期元のアニメーション描画クラスを指定します。</param>
        /// <returns>自分自身のインスタンスを返します。</returns>
        public AbyssLibU_AnimationSpriteRenderer InitSynchronousMode(AbyssLibU_AnimationSpriteRenderer SynchronousSource)
        {
            IsSynchronousMode = true;
            this.SynchronousSource = SynchronousSource;
            return Init();
        }

        /// <summary>
        /// アニメーションを再生します。
        /// </summary>
        public void Play() => IsPlaying = true;
        /// <summary>
        /// アニメーションを停止します。
        /// </summary>
        public void Stop() => IsPlaying = false;

        /// <summary>
        /// フレーム毎処理です。
        /// </summary>
        private void Update()
        {
            //再生中ではない
            if (!IsPlaying)
            {
                return;
            }
            //非同期モード
            if (!IsSynchronousMode)
            {
                //再生開始
                if (ImageIdx == 0 && !Timer.IsRunning)
                {
                    //最初のイメージを設定
                    SpriteRenderer.enabled = true;
                    ImageSetting info = _Animation.Images[ImageIdx];
                    SpriteRenderer.sprite = textureDataHolder.GetSprite(info.FileName, new Rect(info.X, info.Y, info.Width, info.Height), Pivot);
                    float NewScale = Mathf.Min(SpriteSize.x / info.Width, SpriteSize.y / info.Height) * 100;
                    transform.localScale = new Vector3(NewScale, NewScale);
                    Timer.Start();
                    return;
                }
                //再生中
                if (Timer.ElapsedMilliseconds >= _Animation.FrameTime)
                {
                    if (++ImageIdx < _Animation.Images.Count)
                    {
                        //次のイメージを設定
                        ImageSetting info = _Animation.Images[ImageIdx];
                        SpriteRenderer.sprite = textureDataHolder.GetSprite(info.FileName, new Rect(info.X, info.Y, info.Width, info.Height), Pivot);
                        float NewScale = Mathf.Min(SpriteSize.x / info.Width, SpriteSize.y / info.Height) * 100;
                        transform.localScale = new Vector3(NewScale, NewScale);
                        Timer.Restart();
                    }
                    else
                    {
                        if (_Animation.isLoop)
                        {
                            //複数枚のアニメーションか確認（1枚なら最初のイメージの再設定が不要）
                            if (_Animation.Images.Count > 1)
                            {
                                //最初のイメージを設定（ループ）
                                ImageIdx = 0;
                                ImageSetting info = _Animation.Images[ImageIdx];
                                SpriteRenderer.sprite = textureDataHolder.GetSprite(info.FileName, new Rect(info.X, info.Y, info.Width, info.Height), Pivot);
                                float NewScale = Mathf.Min(SpriteSize.x / info.Width, SpriteSize.y / info.Height) * 100;
                                transform.localScale = new Vector3(NewScale, NewScale);
                                Timer.Restart();
                            }
                        }
                        else
                        {
                            //アニメーション終了
                            if (isAutoKill)
                            {
                                //自動で破棄
                                Destroy(gameObject);
                            }
                            else
                            {
                                if (isInit)
                                {
                                    Init();
                                }
                            }
                        }
                    }
                }
            }
            //同期モード
            else
            {
                SpriteRenderer.enabled = SynchronousSource.SpriteRenderer.enabled;
                SpriteRenderer.sprite = SynchronousSource.SpriteRenderer.sprite;
                transform.localScale = SynchronousSource.transform.localScale;
            }
        }
    }
}