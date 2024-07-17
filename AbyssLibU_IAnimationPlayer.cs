namespace AbyssLibU
{
    /// <summary>
    /// アニメーションの描画を行うインターフェースです。
    /// </summary>
    public interface IAnimationPlayer
    {
        /// <summary>
        /// アニメーションを再生中か
        /// </summary>
        bool IsPlaying { get; }
        /// <summary>
        /// アニメーションを再生します。
        /// </summary>
        void Play();
        /// <summary>
        /// アニメーションを停止します。
        /// </summary>
        void Stop();
    }
}