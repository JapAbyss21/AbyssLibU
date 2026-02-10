using DG.Tweening;

namespace AbyssLibU
{
    /// <summary>
    /// Tweenのストリームです。
    /// </summary>
    public class StreamTweenSequence : IStream
    {
        /// <summary>
        /// Sequence
        /// </summary>
        public Sequence Sequence { get; private set; } = DOTween.Sequence();
        /// <summary>
        /// ストリームが再生中か
        /// </summary>
        public bool IsPlaying { get; private set; } = false;
        /// <summary>
        /// ストリームが再生完了か
        /// </summary>
        public bool IsComplete { get; private set; } = false;

        /// <summary>
        /// ストリームを再生します。
        /// </summary>
        public void Play()
        {
            IsPlaying = true;
            Sequence.OnComplete(() => IsComplete = true);
            Sequence.Play();
        }
        /// <summary>
        /// ストリームの再生を停止します。
        /// </summary>
        public void Stop()
        {
            IsPlaying = false;
            Sequence.Pause();
        }
        /// <summary>
        /// フレーム毎処理です。
        /// </summary>
        public void Update()
        {
            if (IsComplete)
            {
                return;
            }
        }
    }
}