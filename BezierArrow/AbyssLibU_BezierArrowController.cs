using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using DG.Tweening;

namespace AbyssLibU
{
    /// <summary>
    /// 矢印タイプのデータ構造です。
    /// </summary>
    [Serializable]
    public class AbyssLibU_ArrowTypeData
    {
        /// <summary>
        /// 使用するプレハブID
        /// </summary>
        public int PrefabID = 0;
        /// <summary>
        /// 線の色（null=プレハブデフォルト）
        /// </summary>
        public Color? LineColor = null;
        /// <summary>
        /// 線の太さ（null=プレハブデフォルト）
        /// </summary>
        public float? LineWidth = null;
        /// <summary>
        /// 曲率（正で左凸、負で右凸）
        /// </summary>
        public float Curvature = 0.3f;
        /// <summary>
        /// 消去時に末尾から消すかどうか
        /// </summary>
        public bool FromTail = true;
    }

    /// <summary>
    /// 矢印インスタンスの内部管理情報です。
    /// </summary>
    internal class ArrowInstance
    {
        /// <summary>
        /// 矢印のGameObject
        /// </summary>
        public GameObject ArrowObject;
        /// <summary>
        /// BezierArrowコンポーネント
        /// </summary>
        public AbyssLibU_BezierArrow Arrow;
        /// <summary>
        /// 使用した矢印タイプID
        /// </summary>
        public int TypeID;
        /// <summary>
        /// 使用したプレハブID
        /// </summary>
        public int PrefabID;
    }

    /// <summary>
    /// BezierArrowの生成・管理・表示制御を一括で扱うコントローラークラスです。
    /// </summary>
    public class AbyssLibU_BezierArrowController : MonoBehaviour
    {
        /// <summary>
        /// インスペクタから登録するプレハブエントリ
        /// </summary>
        [Serializable]
        public class PrefabEntry
        {
            /// <summary>
            /// プレハブID
            /// </summary>
            public int ID;
            /// <summary>
            /// BezierArrowがアタッチされたプレハブ
            /// </summary>
            public GameObject Prefab;
        }
        /// <summary>
        /// インスペクタから登録するプレハブリスト
        /// </summary>
        [SerializeField] private List<PrefabEntry> _PrefabEntries = new List<PrefabEntry>();
        /// <summary>
        /// 矢印の親Transform（未指定の場合はこのオブジェクト）
        /// </summary>
        [SerializeField] private Transform _ArrowParent;
        /// <summary>
        /// 初期プールサイズ
        /// </summary>
        [SerializeField] private int _InitialPoolSize = 20;
        /// <summary>
        /// プレハブIDとプールのマッピング
        /// </summary>
        private Dictionary<int, ObjectPool<GameObject>> _Pools = new Dictionary<int, ObjectPool<GameObject>>();
        /// <summary>
        /// プレハブIDとプレハブのマッピング
        /// </summary>
        private Dictionary<int, GameObject> _Prefabs = new Dictionary<int, GameObject>();
        /// <summary>
        /// 矢印タイプIDとデータのマッピング
        /// </summary>
        private Dictionary<int, AbyssLibU_ArrowTypeData> _ArrowTypes = new Dictionary<int, AbyssLibU_ArrowTypeData>();
        /// <summary>
        /// セット名と矢印インスタンスリストのマッピング
        /// </summary>
        private Dictionary<string, List<ArrowInstance>> _ArrowSets = new Dictionary<string, List<ArrowInstance>>();

        /// <summary>
        /// 初期化処理を行います。
        /// </summary>
        private void Awake()
        {
            foreach (PrefabEntry Entry in _PrefabEntries)
            {
                if (Entry.Prefab != null && !_Prefabs.ContainsKey(Entry.ID))
                {
                    RegisterPrefabInternal(Entry.ID, Entry.Prefab);
                }
            }
        }

        /// <summary>
        /// プレハブを登録します。
        /// </summary>
        /// <param name="ID">プレハブIDを指定します。</param>
        /// <param name="Prefab">BezierArrowがアタッチされたプレハブを指定します。</param>
        public void RegisterPrefab(int ID, GameObject Prefab)
        {
            if (Prefab == null)
            {
                Debug.LogError($"[AbyssLibU_BezierArrowController] Prefab is null for ID: {ID}");
                return;
            }
            if (_Prefabs.ContainsKey(ID))
            {
                Debug.LogWarning($"[AbyssLibU_BezierArrowController] Prefab ID {ID} is already registered. Skipping.");
                return;
            }
            RegisterPrefabInternal(ID, Prefab);
        }

        /// <summary>
        /// 矢印タイプを登録します。
        /// </summary>
        /// <param name="ID">矢印タイプIDを指定します。</param>
        /// <param name="TypeData">矢印タイプデータを指定します。</param>
        public void RegisterArrowType(int ID, AbyssLibU_ArrowTypeData TypeData)
        {
            if (_ArrowTypes.ContainsKey(ID))
            {
                Debug.LogWarning($"[AbyssLibU_BezierArrowController] ArrowType ID {ID} is already registered. Skipping.");
                return;
            }
            _ArrowTypes.Add(ID, TypeData);
        }

