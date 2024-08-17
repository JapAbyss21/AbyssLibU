using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace AbyssLibU
{
    /// <summary>
    /// アニメーションの描画を行います。
    /// </summary>
    public class AbyssLibU_AnimationImage : MonoBehaviour, IAnimationPlayer
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
        public Image Image => _Image != null ? _Image : _Image = GetComponent<Image>();
        private Image _Image = null;
        /// <summary>
        /// 描画先のアルファ
        /// </summary>
        public float ImageA
        {
            get
            {
                return Image.color.a;
            }
            set
            {
                Color NewColor = Image.color;
                NewColor.a = value;
                Image.color = NewColor;
            }
        }

        /// <summary>
        /// 同期モードか
        /// </summary>
        private bool IsSynchronousMode = false;
        /// <summary>
        /// 同期元のアニメーション描画クラス
        /// </summary>
        private AbyssLibU_AnimationImage SynchronousSource = null;

        /// <summary>
        /// アニメーションの初期化を行います。
        /// </summary>
        /// <param name="Animation">アニメーションを指定します。</param>
        /// <param name="textureDataHolder">テクスチャデータの管理用クラスを指定します。</param>
        /// <param name="isInit">アニメーション終了時に自動で初期化するかを指定します。</param>
        /// <param name="isAutoKill">アニメーション終了時に自動で破棄するかを指定します。</param>
        /// <returns>自分自身のインスタンスを返します。</returns>
        public AbyssLibU_AnimationImage Init(Animation Animation, TextureDataHolder textureDataHolder, bool isInit = false, bool isAutoKill = true)
        {
            _Animation = Animation;
            this.textureDataHolder = textureDataHolder;
            this.isInit = isInit;
            this.isAutoKill = isAutoKill;
            return Init();
        }
        /// <summary>
        /// アニメーションの初期化を行います。
        /// </summary>
        /// <returns>自分自身のインスタンスを返します。</returns>
        public AbyssLibU_AnimationImage Init()
        {
            Image.enabled = false;
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
        public AbyssLibU_AnimationImage InitSynchronousMode(AbyssLibU_AnimationImage SynchronousSource)
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
                    Image.enabled = true;
                    Image.sprite = textureDataHolder.GetSprite(_Animation.Images[ImageIdx]);
                    Timer.Start();
                    return;
                }
                //再生中
                if (Timer.ElapsedMilliseconds >= _Animation.FrameTime)
                {
                    if (++ImageIdx < _Animation.Images.Count)
                    {
                        //次のイメージを設定
                        Image.sprite = textureDataHolder.GetSprite(_Animation.Images[ImageIdx]);
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
                                Image.sprite = textureDataHolder.GetSprite(_Animation.Images[ImageIdx]);
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
                Image.enabled = SynchronousSource.Image.enabled;
                Image.sprite = SynchronousSource.Image.sprite;
            }
        }
    }
}