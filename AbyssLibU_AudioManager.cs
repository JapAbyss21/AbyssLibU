using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace AbyssLibU
{

    /// <summary>
    /// ＢＧＭおよびＳＥの管理を行うシングルトンマネージャです。
    /// </summary>
    public sealed class AbyssLibU_AudioManager : MonoBehaviour
    {
        /// <summary>
        /// シングルトンオブジェクト
        /// </summary>
        private static AbyssLibU_AudioManager _instance;
        public static AbyssLibU_AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (AbyssLibU_AudioManager)FindAnyObjectByType(typeof(AbyssLibU_AudioManager));
                    if (_instance == null)
                    {
                        throw new MissingComponentException(typeof(AbyssLibU_AudioManager) + "is nothing");
                    }
                }
                return _instance;
            }
        }
        //音量のデフォルト値
        private const float BGM_VOLUME_DEFULT = 1.0f;
        private const float SE_VOLUME_DEFULT = 1.0f;
        //音量
        private float _BGMVolume = BGM_VOLUME_DEFULT;
        private float _SEVolume = SE_VOLUME_DEFULT;
        //イントロ/ループ再生の遅延
        private const float DELAY_INTROLOOP = 0.1f;
        //BGMがフェードイン/アウト中か？
        private bool _isFadeIn = false;
        private bool _isFadeOut = false;
        //BGMのフェードにかかる時間
        public const float BGM_FADE_SPEED_LOW = 0.3f;
        public const float BGM_FADE_SPEED_HIGH = 0.9f;
        private float _bgmFadeSpeed = BGM_FADE_SPEED_LOW;
        //AudioSource
        private AudioSource[,] _bgmSource;
        private int _bgmSourceIterator = 0;
        private List<AudioSource> _seSourceList;
        private const int DEFAULT_SE_SOURCE_NUM = 10;
        //AudioClip
        private Dictionary<string, AudioClip> _AudioClips;
        /// <summary>
        /// 初期化処理です。
        /// AudioSourceを作成します。
        /// </summary>
        private void Awake()
        {
            //シングルトン
            if (this != Instance)
            {
                Destroy(this);
                return;
            }
            DontDestroyOnLoad(gameObject);
            //AudioSourceを作成
            for (int i = 0; i < DEFAULT_SE_SOURCE_NUM + 4; i++)
            {
                gameObject.AddComponent<AudioSource>();
            }
            AudioSource[] audioSources = GetComponents<AudioSource>();
            _bgmSource = new AudioSource[2, 2];
            _seSourceList = new List<AudioSource>();
            for (int i = 0; i < audioSources.Length; i++)
            {
                audioSources[i].playOnAwake = false;
                switch (i)
                {
                    case 0:
                        _bgmSource[0, 0] = audioSources[i];
                        _bgmSource[0, 0].volume = BGM_VOLUME_DEFULT;
                        break;
                    case 1:
                        audioSources[i].loop = true;
                        _bgmSource[0, 1] = audioSources[i];
                        _bgmSource[0, 1].volume = BGM_VOLUME_DEFULT;
                        break;
                    case 2:
                        _bgmSource[1, 0] = audioSources[i];
                        _bgmSource[1, 0].volume = BGM_VOLUME_DEFULT;
                        break;
                    case 3:
                        audioSources[i].loop = true;
                        _bgmSource[1, 1] = audioSources[i];
                        _bgmSource[1, 1].volume = BGM_VOLUME_DEFULT;
                        break;
                    default:
                        _seSourceList.Add(audioSources[i]);
                        audioSources[i].volume = SE_VOLUME_DEFULT;
                        break;
                }
            }
            _AudioClips = new Dictionary<string, AudioClip>();
        }
        /// <summary>
        /// フレーム毎処理です。
        /// フェード処理を行います。
        /// </summary>
        private void Update()
        {
            float volume = _BGMVolume;
            //フェードイン
            if (_isFadeIn)
            {
                _bgmSource[_bgmSourceIterator, 0].volume += Time.deltaTime * _bgmFadeSpeed * volume;
                _bgmSource[_bgmSourceIterator, 1].volume += Time.deltaTime * _bgmFadeSpeed * volume;
                if (_bgmSource[_bgmSourceIterator, 1].volume >= volume)
                {
                    _bgmSource[_bgmSourceIterator, 0].volume = volume;
                    _bgmSource[_bgmSourceIterator, 1].volume = volume;
                    _isFadeIn = false;
                }
            }
            //フェードアウト
            if (_isFadeOut)
            {
                _bgmSource[1 - _bgmSourceIterator, 0].volume -= Time.deltaTime * _bgmFadeSpeed * volume;
                _bgmSource[1 - _bgmSourceIterator, 1].volume -= Time.deltaTime * _bgmFadeSpeed * volume;
                if (_bgmSource[1 - _bgmSourceIterator, 1].volume <= 0)
                {
                    _bgmSource[1 - _bgmSourceIterator, 0].Stop();
                    _bgmSource[1 - _bgmSourceIterator, 1].Stop();
                    _bgmSource[1 - _bgmSourceIterator, 0].volume = volume;
                    _bgmSource[1 - _bgmSourceIterator, 1].volume = volume;
                    _isFadeOut = false;
                }
            }
        }
        //=================================================================================
        //サウンドデータの読み込み
        //=================================================================================
        /// <summary>
        /// Resourcesフォルダからサウンドデータを読み込みます。
        /// </summary>
        /// <param name="path">サウンドデータのパスを指定します。</param>
        public void LoadResources(string path)
        {
            if (!_AudioClips.ContainsKey(path))
            {
                AudioClip result = Resources.Load<AudioClip>(path);
                if (result == null)
                {
                    throw new FileNotFoundException("Could not find resource \"" + path + "\"");
                }
                result.name = path;
                _AudioClips[path] = result;
            }
        }
        /// <summary>
        /// 外部データからサウンドデータを読み込みます。
        /// </summary>
        /// <param name="path">サウンドデータのパスを指定します。</param>
        public void LoadExternalData(string path)
        {
            if (_AudioClips.ContainsKey(path))
            {
                return;
            }
            Uri URIPath = new Uri(Application.dataPath + "/" + path);
            if (URIPath.IsFile)
            {
                AudioType audioType;
                switch (path.Substring(path.Length - 3, 3).ToLower())
                {
                    case "mp3":
                        audioType = AudioType.MPEG;
                        break;
                    case "ogg":
                        audioType = AudioType.OGGVORBIS;
                        break;
                    case "wav":
                        audioType = AudioType.WAV;
                        break;
                    default:
                        throw new UriFormatException("\"" + path + "\" is not mp3/ogg/wav");
                }
                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(URIPath.AbsoluteUri, audioType))
                {
                    www.SendWebRequest();
                    while (!www.isDone) { }
                    if (www.result == UnityWebRequest.Result.ProtocolError || www.result == UnityWebRequest.Result.ConnectionError)
                    {
                        throw new FileNotFoundException("Could not find file \"" + path + "\"");
                    }
                    AudioClip result = DownloadHandlerAudioClip.GetContent(www);
                    result.name = path;
                    _AudioClips[path] = result;
                }
            }
            else
            {
                throw new UriFormatException("\"" + path + "\" is not a file URI");
            }
        }
        /// <summary>
        /// サウンドデータを読み込みます。
        /// Resourcesフォルダ⇒外部データの順に読み込みを試みます。
        /// </summary>
        /// <param name="path">サウンドデータのパスを指定します。</param>
        public void Load(string path)
        {
            try
            {
                LoadResources(path);
            }
            catch (FileNotFoundException)
            {
                LoadExternalData(path);
            }
        }
        /// <summary>
        /// 全サウンドデータを解放します。
        /// BGM/SE再生中に呼び出された場合は何もしません。
        /// </summary>
        public void UnloadAllData()
        {
            if (!IsPlayingBGM() && !IsPlayingSE())
            {
                foreach (string key in _AudioClips.Keys)
                {
                    _AudioClips[key].UnloadAudioData();
                }
                _AudioClips.Clear();
            }
        }
        //=================================================================================
        //BGM
        //=================================================================================
        /// <summary>
        /// BGMのボリュームを設定します。
        /// ボリュームの設定は即座に反映されます。
        /// </summary>
        /// <param name="volume">BGMのボリュームを指定します。</param>
        public void SetBGMVolume(float volume)
        {
            float oldVolume = _BGMVolume;
            _BGMVolume = volume;
            foreach (AudioSource bgmSource in _bgmSource)
            {
                bgmSource.volume *= (oldVolume == 0 ? volume : (volume / oldVolume));
            }
        }
        /// <summary>
        /// BGMのボリュームを取得します。
        /// </summary>
        /// <returns>BGMのボリュームを返します。</returns>
        public float GetBGMVolume() => _BGMVolume;
        /// <summary>
        /// BGMを即座に停止します。
        /// </summary>
        public void StopBGM()
        {
            foreach (AudioSource bgmSource in _bgmSource)
            {
                bgmSource.Stop();
                bgmSource.volume = _BGMVolume;
            }
            _isFadeIn = false;
            _isFadeOut = false;
        }
        /// <summary>
        /// BGMをフェード停止します。
        /// </summary>
        /// <param name="fadeSpeed">フェード速度を指定します。</param>
        public void FadeOutBGM(float fadeSpeed = BGM_FADE_SPEED_LOW)
        {
            if (IsPlayingBGM())
            {
                _isFadeOut = true;
                _bgmFadeSpeed = fadeSpeed;
                _bgmSourceIterator = 1 - _bgmSourceIterator;
            }
        }
        /// <summary>
        /// BGMを再生します。
        /// イントロ/ループ分割に対応しています。
        /// 前BGMの停止およびサウンドデータの読み込みを自動で行います。
        /// </summary>
        /// <param name="loop">ループ部分を指定します。</param>
        /// <param name="intro">イントロ部分を指定します。</param>
        public void PlayBGM(string loop, string intro = "")
        {
            if (IsPlayingBGM() && (_bgmSource[_bgmSourceIterator, 1].clip != null ? _bgmSource[_bgmSourceIterator, 1].clip.name : "") == loop)
            {
                //BGM再生中に同BGMを再生した場合は何もしない
                //同じBGMかどうかはループ部分だけ見て判定（問題ないよね？）
                return;
            }
            //前BGMの停止
            _bgmSource[_bgmSourceIterator, 0].Stop();
            _bgmSource[_bgmSourceIterator, 1].Stop();
            //読み込み
            Load(loop);
            AudioSource LoopSource = _bgmSource[_bgmSourceIterator, 1];
            LoopSource.clip = _AudioClips[loop];
            LoopSource.volume = _BGMVolume;
            if (string.IsNullOrEmpty(intro))
            {
                LoopSource.Play();
            }
            else
            {
                Load(intro);
                AudioSource IntroSource = _bgmSource[_bgmSourceIterator, 0];
                IntroSource.clip = _AudioClips[intro];
                IntroSource.volume = _BGMVolume;
                IntroSource.PlayScheduled(AudioSettings.dspTime + DELAY_INTROLOOP);
                LoopSource.PlayScheduled(AudioSettings.dspTime + DELAY_INTROLOOP + IntroSource.clip.length);
            }
        }
        /// <summary>
        /// BGMをフェード再生します。
        /// イントロ/ループ分割に対応しています。
        /// 前BGMのフェード停止およびサウンドデータの読み込みを自動で行います。
        /// </summary>
        /// <param name="loop">ループ部分を指定します。</param>
        /// <param name="fadeSpeed">フェード速度を指定します。</param>
        /// <param name="intro">イントロ部分を指定します。</param>
        public void FadeBGM(string loop, float fadeSpeed, string intro = "")
        {
            if (IsPlayingBGM() && (_bgmSource[_bgmSourceIterator, 1].clip != null ? _bgmSource[_bgmSourceIterator, 1].clip.name : "") == loop)
            {
                //BGM再生中に同BGMをフェード再生した場合は何もしない
                //同じBGMかどうかはループ部分だけ見て判定（問題ないよね？）
                return;
            }
            //前BGMをフェード停止
            FadeOutBGM(fadeSpeed);
            //読み込み
            Load(loop);
            AudioSource LoopSource = _bgmSource[_bgmSourceIterator, 1];
            LoopSource.clip = _AudioClips[loop];
            LoopSource.volume = 0.0f;
            if (string.IsNullOrEmpty(intro))
            {
                LoopSource.Play();
            }
            else
            {
                Load(intro);
                AudioSource IntroSource = _bgmSource[_bgmSourceIterator, 0];
                IntroSource.clip = _AudioClips[intro];
                IntroSource.volume = 0.0f;
                IntroSource.PlayScheduled(AudioSettings.dspTime + DELAY_INTROLOOP);
                LoopSource.PlayScheduled(AudioSettings.dspTime + DELAY_INTROLOOP + IntroSource.clip.length);
            }
            _isFadeIn = true;
            _bgmFadeSpeed = fadeSpeed;
        }
        /// <summary>
        /// BGMが再生中かを返します。
        /// </summary>
        /// <returns>BGMが再生中の場合はtrueを、再生中でない場合はfalseを返します。</returns>
        public bool IsPlayingBGM() => _bgmSource.Cast<AudioSource>().Any((e) => e.isPlaying);
        //=================================================================================
        //SE
        //=================================================================================
        /// <summary>
        /// SEのボリュームを設定します。
        /// ボリュームの設定は即座に反映されます。
        /// </summary>
        /// <param name="volume">SEのボリュームを指定します。</param>
        public void SetSEVolume(float volume)
        {
            float oldVolume = _SEVolume;
            _SEVolume = volume;
            foreach (AudioSource seSource in _seSourceList)
            {
                seSource.volume *= (oldVolume == 0 ? volume : (volume / oldVolume));
            }
        }
        /// <summary>
        /// SEのボリュームを取得します。
        /// </summary>
        /// <returns>SEのボリュームを返します。</returns>
        public float GetSEVolume() => _SEVolume;
        /// <summary>
        /// SEを即座に停止します。
        /// </summary>
        public void StopSE()
        {
            foreach (AudioSource seSource in _seSourceList)
            {
                seSource.Stop();
                seSource.volume = _SEVolume;
            }
        }
        /// <summary>
        /// SEを再生します。
        /// サウンドデータの読み込みを自動で行います。
        /// </summary>
        /// <param name="path">サウンドデータのパスを指定します。</param>
        /// <param name="volume">SEのボリュームを指定します。ボリュームは乗算で設定されます。</param>
        public void PlaySE(string path, float volume = 1.0f)
        {
            //読み込み
            Load(path);
            foreach (AudioSource seSource in _seSourceList)
            {
                if (!seSource.isPlaying)
                {
                    //未使用のAudioSourceで再生する
                    seSource.clip = _AudioClips[path];
                    seSource.volume = _SEVolume * volume;
                    seSource.Play();
                    return;
                }
            }
            //未使用のAudioSourceがなければ新規作成・追加
            AudioSource result = gameObject.AddComponent<AudioSource>();
            result.playOnAwake = false;
            result.clip = _AudioClips[path];
            result.volume = _SEVolume * volume;
            result.Play();
            _seSourceList.Add(result);
        }
        /// <summary>
        /// SEが再生中かを返します。
        /// </summary>
        /// <returns>SEが再生中の場合はtrueを、再生中でない場合はfalseを返します。</returns>
        public bool IsPlayingSE() => _seSourceList.Any((e) => e.isPlaying);
    }

}
