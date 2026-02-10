using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace AbyssLibU
{
    /// <summary>
    /// ファイル入出力の管理を行うシングルトンマネージャです。
    /// </summary>
    public sealed class AbyssLibU_FileManager : MonoBehaviour
    {
        /// <summary>
        /// シングルトンオブジェクト
        /// </summary>
        private static AbyssLibU_FileManager _instance;
        public static AbyssLibU_FileManager Instance
        {
            get
            {
                if (_instance is null)
                {
                    _instance = (AbyssLibU_FileManager)FindAnyObjectByType(typeof(AbyssLibU_FileManager));
                    if (_instance is null)
                    {
                        throw new MissingComponentException(typeof(AbyssLibU_FileManager) + "is nothing");
                    }
                }
                return _instance;
            }
        }
        /// <summary>
        /// ブロック長（デフォルト値）
        /// </summary>
        private const int DEFAULT_BLOCK_SIZE = 128;
        /// <summary>
        /// 鍵長（デフォルト値）
        /// </summary>
        private const int DEFAULT_KEY_SIZE = 128;
        /// <summary>
        /// パスワード（IV）
        /// </summary>
        [SerializeField] private string IVPassword = default;
        /// <summary>
        /// パスワード（Key）
        /// </summary>
        [SerializeField] private string KeyPassword = default;
        /// <summary>
        /// AES暗号化サービスプロバイダ
        /// </summary>
        private AesCryptoServiceProvider CPS;

        /// <summary>
        /// フィールドのみを対象とするContractResolver（プロパティは対象外とする）
        /// </summary>
        private class IgnorePropertiesResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                return base.CreateProperties(type, memberSerialization)
                    .Where(p => IsField(p))
                    .ToList();
            }
            private static bool IsField(JsonProperty prop)
            {
                var memberInfo = prop.DeclaringType?.GetMember(prop.UnderlyingName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                    .FirstOrDefault();
                return memberInfo is FieldInfo;
            }
        }
        /// <summary>
        /// フィールドのみを対象とするJson設定（プロパティは対象外とする）
        /// また、型情報を出力する。
        /// </summary>
        public static readonly JsonSerializerSettings SaveSettings_IgnoreProperties = new()
        {
            ContractResolver = new IgnorePropertiesResolver(),
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented
        };

        /// <summary>
        /// 初期化処理です。
        /// パスワードが未入力の場合、自動生成されます。
        /// </summary>
        private void Awake()
        {
            CPS = new AesCryptoServiceProvider
            {
                BlockSize = DEFAULT_BLOCK_SIZE,
                KeySize = DEFAULT_KEY_SIZE,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                IV = string.IsNullOrEmpty(IVPassword) ? new Rfc2898DeriveBytes(IVPassword ?? "", DEFAULT_BLOCK_SIZE / 8).GetBytes(DEFAULT_BLOCK_SIZE / 8) : Convert.FromBase64String(IVPassword),
                Key = string.IsNullOrEmpty(KeyPassword) ? new Rfc2898DeriveBytes(KeyPassword ?? "", DEFAULT_KEY_SIZE / 8).GetBytes(DEFAULT_KEY_SIZE / 8) : Convert.FromBase64String(KeyPassword)
            };
            if (string.IsNullOrEmpty(IVPassword))
            {
                IVPassword = Convert.ToBase64String(CPS.IV);
            }
            if (string.IsNullOrEmpty(KeyPassword))
            {
                KeyPassword = Convert.ToBase64String(CPS.Key);
            }
        }

        //=================================================================================
        //データの書き込み
        //=================================================================================
        /// <summary>
        /// テキストデータを保存します。
        /// </summary>
        /// <param name="path">保存ファイルのパスを指定します。</param>
        /// <param name="text">保存するテキストデータを指定します。</param>
        /// <param name="Encrypt">暗号化するかを指定します（デフォルト：しない）</param>
        public void SaveText(string path, string text, bool Encrypt = false)
        {
            if (Encrypt)
            {
                using (ICryptoTransform encrypt = CPS.CreateEncryptor())
                {
                    byte[] src = Encoding.Unicode.GetBytes(text);
                    text = Convert.ToBase64String(encrypt.TransformFinalBlock(src, 0, src.Length));
                }
            }
            File.WriteAllText(Application.dataPath + "/" + path, text);
        }
        /// <summary>
        /// オブジェクトデータを保存します（Newtonsoft.Json使用）
        /// シリアライズ可能なオブジェクトである必要があります。
        /// </summary>
        /// <param name="path">保存ファイルのパスを指定します。</param>
        /// <param name="obj">保存するオブジェクトデータを指定します。</param>
        /// <param name="Encrypt">暗号化するかを指定します（デフォルト：しない）</param>
        public void SaveObject(string path, object obj, bool Encrypt = false)
        {
            string Json = JsonConvert.SerializeObject(obj, SaveSettings_IgnoreProperties);
            SaveText(path, Json, Encrypt);
        }

        //=================================================================================
        //データの読み込み
        //=================================================================================
        /// <summary>
        /// テキストデータを読み込みます。
        /// </summary>
        /// <param name="path">読み込むファイルのパスを指定します。</param>
        /// <param name="text">読み込み先のstring型を指定します。</param>
        /// <param name="Decrypt">復号化するかを指定します（デフォルト：しない）</param>
        public void LoadText(string path, out string text, bool Decrypt = false)
        {
            // TextAssetとして、Resourcesフォルダからスクリプトファイルをロードする
            TextAsset ta = Resources.Load(path) as TextAsset;
            if (ta is not null)
            {
                text = ta.text;
            }
            // テキストデータとして、外部データからスクリプトファイルをロードする
            else
            {
                text = File.ReadAllText(Application.dataPath + "/" + path);
            }
            if (Decrypt)
            {
                using (ICryptoTransform decrypt = CPS.CreateDecryptor())
                {
                    byte[] src = Convert.FromBase64String(text);
                    text = Encoding.Unicode.GetString(decrypt.TransformFinalBlock(src, 0, src.Length));
                }
            }
        }
        /// <summary>
        /// オブジェクトデータを読み込みます（Newtonsoft.Json使用）
        /// シリアライズ可能なオブジェクトである必要があります。
        /// </summary>
        /// <param name="path">読み込むファイルのパスを指定します。</param>
        /// <param name="obj">読み込み先のobject型を指定します。</param>
        /// <param name="Decrypt">復号化するかを指定します（デフォルト：しない）</param>
        public void LoadObject<T>(string path, out T obj, bool Decrypt = false)
        {
            LoadText(path, out string text, Decrypt);
            obj = JsonConvert.DeserializeObject<T>(text, SaveSettings_IgnoreProperties);
            if (obj is AbyssLibU_IExternalRefRoot Root)
            {
                Root.SetExternalRef();
            }
        }
    }
}