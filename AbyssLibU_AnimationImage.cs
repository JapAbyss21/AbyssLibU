using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace AbyssLibU
{

    /// <summary>
    /// アニメーションの描画を行います。
    /// </summary>
    public class AbyssLibU_AnimationImage : MonoBehaviour
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
        private Stopwatch Timer = new Stopwatch();
        /// <summary>
        /// 描画先
        /// </summary>
        private Image _Image = null;
        /// <summary>
        /// 描画先のアルファ
        /// </summary>
        private float ImageA;

        /// <summary>
        /// アニメーションの初期化を行います。
        /// </summary>
        /// <param name="Animation">アニメーションを指定します。</param>
        /// <param name="isLoop">ループするかを指定します。</param>
        /// <param name="textureDataHolder">テクスチャデータの管理用クラスを指定します。</param>
        /// <param name="isAutoKill">アニメーション終了時に自動で破棄するかを指定します。</param>
        /// <returns>自分自身のインスタンスを返します。</returns>
        public AbyssLibU_AnimationImage Init(Animation Animation, TextureDataHolder textureDataHolder, bool isAutoKill = true)
        {
            _Animation = Animation;
            this.textureDataHolder = textureDataHolder;
            this.isAutoKill = isAutoKill;
            return Init();
        }
        /// <summary>
        /// アニメーションの初期化を行います。
        /// </summary>
        /// <returns>自分自身のインスタンスを返します。</returns>
        public AbyssLibU_AnimationImage Init()
        {
            if (_Image == null)
            {
                _Image = gameObject.GetComponent<Image>();
                ImageA = _Image.color.a;
            }
            Color NewColor = _Image.color;
            NewColor.a = 0.0f;
            _Image.color = NewColor;
            IsPlaying = false;
            ImageIdx = 0;
            Timer.Stop();
            return this;
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
        /// 初期化処理です。
        /// </summary>
        private void Start()
        {
            if (_Image == null)
            {
                _Image = gameObject.GetComponent<Image>();
                ImageA = _Image.color.a;
            }
            Color NewColor = _Image.color;
            NewColor.a = 0.0f;
            _Image.color = NewColor;
        }
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
            //再生開始
            if (ImageIdx == 0 && !Timer.IsRunning)
            {
                //最初のイメージを設定
                Color NewColor = _Image.color;
                NewColor.a = ImageA;
                _Image.color = NewColor;
                ImageSetting info = _Animation.Images[ImageIdx];
                _Image.sprite = textureDataHolder.GetSprite(info.FileName, new Rect(info.X, info.Y, info.Width, info.Height));
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
                    _Image.sprite = textureDataHolder.GetSprite(info.FileName, new Rect(info.X, info.Y, info.Width, info.Height));
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
                            _Image.sprite = textureDataHolder.GetSprite(info.FileName, new Rect(info.X, info.Y, info.Width, info.Height));
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
                    }
                }
            }
        }
    }

}
