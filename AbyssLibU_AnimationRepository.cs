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
        private static Dictionary<string, Animation> _Animation = new Dictionary<string, Animation>();

        /// <summary>
        /// 初期化を行います。
        /// </summary>
        public static void Init()
        {
            _Animation.Clear();
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