        /// <summary>
        /// 矢印を追加します。追加時は非表示状態で保持されます。
        /// </summary>
        /// <param name="SetName">セット名を指定します。</param>
        /// <param name="TypeID">矢印タイプIDを指定します。</param>
        /// <param name="Source">始点のGameObjectを指定します。</param>
        /// <param name="Destination">終点のGameObjectを指定します。</param>
        /// <param name="SourceEdge">始点の辺を指定します。</param>
        /// <param name="DestinationEdge">終点の辺を指定します。</param>
        public void AddArrow(string SetName, int TypeID, GameObject Source, GameObject Destination, AbyssLibU_BezierArrowEdgePoint SourceEdge, AbyssLibU_BezierArrowEdgePoint DestinationEdge)
        {
            if (!_ArrowTypes.TryGetValue(TypeID, out AbyssLibU_ArrowTypeData TypeData))
            {
                Debug.LogError($"[AbyssLibU_BezierArrowController] ArrowType ID {TypeID} is not registered.");
                return;
            }
            int TargetPrefabID = TypeData.PrefabID;
            if (!_Pools.TryGetValue(TargetPrefabID, out ObjectPool<GameObject> Pool))
            {
                Debug.LogError($"[AbyssLibU_BezierArrowController] Prefab ID {TargetPrefabID} is not registered.");
                return;
            }
            GameObject ArrowObj = Pool.Get();
            ArrowObj.transform.SetParent(_ArrowParent != null ? _ArrowParent : transform);
            AbyssLibU_BezierArrow Arrow = ArrowObj.GetComponent<AbyssLibU_BezierArrow>();
            if (Arrow == null)
            {
                Debug.LogError($"[AbyssLibU_BezierArrowController] Prefab ID {TargetPrefabID} does not have BezierArrow component.");
                Pool.Release(ArrowObj);
                return;
            }
            Arrow.Init(Source, Destination, SourceEdge, DestinationEdge, TypeData.Curvature, TypeData.LineColor, TypeData.LineWidth ?? -1f);
            Arrow.Hide();
            ArrowInstance Instance = new ArrowInstance
            {
                ArrowObject = ArrowObj,
                Arrow = Arrow,
                TypeID = TypeID,
                PrefabID = TargetPrefabID,
            };
            if (!_ArrowSets.TryGetValue(SetName, out List<ArrowInstance> Instances))
            {
                Instances = new List<ArrowInstance>();
                _ArrowSets.Add(SetName, Instances);
            }
            Instances.Add(Instance);
        }

        /// <summary>
        /// 全セットの矢印を一括表示するTweenを取得します。
        /// </summary>
        /// <param name="Duration">アニメーション時間（秒）を指定します。</param>
        /// <returns>全セットの矢印を一括表示するTweenを返します。</returns>
        public Sequence GetTweenPlayAll(float Duration)
        {
            Sequence Seq = DOTween.Sequence();
            foreach (KeyValuePair<string, List<ArrowInstance>> KV in _ArrowSets)
            {
                Sequence SetSeq = PlaySet(KV.Value, Duration);
                Seq.Join(SetSeq);
            }
            return Seq;
        }

        /// <summary>
        /// 指定セット名の矢印を一括表示するTweenを取得します。
        /// </summary>
        /// <param name="SetName">セット名を指定します。</param>
        /// <param name="Duration">アニメーション時間（秒）を指定します。</param>
        /// <returns>指定セット名の矢印を一括表示するTweenを返します。</returns>
        public Sequence GetTweenPlay(string SetName, float Duration)
        {
            if (!_ArrowSets.TryGetValue(SetName, out List<ArrowInstance> Instances))
            {
                Debug.LogWarning($"[AbyssLibU_BezierArrowController] Set '{SetName}' does not exist.");
                return DOTween.Sequence();
            }
            return PlaySet(Instances, Duration);
        }

        /// <summary>
        /// 全セットの矢印を一括非表示にしてプールに返却します。
        /// </summary>
        /// <param name="Duration">アニメーション時間（秒）を指定します。</param>
        /// <returns>消去アニメーションのSequenceを返します。</returns>
        public Sequence GetTweenClearAll(float Duration)
        {
            Sequence Seq = DOTween.Sequence();
            List<string> SetNames = new List<string>(_ArrowSets.Keys);
            foreach (string SetName in SetNames)
            {
                Sequence SetSeq = ClearSet(SetName, Duration);
                Seq.Join(SetSeq);
            }
            return Seq;
        }

