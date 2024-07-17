using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XLua;

namespace AbyssLibU
{

    /// <summary>
    /// エフェクトのインターフェースです。
    /// </summary>
    public interface IEffect
    {
        /// <summary>
        /// エフェクトが再生中か
        /// </summary>
        bool IsPlaying { get; }
        /// <summary>
        /// エフェクトが再生完了か
        /// </summary>
        bool IsComplete { get; }

        /// <summary>
        /// エフェクトを再生します。
        /// </summary>
        void Play();
        /// <summary>
        /// エフェクトの再生を停止します。
        /// </summary>
        void Stop();
        /// <summary>
        /// フレーム毎処理です。
        /// </summary>
        void Update();
    }

    /// <summary>
    /// アニメーションのエフェクトです。
    /// </summary>
    public class EffectAnimation : IEffect
    {
        /// <summary>
        /// アニメーション
        /// </summary>
        private IAnimationPlayer Animation = null;
        /// <summary>
        /// エフェクトが再生中か
        /// </summary>
        public bool IsPlaying { get; private set; } = false;
        /// <summary>
        /// エフェクトが再生完了か
        /// </summary>
        public bool IsComplete { get; private set; } = false;

        /// <summary>
        /// デフォルトコンストラクタ（禁止）です。
        /// </summary>
        private EffectAnimation() { }
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="Animation"></param>
        public EffectAnimation(IAnimationPlayer Animation) => this.Animation = Animation;

        /// <summary>
        /// エフェクトを再生します。
        /// </summary>
        public void Play()
        {
            IsPlaying = true;
            Animation.Play();
        }
        /// <summary>
        /// エフェクトの再生を停止します。
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
    /// サウンドのエフェクトです。
    /// </summary>
    public class EffectSound : IEffect
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
        /// エフェクトが再生中か
        /// </summary>
        public bool IsPlaying { get; private set; } = false;
        /// <summary>
        /// エフェクトが再生完了か
        /// </summary>
        public bool IsComplete { get; private set; } = false;

        /// <summary>
        /// デフォルトコンストラクタ（禁止）です。
        /// </summary>
        private EffectSound() { }
        public EffectSound(string path, float volume = 1.0f)
        {
            Path = path;
            Volume = volume;
        }

        /// <summary>
        /// エフェクトを再生します。
        /// </summary>
        public void Play()
        {
            IsPlaying = true;
            AbyssLibU_AudioManager.Instance.PlaySE(Path, Volume);
            IsComplete = true;
        }
        /// <summary>
        /// エフェクトの再生を停止します。
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
    /// 遅延のエフェクトです。
    /// </summary>
    public class EffectDelay : IEffect
    {
        /// <summary>
        /// 遅延時間（ミリ秒）
        /// </summary>
        private readonly long Duration;
        /// <summary>
        /// 内部タイマー
        /// </summary>
        private Stopwatch Timer = new Stopwatch();
        /// <summary>
        /// エフェクトが再生中か
        /// </summary>
        public bool IsPlaying { get; private set; } = false;
        /// <summary>
        /// エフェクトが再生完了か
        /// </summary>
        public bool IsComplete { get; private set; } = false;

        /// <summary>
        /// デフォルトコンストラクタ（禁止）です。
        /// </summary>
        private EffectDelay() { }
        public EffectDelay(long Duration) => this.Duration = Duration;

        /// <summary>
        /// エフェクトを再生します。
        /// </summary>
        public void Play()
        {
            IsPlaying = true;
            Timer.Start();
        }
        /// <summary>
        /// エフェクトの再生を停止します。
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
    /// Lua関数のエフェクトです。
    /// </summary>
    public class EffectLuaFunc : IEffect
    {
        /// <summary>
        /// エフェクトから呼び出される関数のシグネチャ
        /// </summary>
        /// <param name="Effect">呼び出し元のEffectLuaFuncクラスを指定します。</param>
        [CSharpCallLua]
        public delegate void EffectLuaFunction(EffectLuaFunc Effect);
        /// <summary>
        /// Lua関数（Play時呼び出し）
        /// </summary>
        private readonly EffectLuaFunction LuaFuncPlay = null;
        /// <summary>
        /// Lua関数（Stop時呼び出し）
        /// </summary>
        private readonly EffectLuaFunction LuaFuncStop = null;
        /// <summary>
        /// Lua関数（Update時呼び出し）
        /// </summary>
        private readonly EffectLuaFunction LuaFuncUpdate = null;
        /// <summary>
        /// エフェクトが再生中か
        /// </summary>
        public bool IsPlaying { get; set; } = false;
        /// <summary>
        /// エフェクトが再生完了か
        /// </summary>
        public bool IsComplete { get; set; } = false;

