using UnityEngine;
using DG.Tweening;

namespace AbyssLibU
{
    /// <summary>
    /// 辺の中点を表す列挙体です。
    /// </summary>
    public enum AbyssLibU_BezierArrowEdgePoint
    {
        /// <summary>
        /// 上辺
        /// </summary>
        Top = 0,
        /// <summary>
        /// 下辺
        /// </summary>
        Bottom = 1,
        /// <summary>
        /// 左辺
        /// </summary>
        Left = 2,
        /// <summary>
        /// 右辺
        /// </summary>
        Right = 3,
    }

    /// <summary>
    /// 二つのGameObject間にベジェ曲線の矢印を描画するコンポーネントです。
    /// </summary>
    public class AbyssLibU_BezierArrow : MonoBehaviour
    {
        /// <summary>
        /// 矢じりの大きさ
        /// </summary>
        [SerializeField] private float ArrowheadSize = 30f;
        /// <summary>
        /// デフォルトの線の色
        /// </summary>
        [SerializeField] private Color DefaultLineColor = Color.red;
        /// <summary>
        /// デフォルトの線の太さ
        /// </summary>
        [SerializeField] private float DefaultLineWidth = 5f;
        /// <summary>
        /// 矢じりのTransform
        /// </summary>
        [SerializeField] private Transform _Arrowhead;
        /// <summary>
        /// 矢じりのSprite（右向き（+X方向）が前提）
        /// </summary>
        [SerializeField] private Sprite ArrowheadSprite;
        /// <summary>
        /// ソフトエッジ用テクスチャ
        /// </summary>
        [SerializeField] private Texture2D SoftEdgeTexture;
        /// <summary>
        /// ベジェ曲線のセグメント数
        /// </summary>
        private const int SegmentCount = 32;
        /// <summary>
        /// LineRenderer
        /// </summary>
        private LineRenderer _LineRenderer;
        /// <summary>
        /// 開始地点のワールド座標
        /// </summary>
        private Vector3 _StartPosition;
        /// <summary>
        /// 終了地点のワールド座標
        /// </summary>
        private Vector3 _EndPosition;
        /// <summary>
        /// 制御点のワールド座標
        /// </summary>
        private Vector3 _ControlPoint;
        /// <summary>
        /// ベジェ曲線上の全ポイント（キャッシュ）
        /// </summary>
        private Vector3[] _CurvePoints;
        /// <summary>
        /// 描画範囲の開始進捗（0～1）
        /// </summary>
        private float _ProgressStart;
        /// <summary>
        /// 描画範囲の終了進捗（0～1）
        /// </summary>
        private float _ProgressEnd;

        /// <summary>
        /// ベジェ曲線の矢印を初期化します。
        /// </summary>
        /// <param name="Source">始点のGameObjectを指定します。</param>
        /// <param name="Destination">終点のGameObjectを指定します。</param>
        /// <param name="SourceEdge">始点の辺を指定します。</param>
        /// <param name="DestinationEdge">終点の辺を指定します。</param>
        /// <param name="Curvature">曲率を指定します。正で左凸、負で右凸です。</param>
        /// <param name="LineColor">線の色を指定します。未指定の場合は既存値を維持します。</param>
        /// <param name="Width">線の太さを指定します。未指定の場合は既存値を維持します。</param>
        public void Init(GameObject Source, GameObject Destination, AbyssLibU_BezierArrowEdgePoint SourceEdge, AbyssLibU_BezierArrowEdgePoint DestinationEdge, float Curvature, Color? LineColor = null, float Width = -1f)
        {
            EnsureLineRenderer();
            if (LineColor.HasValue)
            {
                _LineRenderer.startColor = LineColor.Value;
                _LineRenderer.endColor = LineColor.Value;
            }
            if (Width >= 0f)
            {
                _LineRenderer.startWidth = Width;
                _LineRenderer.endWidth = Width;
            }
            _StartPosition = GetEdgeMidpoint(Source, SourceEdge);
            _EndPosition = GetEdgeMidpoint(Destination, DestinationEdge);
            _ControlPoint = CalculateControlPoint(_StartPosition, _EndPosition, Curvature);
            BuildCurvePoints();
            CreateArrowhead();
            _ProgressStart = 0f;
            _ProgressEnd = 0f;
            ApplyProgress();
        }

        /// <summary>
        /// 矢印を即時表示します。
        /// </summary>
        public void Show()
        {
            _ProgressStart = 0f;
            _ProgressEnd = 1f;
            ApplyProgress();
        }

