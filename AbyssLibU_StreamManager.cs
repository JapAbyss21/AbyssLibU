using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AbyssLibU
{
    /// <summary>
    /// ストリームのインターフェースです。
    /// </summary>
    public interface IStream
    {
        /// <summary>
        /// ストリームが再生中か
        /// </summary>
        bool IsPlaying { get; }
        /// <summary>
        /// ストリームが再生完了か
        /// </summary>
        bool IsComplete { get; }

        /// <summary>
        /// ストリームを再生します。
        /// </summary>
        void Play();
        /// <summary>
        /// ストリームの再生を停止します。
        /// </summary>
        void Stop();
        /// <summary>
        /// フレーム毎処理です。
        /// </summary>
        void Update();
    }

    /// <summary>
    /// アニメーションのストリームです。
    /// </summary>
    public class StreamAnimation : IStream
    {
        /// <summary>
        /// アニメーション
        /// </summary>
        private readonly IAnimationPlayer Animation = null;
        /// <summary>
        /// ストリームが再生中か
        /// </summary>
        public bool IsPlaying { get; private set; } = false;
        /// <summary>
        /// ストリームが再生完了か
        /// </summary>
        public bool IsComplete { get; private set; } = false;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="Animation">アニメーション描画クラスを指定します。</param>
        public StreamAnimation(IAnimationPlayer Animation) => this.Animation = Animation;

        /// <summary>
        /// ストリームを再生します。
        /// </summary>
        public void Play()
        {
            IsPlaying = true;
            Animation.Play();
        }
        /// <summary>
        /// ストリームの再生を停止します。
        /// </summary>
        public void Stop()
        {
            IsPlaying = false;
            Animation.Stop();
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
            IsComplete = IsPlaying && !Animation.IsPlaying;
        }
    }

    /// <summary>
    /// サウンドのストリームです。
    /// </summary>
    public class StreamSound : IStream
    {
        /// <summary>
        /// サウンドデータのパス
        /// </summary>
        private readonly string Path;
        /// <summary>
        /// サウンドデータの音量
        /// </summary>
        private readonly float Volume = 1.0f;
        /// <summary>
        /// ストリームが再生中か
        /// </summary>
        public bool IsPlaying { get; private set; } = false;
        /// <summary>
        /// ストリームが再生完了か
        /// </summary>
        public bool IsComplete { get; private set; } = false;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="path">再生するサウンドのファイルパスを指定します。</param>
        /// <param name="volume">再生するサウンドの音量を指定します。</param>
        public StreamSound(string path, float volume = 1.0f)
        {
            Path = path;
            Volume = volume;
        }

        /// <summary>
        /// ストリームを再生します。
        /// </summary>
        public void Play()
        {
            IsPlaying = true;
            AbyssLibU_AudioManager.Instance.PlaySE(Path, Volume);
            IsComplete = true;
        }
        /// <summary>
        /// ストリームの再生を停止します。
        /// </summary>
        public void Stop()
        {
            IsPlaying = false;
        }
        /// <summary>
        /// フレーム毎処理です。
        /// </summary>
        public void Update() { }
    }

    /// <summary>
    /// 遅延のストリームです。
    /// </summary>
    public class StreamDelay : IStream
    {
        /// <summary>
        /// 遅延時間（ミリ秒）
        /// </summary>
        private readonly long Duration;
        /// <summary>
        /// 内部タイマー
        /// </summary>
        private readonly Stopwatch Timer = new Stopwatch();
        /// <summary>
        /// ストリームが再生中か
        /// </summary>
        public bool IsPlaying { get; private set; } = false;
        /// <summary>
        /// ストリームが再生完了か
        /// </summary>
        public bool IsComplete { get; private set; } = false;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="Duration">遅延時間を指定します（単位：ミリ秒）</param>
        public StreamDelay(long Duration) => this.Duration = Duration;

        /// <summary>
        /// ストリームを再生します。
        /// </summary>
        public void Play()
        {
            IsPlaying = true;
            Timer.Start();
        }
        /// <summary>
        /// ストリームの再生を停止します。
        /// </summary>
        public void Stop()
        {
            IsPlaying = false;
            Timer.Stop();
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
            IsComplete = IsPlaying && Timer.ElapsedMilliseconds >= Duration;
        }
    }

    /// <summary>
    /// 複数のストリームを管理するクラスです。
    /// </summary>
    public class Streams
    {
        /// <summary>
        /// 順序性のあるストリーム
        /// </summary>
        private readonly List<HashSet<IStream>> OrderingStreams = new List<HashSet<IStream>>();
        /// <summary>
        /// 順序性のあるストリームのインデックス
        /// </summary>
        private int OrderingStreamsIndex = 0;
        /// <summary>
        /// Insertされたストリーム
        /// </summary>
        private readonly Dictionary<long, HashSet<IStream>> InsertedStreams = new Dictionary<long, HashSet<IStream>>();
        /// <summary>
        /// 内部タイマー
        /// </summary>
        private readonly Stopwatch Timer = new Stopwatch();
        /// <summary>
        /// ストリームが再生中か
        /// </summary>
        public bool IsPlaying { get; private set; } = false;
        /// <summary>
        /// ストリームが再生完了か
        /// </summary>
        public bool IsComplete { get; private set; } = false;

        /// <summary>
        /// 最後尾にストリームを追加します。
        /// ストリーム再生中は何もしません。
        /// </summary>
        /// <param name="Stream">追加するストリームを指定します。</param>
        /// <returns>自分自身のインスタンスを返します。</returns>
        public Streams Append(IStream Stream)
        {
            if (!IsPlaying)
            {
                OrderingStreams.Add(new HashSet<IStream>());
                OrderingStreams.Last().Add(Stream);
            }
            return this;
        }
        /// <summary>
        /// 最後尾と並列にストリームを追加します。
        /// ストリーム再生中は何もしません。
        /// </summary>
        /// <param name="Stream">追加するストリームを指定します。</param>
        /// <returns>自分自身のインスタンスを返します。</returns>
        public Streams Join(IStream Stream)
        {
            if (!IsPlaying)
            {
                if (OrderingStreams.Count == 0)
                {
                    return Append(Stream);
                }
                else
                {
                    OrderingStreams.Last().Add(Stream);
                }
            }
            return this;
        }
        /// <summary>
        /// 先頭にストリームを追加します。
        /// ストリーム再生中は何もしません。
        /// </summary>
        /// <param name="Stream">追加するストリームを指定します。</param>
        /// <returns>自分自身のインスタンスを返します。</returns>
        public Streams Prepend(IStream Stream)
        {
            if (!IsPlaying)
            {
                OrderingStreams.Insert(0, new HashSet<IStream>());
                OrderingStreams.First().Add(Stream);
            }
            return this;
        }
        /// <summary>
        /// 指定した時間にストリームを追加します。
        /// ストリーム再生中は何もしません。
        /// </summary>
        /// <param name="Position">時間（ミリ秒）を指定します。</param>
        /// <param name="Stream">追加するストリームを指定します。</param>
        /// <returns>自分自身のインスタンスを返します。</returns>
        public Streams Insert(long Position, IStream Stream)
        {
            if (!IsPlaying)
            {
                if (!InsertedStreams.ContainsKey(Position))
                {
                    InsertedStreams.Add(Position, new HashSet<IStream>());
                }
                InsertedStreams[Position].Add(Stream);
            }
            return this;
        }

        /// <summary>
        /// ストリームを再生します。
        /// </summary>
        public void Play()
        {
            Timer.Start();
            IsPlaying = true;
        }
        /// <summary>
        /// ストリームの再生を停止します。
        /// </summary>
        public void Stop()
        {
            Timer.Stop();
            IsPlaying = false;
        }
        /// <summary>
        /// フレーム毎処理です。
        /// </summary>
        public void Update()
        {
            //再生中ではない or 再生完了後は何もしない
            if (!IsPlaying || IsComplete)
            {
                return;
            }
            //順序性のあるストリームを実行
            bool IsCompleteOrderingStreams = OrderingStreamsIndex >= OrderingStreams.Count;
            if (!IsCompleteOrderingStreams)
            {
                foreach (var Stream in OrderingStreams[OrderingStreamsIndex])
                {
                    if (!Stream.IsPlaying)
                    {
                        Stream.Play();
                    }
                    if (!Stream.IsComplete)
                    {
                        Stream.Update();
                    }
                }
                if (OrderingStreams[OrderingStreamsIndex].All((e) => e.IsComplete))
                {
                    OrderingStreamsIndex += 1;
                    IsCompleteOrderingStreams = OrderingStreamsIndex >= OrderingStreams.Count;
                }
            }
            //Insertされたストリームを実行
            foreach (var Position in InsertedStreams.Keys.Where((e) => e <= Timer.ElapsedMilliseconds))
            {
                foreach (var Stream in InsertedStreams[Position])
                {
                    if (!Stream.IsPlaying)
                    {
                        Stream.Play();
                    }
                    if (!Stream.IsComplete)
                    {
                        Stream.Update();
                    }
                }
            }
            bool IsCompleteInsertedStreams = InsertedStreams.All((e) => e.Value.All((e2) => e2.IsComplete));
            IsComplete = IsCompleteOrderingStreams & IsCompleteInsertedStreams;
        }
    }

    /// <summary>
    /// ストリームの管理を行うマネージャクラスです。
    /// </summary>
    public class StreamManager
    {
        /// <summary>
        /// ストリーム
        /// </summary>
        private readonly HashSet<Streams> Streams = new HashSet<Streams>();

        /// <summary>
        /// ストリームを追加します。
        /// </summary>
        /// <param name="Stream">ストリームを指定します。</param>
        public void Add(Streams Stream) => Streams.Add(Stream);
        /// <summary>
        /// ストリームを削除します。
        /// </summary>
        /// <param name="Stream">ストリームを指定します。</param>
        public void Remove(Streams Stream) => Streams.Remove(Stream);
        /// <summary>
        /// ストリームをキャンセル（全削除）します。
        /// </summary>
        public void Cancel() => Streams.Clear();

        /// <summary>
        /// ストリームが空か？
        /// </summary>
        public bool IsEmpty => Streams.Count == 0;

        /// <summary>
        /// フレーム毎処理です。
        /// </summary>
        public void Update()
        {
            foreach (var Stream in Streams.ToArray())
            {
                if (!Stream.IsPlaying)
                {
                    Stream.Play();
                    if (Streams.Count == 0)
                    {
                        return;
                    }
                }
                if (!Stream.IsComplete)
                {
                    Stream.Update();
                    if (Streams.Count == 0)
                    {
                        return;
                    }
                }
            }
            Streams.RemoveWhere((e) => e.IsComplete);
        }
    }
}