        /// <summary>
        /// デフォルトコンストラクタ（禁止）です。
        /// </summary>
        private EffectLuaFunc() { }
        public EffectLuaFunc(string LuaFuncPlay, string LuaFuncStop, string LuaFuncUpdate)
        {
            if (!string.IsNullOrEmpty(LuaFuncPlay))
            {
                this.LuaFuncPlay = XLuaUtils.GetDelegateLuaFunction<EffectLuaFunction>(LuaFuncPlay);
            }
            if (!string.IsNullOrEmpty(LuaFuncStop))
            {
                this.LuaFuncStop = XLuaUtils.GetDelegateLuaFunction<EffectLuaFunction>(LuaFuncStop);
            }
            if (!string.IsNullOrEmpty(LuaFuncUpdate))
            {
                this.LuaFuncUpdate = XLuaUtils.GetDelegateLuaFunction<EffectLuaFunction>(LuaFuncUpdate);
            }
        }

        /// <summary>
        /// エフェクトを再生します。
        /// </summary>
        public void Play()
        {
            IsPlaying = true;
            LuaFuncPlay?.Invoke(this);
        }
        /// <summary>
        /// エフェクトの再生を停止します。
        /// </summary>
        public void Stop()
        {
            IsPlaying = false;
            LuaFuncStop?.Invoke(this);
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
            LuaFuncUpdate?.Invoke(this);
        }
    }

    /// <summary>
    /// 複数のエフェクトを管理するクラスです。
    /// </summary>
    public class Effects
    {
        /// <summary>
        /// 順序性のあるエフェクト
        /// </summary>
        private List<HashSet<IEffect>> OrderingEffects = new List<HashSet<IEffect>>();
        /// <summary>
        /// 順序性のあるエフェクトのインデックス
        /// </summary>
        private int OrderingEffectsIndex = 0;
        /// <summary>
        /// Insertされたエフェクト
        /// </summary>
        private Dictionary<long, HashSet<IEffect>> InsertedEffects = new Dictionary<long, HashSet<IEffect>>();
        /// <summary>
        /// 内部タイマー
        /// </summary>
        private Stopwatch Timer = new Stopwatch();
        /// <summary>
        /// エフェクトが再生中か
        /// </summary>
        public bool IsPlaying { get; private set; } = false;
        /// <summary>
        /// エフェクトが再生完了か
        /// </summary>
        public bool IsComplete { get; private set; } = false;