        /// <summary>
        /// 矢印を即時非表示にします。
        /// </summary>
        public void Hide()
        {
            _ProgressStart = 0f;
            _ProgressEnd = 0f;
            ApplyProgress();
        }

        /// <summary>
        /// 矢印の描画アニメーションのTweenを取得します。
        /// </summary>
        /// <param name="Duration">アニメーション時間（秒）を指定します。</param>
        /// <returns>矢印の描画アニメーションのTweenを返します。</returns>
        public Tween GetTweenDrawArrow(float Duration)
        {
            _ProgressStart = 0f;
            _ProgressEnd = 0f;
            ApplyProgress();
            Sequence Seq = DOTween.Sequence();
            Seq.Join(DOTween.To(
                () => _ProgressEnd,
                e => { _ProgressEnd = e; ApplyProgress(); },
                1f, Duration
            ).SetEase(Ease.OutQuad));
            return Seq;
        }

        /// <summary>
        /// 矢印の消去アニメーションのTweenを取得します。
        /// </summary>
        /// <param name="Duration">アニメーション時間（秒）を指定します。</param>
        /// <param name="FromTail">trueの場合は末尾から、falseの場合は先端から消去します。</param>
        /// <returns>矢印の消去アニメーションのTweenを返します。</returns>
        public Tween GetTweenEraseArrow(float Duration, bool FromTail = true)
        {
            Sequence Seq = DOTween.Sequence();
            if (FromTail)
            {
                Seq.Join(DOTween.To(
                    () => _ProgressStart,
                    e => { _ProgressStart = e; ApplyProgress(); },
                    1f, Duration
                ).SetEase(Ease.InQuad));
            }
            else
            {
                Seq.Join(DOTween.To(
                    () => _ProgressEnd,
                    e => { _ProgressEnd = e; ApplyProgress(); },
                    0f, Duration
                ).SetEase(Ease.InQuad));
            }
            return Seq;
        }