        /// <summary>
        /// 指定セット名の矢印を一括非表示にしてプールに返却するTweenを取得します。
        /// </summary>
        /// <param name="SetName">セット名を指定します。</param>
        /// <param name="Duration">アニメーション時間（秒）を指定します。</param>
        /// <returns>指定セット名の矢印を一括非表示にしてプールに返却するTweenを返します。セットが存在しない場合は空のSequenceを返します。</returns>
        public Sequence GetTweenClear(string SetName, float Duration)
        {
            if (!_ArrowSets.ContainsKey(SetName))
            {
                return DOTween.Sequence();
            }
            return ClearSet(SetName, Duration);
        }

        /// <summary>
        /// 全セットの矢印を即時非表示にしてプールに返却します（アニメーションなし）
        /// </summary>
        public void ClearAllImmediate()
        {
            List<string> SetNames = new List<string>(_ArrowSets.Keys);
            foreach (string SetName in SetNames)
            {
                ClearSetImmediate(SetName);
            }
        }

        /// <summary>
        /// 指定セット名の矢印を即時非表示にしてプールに返却します（アニメーションなし）
        /// セットが存在しない場合は何もしません。
        /// </summary>
        /// <param name="SetName">セット名を指定します。</param>
        public void ClearImmediate(string SetName)
        {
            if (!_ArrowSets.ContainsKey(SetName))
            {
                return;
            }
            ClearSetImmediate(SetName);
        }

        /// <summary>
        /// プレハブを内部的に登録します。
        /// </summary>
        /// <param name="ID">プレハブIDを指定します。</param>
        /// <param name="Prefab">プレハブを指定します。</param>
        private void RegisterPrefabInternal(int ID, GameObject Prefab)
        {
            _Prefabs.Add(ID, Prefab);
            ObjectPool<GameObject> Pool = new ObjectPool<GameObject>(
                createFunc: () => { GameObject o = Instantiate(Prefab); o.SetActive(false); return o; },
                actionOnGet: e => e.SetActive(true),
                actionOnRelease: e => e.SetActive(false),
                actionOnDestroy: e => Destroy(e),
                collectionCheck: false,
                defaultCapacity: _InitialPoolSize);
            _Pools.Add(ID, Pool);
        }
        /// <summary>
        /// 矢印インスタンスリストの矢印を表示するTweenを取得します。
        /// </summary>
        /// <param name="Instances">矢印インスタンスリストを指定します。</param>
        /// <param name="Duration">アニメーション時間（秒）を指定します。</param>
        /// <returns>矢印インスタンスリストの矢印を表示するTweenを返します。</returns>
        private Sequence PlaySet(List<ArrowInstance> Instances, float Duration)
        {
            Sequence Seq = DOTween.Sequence();
            foreach (ArrowInstance Instance in Instances)
            {
                Tween DrawTween = Instance.Arrow.GetTweenDrawArrow(Duration);
                Seq.Join(DrawTween);
            }
            return Seq;
        }
        /// <summary>
        /// セット名を指定して矢印を消去するTweenを取得します。
        /// </summary>
        /// <param name="SetName">セット名を指定します。</param>
        /// <param name="Duration">アニメーション時間（秒）を指定します。</param>
        /// <returns>矢印を消去するTweenを返します。</returns>
        private Sequence ClearSet(string SetName, float Duration)
        {
            if (!_ArrowSets.TryGetValue(SetName, out List<ArrowInstance> Instances))
            {
                return DOTween.Sequence();
            }
            Sequence Seq = DOTween.Sequence();
            foreach (ArrowInstance Instance in Instances)
            {
                if (_ArrowTypes.TryGetValue(Instance.TypeID, out AbyssLibU_ArrowTypeData TypeData))
                {
                    Tween EraseTween = Instance.Arrow.GetTweenEraseArrow(Duration, TypeData.FromTail);
                    Seq.Join(EraseTween);
                }
            }
            Seq.OnComplete(() =>
            {
                ReturnInstancesToPool(SetName);
            });
            return Seq;
        }
        /// <summary>
        /// セット名を指定して矢印を即時消去します。
        /// </summary>
        /// <param name="SetName">セット名を指定します。</param>
        private void ClearSetImmediate(string SetName)
        {
            if (!_ArrowSets.TryGetValue(SetName, out List<ArrowInstance> Instances))
            {
                return;
            }
            foreach (ArrowInstance Instance in Instances)
            {
                Instance.Arrow.Hide();
                _Pools[Instance.PrefabID].Release(Instance.ArrowObject);
            }
            Instances.Clear();
            _ArrowSets.Remove(SetName);
        }
        /// <summary>
        /// インスタンスをプールに返却します。
        /// </summary>
        /// <param name="SetName">セット名を指定します。</param>
        private void ReturnInstancesToPool(string SetName)
        {
            if (!_ArrowSets.TryGetValue(SetName, out List<ArrowInstance> Instances))
            {
                return;
            }
            foreach (ArrowInstance Instance in Instances)
            {
                _Pools[Instance.PrefabID].Release(Instance.ArrowObject);
            }
            Instances.Clear();
            _ArrowSets.Remove(SetName);
        }
    }
}