        /// <summary>
        /// 最後尾にエフェクトを追加します。
        /// エフェクト再生中は何もしません。
        /// </summary>
        /// <param name="Effect">追加するエフェクトを指定します。</param>
        /// <returns>自分自身のインスタンスを返します。</returns>
        public Effects Append(IEffect Effect)
        {
            if (!IsPlaying)
            {
                OrderingEffects.Add(new HashSet<IEffect>());
                OrderingEffects.Last().Add(Effect);
            }
            return this;
        }
        /// <summary>
        /// 最後尾と並列にエフェクトを追加します。
        /// エフェクト再生中は何もしません。
        /// </summary>
        /// <param name="Effect">追加するエフェクトを指定します。</param>
        /// <returns>自分自身のインスタンスを返します。</returns>
        public Effects Join(IEffect Effect)
        {
            if (!IsPlaying)
            {
                if (OrderingEffects.Count == 0)
                {
                    return Append(Effect);
                }
                else
                {
                    OrderingEffects.Last().Add(Effect);
                }
            }
            return this;
        }
        /// <summary>
        /// 先頭にエフェクトを追加します。
        /// エフェクト再生中は何もしません。
        /// </summary>
        /// <param name="Effect">追加するエフェクトを指定します。</param>
        /// <returns>自分自身のインスタンスを返します。</returns>
        public Effects Prepend(IEffect Effect)
        {
            if (!IsPlaying)
            {
                OrderingEffects.Insert(0, new HashSet<IEffect>());
                OrderingEffects.First().Add(Effect);
            }
            return this;
        }
        /// <summary>
        /// 指定した時間にエフェクトを追加します。
        /// エフェクト再生中は何もしません。
        /// </summary>
        /// <param name="Position">時間（ミリ秒）を指定します。</param>
        /// <param name="Effect">追加するエフェクトを指定します。</param>
        /// <returns>自分自身のインスタンスを返します。</returns>
        public Effects Insert(long Position, IEffect Effect)
        {
            if (!IsPlaying)
            {
                if (!InsertedEffects.ContainsKey(Position))
                {
                    InsertedEffects.Add(Position, new HashSet<IEffect>());
                }
                InsertedEffects[Position].Add(Effect);
            }
            return this;
        }

        /// <summary>
        /// エフェクトを再生します。
        /// </summary>
        public void Play()
        {
            Timer.Start();
            IsPlaying = true;
        }
        /// <summary>
        /// エフェクトの再生を停止します。
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
            //順序性のあるエフェクトを実行
            bool IsCompleteOrderingEffects = OrderingEffectsIndex >= OrderingEffects.Count;
            if (!IsCompleteOrderingEffects)
            {
                foreach (var Effect in OrderingEffects[OrderingEffectsIndex])
                {
                    if (!Effect.IsPlaying)
                    {
                        Effect.Play();
                    }
                    if (!Effect.IsComplete)
                    {
                        Effect.Update();
                    }
                }
                if (OrderingEffects[OrderingEffectsIndex].All(e => e.IsComplete))
                {
                    OrderingEffectsIndex += 1;
                    IsCompleteOrderingEffects = OrderingEffectsIndex >= OrderingEffects.Count;
                }
            }
            //Insertされたエフェクトを実行
            foreach (var Position in InsertedEffects.Keys.Where(e => e <= Timer.ElapsedMilliseconds))
            {
                foreach (var Effect in InsertedEffects[Position])
                {
                    if (!Effect.IsPlaying)
                    {
                        Effect.Play();
                    }
                    if (!Effect.IsComplete)
                    {
                        Effect.Update();
                    }
                }
            }
            bool IsCompleteInsertedEffects = InsertedEffects.All(e => e.Value.All(e2 => e2.IsComplete));
            IsComplete = IsCompleteOrderingEffects & IsCompleteInsertedEffects;
        }
    }

    /// <summary>
    /// エフェクトの管理を行うマネージャクラスです。
    /// </summary>
    public static class AbyssLibU_EffectManager
    {
        /// <summary>
        /// エフェクト
        /// </summary>
        private static HashSet<Effects> Effects = new HashSet<Effects>();

        /// <summary>
        /// エフェクトを追加します。
        /// </summary>
        /// <param name="Effect">エフェクトを指定します。</param>
        public static void Add(Effects Effect)
        {
            Effects.Add(Effect);
        }
        /// <summary>
        /// エフェクトを削除します。
        /// </summary>
        /// <param name="Effect">エフェクトを指定します。</param>
        public static void Remove(Effects Effect)
        {
            Effects.Remove(Effect);
        }

        /// <summary>
        /// フレーム毎処理です。
        /// </summary>
        public static void Update()
        {
            foreach (var Effect in Effects)
            {
                if (!Effect.IsPlaying)
                {
                    Effect.Play();
                }
                if (!Effect.IsComplete)
                {
                    Effect.Update();
                }
            }
            Effects.RemoveWhere(e => e.IsComplete);
        }
    }

}
