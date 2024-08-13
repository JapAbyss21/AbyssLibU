using System;
using System.Collections.Generic;

namespace AbyssLibU
{
    /// <summary>
    /// アニメーション
    /// </summary>
    public class Animation : ICloneable
    {
        /// <summary>
        /// イメージ（複数）
        /// </summary>
        public List<ImageSetting> Images = new List<ImageSetting>();
        /// <summary>
        /// フレームレート
        /// </summary>
        public int FPS = 30;
        /// <summary>
        /// 1フレーム（単位：ミリ秒）
        /// </summary>
        public int FrameTime => 1000 / FPS;
        /// <summary>
        /// ループするか
        /// </summary>
        public bool isLoop = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FileName">ファイル名を指定します。</param>
        /// <param name="ImageX"></param>
        /// <param name="ImageY"></param>
        /// <param name="ImageWidth">イメージの横幅を指定します。</param>
        /// <param name="ImageHeight">イメージの縦幅を指定します。</param>
        public void AddImage(string FileName, int ImageX, int ImageY, int ImageWidth, int ImageHeight) => Images.Add(new ImageSetting { FileName = FileName, X = ImageX, Y = ImageY, Width = ImageWidth, Height = ImageHeight });

        /// <summary>
        /// 現在のインスタンスのコピーである新しいオブジェクトを作成します。
        /// </summary>
        /// <returns>現在のインスタンスのコピーである新しいオブジェクトを返します。</returns>
        public object Clone()
        {
            Animation NewObj = new Animation
            {
                Images = new List<ImageSetting>(Images),
                FPS = FPS,
                isLoop = isLoop
            };
            return NewObj;
        }
    }

    /// <summary>
    /// アニメーションのリポジトリです。
    /// </summary>
    public static class AnimationRepository
    {
        /// <summary>
        /// アニメーションのリポジトリ
        /// </summary>
        private static readonly Dictionary<string, Animation> _Animation = new Dictionary<string, Animation>();

        /// <summary>
        /// 初期化を行います。
        /// </summary>
        public static void Init() => _Animation.Clear();