        /// <summary>
        /// LineRendererの存在を保証します。
        /// 既にアタッチされている場合はそのまま使用し、存在しない場合は追加してデフォルト設定を適用します。
        /// </summary>
        private void EnsureLineRenderer()
        {
            _LineRenderer = GetComponent<LineRenderer>();
            if (_LineRenderer == null)
            {
                _LineRenderer = gameObject.AddComponent<LineRenderer>();
            }
            if (SoftEdgeTexture != null)
            {
                Material Mat = new Material(Shader.Find("Sprites/Default"));
                Mat.mainTexture = SoftEdgeTexture;
                _LineRenderer.material = Mat;
            }
            else if (_LineRenderer.sharedMaterial == null)
            {
                _LineRenderer.material = CreateSoftEdgeMaterial();
            }
            _LineRenderer.numCornerVertices = 4;
            _LineRenderer.numCapVertices = 4;
            _LineRenderer.textureMode = LineTextureMode.Stretch;
            _LineRenderer.startColor = DefaultLineColor;
            _LineRenderer.endColor = DefaultLineColor;
            _LineRenderer.startWidth = DefaultLineWidth;
            _LineRenderer.endWidth = DefaultLineWidth;
            _LineRenderer.useWorldSpace = false;
        }
        /// <summary>
        /// GameObjectの辺の中点をワールド座標で取得します。
        /// </summary>
        /// <param name="Target">対象のGameObjectを指定します。</param>
        /// <param name="Edge">辺を指定します。</param>
        /// <returns>辺の中点のワールド座標を返します。</returns>
        private static Vector3 GetEdgeMidpoint(GameObject Target, AbyssLibU_BezierArrowEdgePoint Edge)
        {
            RectTransform RectTf = Target.GetComponent<RectTransform>();
            if (RectTf != null)
            {
                Rect R = RectTf.rect;
                Vector3 LocalPoint = Edge switch
                {
                    AbyssLibU_BezierArrowEdgePoint.Top => new Vector3(R.center.x, R.yMax, 0f),
                    AbyssLibU_BezierArrowEdgePoint.Bottom => new Vector3(R.center.x, R.yMin, 0f),
                    AbyssLibU_BezierArrowEdgePoint.Left => new Vector3(R.xMin, R.center.y, 0f),
                    AbyssLibU_BezierArrowEdgePoint.Right => new Vector3(R.xMax, R.center.y, 0f),
                    _ => new Vector3(R.center.x, R.center.y, 0f),
                };
                return RectTf.TransformPoint(LocalPoint);
            }
            Renderer Rend = Target.GetComponent<Renderer>();
            if (Rend != null)
            {
                Bounds B = Rend.bounds;
                return Edge switch
                {
                    AbyssLibU_BezierArrowEdgePoint.Top => B.center + new Vector3(0f, B.extents.y, 0f),
                    AbyssLibU_BezierArrowEdgePoint.Bottom => B.center - new Vector3(0f, B.extents.y, 0f),
                    AbyssLibU_BezierArrowEdgePoint.Left => B.center - new Vector3(B.extents.x, 0f, 0f),
                    AbyssLibU_BezierArrowEdgePoint.Right => B.center + new Vector3(B.extents.x, 0f, 0f),
                    _ => B.center,
                };
            }
            return Target.transform.position;
        }
        /// <summary>
        /// ベジェ曲線の制御点を算出します。
        /// </summary>
        /// <param name="Start">始点を指定します。</param>
        /// <param name="End">終点を指定します。</param>
        /// <param name="Curvature">曲率を指定します。</param>
        /// <returns>制御点のワールド座標を返します。</returns>
        private static Vector3 CalculateControlPoint(Vector3 Start, Vector3 End, float Curvature)
        {
            Vector3 Midpoint = (Start + End) * 0.5f;
            Vector3 Direction = End - Start;
            Vector3 Perpendicular = new Vector3(-Direction.y, Direction.x, 0f).normalized;
            Vector3 Offset = Perpendicular * Curvature * Direction.magnitude;
            return Midpoint + Offset;
        }
        /// <summary>
        /// ベジェ曲線上のポイントを事前計算します。
        /// </summary>
        private void BuildCurvePoints()
        {
            _CurvePoints = new Vector3[SegmentCount + 1];
            for (int I = 0; I <= SegmentCount; I++)
            {
                float T = (float)I / SegmentCount;
                _CurvePoints[I] = EvaluateBezier(T);
            }
        }
        /// <summary>
        /// 二次ベジェ曲線上の座標を算出します。
        /// </summary>
        /// <param name="T">パラメータ（0～1）を指定します。</param>
        /// <returns>曲線上の座標を返します。</returns>
        private Vector3 EvaluateBezier(float T)
        {
            float U = 1f - T;
            return U * U * _StartPosition + 2f * U * T * _ControlPoint + T * T * _EndPosition;
        }
        /// <summary>
        /// 二次ベジェ曲線の接線を算出します。
        /// </summary>
        /// <param name="T">パラメータ（0～1）を指定します。</param>
        /// <returns>接線ベクトルを返します。</returns>
        private Vector3 EvaluateBezierTangent(float T)
        {
            float U = 1f - T;
            return 2f * U * (_ControlPoint - _StartPosition) + 2f * T * (_EndPosition - _ControlPoint);
        }
        /// <summary>
        /// ワールド座標をローカル座標に変換します。
        /// </summary>
        /// <param name="WorldPosition">ワールド座標を指定します。</param>
        /// <returns>ローカル座標を返します。</returns>
        private Vector3 ToLocal(Vector3 WorldPosition) => transform.InverseTransformPoint(WorldPosition);
        /// <summary>
        /// 描画進捗を適用します。_ProgressStartから_ProgressEndの範囲を描画します。
        /// </summary>
        private void ApplyProgress()
        {
            float StartT = Mathf.Clamp01(_ProgressStart);
            float EndT = Mathf.Clamp01(_ProgressEnd);
            if (EndT <= StartT)
            {
                _LineRenderer.positionCount = 0;
                if (_Arrowhead != null)
                {
                    _Arrowhead.gameObject.SetActive(false);
                }
                return;
            }
            float ScaledStart = StartT * SegmentCount;
            float ScaledEnd = EndT * SegmentCount;
            int FirstCachedIndex = Mathf.CeilToInt(ScaledStart);
            int LastCachedIndex = Mathf.FloorToInt(ScaledEnd);
            FirstCachedIndex = Mathf.Clamp(FirstCachedIndex, 0, SegmentCount);
            LastCachedIndex = Mathf.Clamp(LastCachedIndex, 0, SegmentCount);
            bool HasStartInterp = ScaledStart < FirstCachedIndex;
            bool HasEndInterp = ScaledEnd > LastCachedIndex;
            int CachedCount = Mathf.Max(0, LastCachedIndex - FirstCachedIndex + 1);
            int TotalPoints = CachedCount + (HasStartInterp ? 1 : 0) + (HasEndInterp ? 1 : 0);
            if (TotalPoints < 2)
            {
                _LineRenderer.positionCount = 2;
                _LineRenderer.SetPosition(0, ToLocal(EvaluateBezier(StartT)));
                _LineRenderer.SetPosition(1, ToLocal(EvaluateBezier(EndT)));
            }
            else
            {
                _LineRenderer.positionCount = TotalPoints;
                int Idx = 0;
                if (HasStartInterp)
                {
                    _LineRenderer.SetPosition(Idx++, ToLocal(EvaluateBezier(StartT)));
                }
                for (int I = FirstCachedIndex; I <= LastCachedIndex; I++)
                {
                    _LineRenderer.SetPosition(Idx++, ToLocal(_CurvePoints[I]));
                }
                if (HasEndInterp)
                {
                    _LineRenderer.SetPosition(Idx++, ToLocal(EvaluateBezier(EndT)));
                }
            }
            if (_Arrowhead != null)
            {
                _Arrowhead.gameObject.SetActive(true);
                _Arrowhead.localPosition = ToLocal(EvaluateBezier(EndT));
                Vector3 Tangent = EvaluateBezierTangent(EndT);
                float Angle = Mathf.Atan2(Tangent.y, Tangent.x) * Mathf.Rad2Deg;
                _Arrowhead.localRotation = Quaternion.Euler(0f, 0f, Angle);
            }
        }
        /// <summary>
        /// 矢じりを生成します。
        /// </summary>
        private void CreateArrowhead()
        {
            SpriteRenderer SR = null;
            if (_Arrowhead == null)
            {
                GameObject ArrowheadObject = new GameObject("Arrowhead");
                ArrowheadObject.transform.SetParent(transform);
                _Arrowhead = ArrowheadObject.transform;
                SR = ArrowheadObject.AddComponent<SpriteRenderer>();
            }
            else
            {
                SR = _Arrowhead.GetComponent<SpriteRenderer>();
            }
            SR.sprite = ArrowheadSprite ?? CreateTriangleSprite();
            SR.color = _LineRenderer.startColor;
            SR.sortingLayerID = _LineRenderer.sortingLayerID;
            SR.sortingOrder = _LineRenderer.sortingOrder + 1;
            _Arrowhead.gameObject.layer = gameObject.layer;
            _Arrowhead.transform.localScale = Vector3.one * ArrowheadSize;
            _Arrowhead.gameObject.SetActive(false);
        }
        /// <summary>
        /// 右向き三角形のSpriteを動的に生成します。
        /// </summary>
        /// <returns>三角形のSpriteを返します。</returns>
        private static Sprite CreateTriangleSprite()
        {
            int Size = 64;
            Texture2D Tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            Color[] Pixels = new Color[Size * Size];
            float InvSqrt125 = 1f / Mathf.Sqrt(1.25f);
            for (int Y = 0; Y < Size; Y++)
            {
                for (int X = 0; X < Size; X++)
                {
                    float NX = (float)X / (Size - 1);
                    float NY = (float)Y / (Size - 1);
                    float DistTop = (1f - 0.5f * NX - NY) * InvSqrt125;
                    float DistBottom = (NY - 0.5f * NX) * InvSqrt125;
                    float DistLeft = NX;
                    float MinDist = Mathf.Min(DistTop, Mathf.Min(DistBottom, DistLeft));
                    float Alpha = Mathf.Clamp01(MinDist * (Size - 1));
                    Pixels[Y * Size + X] = new Color(1f, 1f, 1f, Alpha);
                }
            }
            Tex.SetPixels(Pixels);
            Tex.Apply();
            return Sprite.Create(Tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
        }
        /// <summary>
        /// ソフトエッジのマテリアルを生成します。
        /// </summary>
        /// <returns>ソフトエッジのマテリアルを返します。</returns>
        private static Material CreateSoftEdgeMaterial()
        {
            int Width = 1;
            int Height = 16;
            Texture2D Tex = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            Tex.wrapMode = TextureWrapMode.Clamp;
            Color[] Pixels = new Color[Width * Height];
            for (int Y = 0; Y < Height; Y++)
            {
                float T = (float)Y / (Height - 1);
                float Alpha = 1f - Mathf.Abs(T * 2f - 1f);
                Alpha = Mathf.Pow(Alpha, 0.5f);
                Pixels[Y] = new Color(1f, 1f, 1f, Alpha);
            }
            Tex.SetPixels(Pixels);
            Tex.Apply();
            Material Mat = new Material(Shader.Find("Sprites/Default"));
            Mat.mainTexture = Tex;
            return Mat;
        }
    }
}