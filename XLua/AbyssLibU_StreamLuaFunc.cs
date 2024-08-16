using XLua;

namespace AbyssLibU
{
    /// <summary>
    /// Lua関数のストリームです。
    /// </summary>
    public class StreamLuaFunc : IStream
    {
        /// <summary>
        /// ストリームから呼び出される関数のシグネチャ
        /// </summary>
        /// <param name="Stream">呼び出し元のStreamLuaFuncクラスを指定します。</param>
        [CSharpCallLua]
        public delegate void StreamLuaFunction(StreamLuaFunc Stream);
        /// <summary>
        /// Lua関数（Play時呼び出し）
        /// </summary>
        private readonly StreamLuaFunction LuaFuncPlay = null;
        /// <summary>
        /// Lua関数（Stop時呼び出し）
        /// </summary>
        private readonly StreamLuaFunction LuaFuncStop = null;
        /// <summary>
        /// Lua関数（Update時呼び出し）
        /// </summary>
        private readonly StreamLuaFunction LuaFuncUpdate = null;
        /// <summary>
        /// ストリームが再生中か
        /// </summary>
        public bool IsPlaying { get; set; } = false;
        /// <summary>
        /// ストリームが再生完了か
        /// </summary>
        public bool IsComplete { get; set; } = false;

        /// <summary>
        /// デフォルトコンストラクタ（禁止）です。
        /// </summary>
        private StreamLuaFunc() { }
        public StreamLuaFunc(string LuaFuncPlay, string LuaFuncStop, string LuaFuncUpdate)
        {
            if (!string.IsNullOrEmpty(LuaFuncPlay))
            {
                this.LuaFuncPlay = XLuaUtils.GetDelegateLuaFunction<StreamLuaFunction>(LuaFuncPlay);
            }
            if (!string.IsNullOrEmpty(LuaFuncStop))
            {
                this.LuaFuncStop = XLuaUtils.GetDelegateLuaFunction<StreamLuaFunction>(LuaFuncStop);
            }
            if (!string.IsNullOrEmpty(LuaFuncUpdate))
            {
                this.LuaFuncUpdate = XLuaUtils.GetDelegateLuaFunction<StreamLuaFunction>(LuaFuncUpdate);
            }
        }

        /// <summary>
        /// ストリームを再生します。
        /// </summary>
        public void Play()
        {
            IsPlaying = true;
            LuaFuncPlay?.Invoke(this);
        }
        /// <summary>
        /// ストリームの再生を停止します。
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
}