        /// <summary>
        /// アニメーションを設定します。
        /// </summary>
        /// <param name="Name">アニメーション名を指定します。</param>
        /// <param name="Animation">設定するアニメーションを指定します。</param>
        public static void SetAnimation(string Name, Animation Animation) => _Animation[Name] = Animation;
        /// <summary>
        /// アニメーションを設定します。
        /// </summary>
        /// <param name="Name">アニメーション名を指定します。</param>
        /// <param name="FileName">ファイル名を指定します。</param>
        /// <param name="ImageNum">イメージ数を指定します。</param>
        /// <param name="ImageX">イメージのX座標を指定します。</param>
        /// <param name="ImageY">イメージのY座標を指定します。</param>
        /// <param name="ImageWidth">イメージの横幅を指定します。</param>
        /// <param name="ImageHeight">イメージの縦幅を指定します。</param>
        /// <param name="isLoop">ループするかを指定します（デフォルト：しない）</param>
        /// <param name="FPS">フレームレートを指定します（デフォルト：30）</param>
        /// <param name="isHorizontal">アニメーションの方向が水平かを指定します（true：水平、false：垂直）</param>
        public static void SetAnimation(string Name, string FileName, int ImageNum, int ImageX, int ImageY, int ImageWidth, int ImageHeight, bool isLoop = false, int FPS = 30, bool isHorizontal = true)
        {
            Animation NewAnimation = new Animation { FPS = FPS, isLoop = isLoop };
            for (int i = 0; i < ImageNum; i++)
            {
                int BaseX = ImageX + (isHorizontal ? i * ImageWidth : 0);
                int BaseY = ImageY + (isHorizontal ? 0 : i * ImageHeight);
                NewAnimation.Images.Add(new ImageSetting { FileName = FileName, X = BaseX, Y = BaseY, Width = ImageWidth, Height = ImageHeight });
            }
            _Animation[Name] = NewAnimation;
        }
        /// <summary>
        /// アニメーションを設定します。
        /// </summary>
        /// <param name="Name">アニメーション名を指定します。</param>
        /// <param name="FileName">ファイル名を指定します。</param>
        /// <param name="ImageNum">イメージ数を指定します。</param>
        /// <param name="ImageWidth">イメージの横幅を指定します。</param>
        /// <param name="ImageHeight">イメージの縦幅を指定します。</param>
        /// <param name="DivNumX">イメージの分割数（横）を指定します。</param>
        /// <param name="isLoop">ループするかを指定します（デフォルト：しない）</param>
        /// <param name="FPS">フレームレートを指定します（デフォルト：30）</param>
        public static void SetAnimation(string Name, string FileName, int ImageNum, int ImageWidth, int ImageHeight, int DivNumX, bool isLoop = false, int FPS = 30)
        {
            Animation NewAnimation = new Animation { FPS = FPS, isLoop = isLoop };
            for (int i = 0; i < ImageNum; i++)
            {
                int ImageX = ImageWidth * (i < DivNumX ? i : (i % DivNumX));
                int ImageY = ImageHeight * (i < DivNumX ? 0 : (i / DivNumX));
                NewAnimation.Images.Add(new ImageSetting { FileName = FileName, X = ImageX, Y = ImageY, Width = ImageWidth, Height = ImageHeight });
            }
            _Animation[Name] = NewAnimation;
        }
        /// <summary>
        /// アニメーションを追加します。
        /// </summary>
        /// <param name="Name">アニメーション名を指定します。</param>
        /// <param name="Animation">追加するアニメーションを指定します。</param>
        public static void AddAnimatiion(string Name, Animation Animation) => _Animation[Name].Images.AddRange(Animation.Images);
        /// <summary>
        /// アニメーションを追加します。
        /// </summary>
        /// <param name="Name">アニメーション名を指定します。</param>
        /// <param name="FileName">ファイル名を指定します。</param>
        /// <param name="ImageNum">イメージ数を指定します。</param>
        /// <param name="ImageX">イメージのX座標を指定します。</param>
        /// <param name="ImageY">イメージのY座標を指定します。</param>
        /// <param name="ImageWidth">イメージの横幅を指定します。</param>
        /// <param name="ImageHeight">イメージの縦幅を指定します。</param>
        /// <param name="isHorizontal">アニメーションの方向が水平かを指定します（true：水平、false：垂直）</param>
        public static void SetAnimation(string Name, string FileName, int ImageNum, int ImageX, int ImageY, int ImageWidth, int ImageHeight, bool isHorizontal = true)
        {
            for (int i = 0; i < ImageNum; i++)
            {
                int BaseX = ImageX + (isHorizontal ? i * ImageWidth : 0);
                int BaseY = ImageY + (isHorizontal ? 0 : i * ImageHeight);
                _Animation[Name].Images.Add(new ImageSetting { FileName = FileName, X = BaseX, Y = BaseY, Width = ImageWidth, Height = ImageHeight });
            }
        }
        /// <summary>
        /// アニメーションを追加します。
        /// </summary>
        /// <param name="Name">アニメーション名を指定します。</param>
        /// <param name="FileName">ファイル名を指定します。</param>
        /// <param name="ImageNum">イメージ数を指定します。</param>
        /// <param name="ImageWidth">イメージの横幅を指定します。</param>
        /// <param name="ImageHeight">イメージの縦幅を指定します。</param>
        /// <param name="DivNumX">イメージの分割数（横）を指定します。</param>
        public static void AddAnimation(string Name, string FileName, int ImageNum, int ImageWidth, int ImageHeight, int DivNumX)
        {
            for (int i = 0; i < ImageNum; i++)
            {
                int ImageX = ImageWidth * (i < DivNumX ? i : (i % DivNumX));
                int ImageY = ImageHeight * (i < DivNumX ? i : (i / DivNumX));
                _Animation[Name].Images.Add(new ImageSetting { FileName = FileName, X = ImageX, Y = ImageY, Width = ImageWidth, Height = ImageHeight });
            }
        }
        /// <summary>
        /// アニメーションを取得します。
        /// </summary>
        /// <param name="Name">アニメーション名を指定します。</param>
        /// <returns>アニメーションを返します。</returns>
        public static Animation GetAnimation(string Name) => (Animation)_Animation[Name].Clone();
    }
}