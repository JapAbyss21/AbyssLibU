using UnityEngine;

namespace AbyssLibU
{
    /// <summary>
    /// ツールチップの表示位置を計算する静的クラスです。
    /// 座標変換を含めてこのクラス内で完結させます（呼び出し側に座標系の知識を要求しません）。
    /// </summary>
    public static class AbyssLibU_TooltipPositioner
    {
        /// <summary>
        /// 基準RectTransformに隣接し、かつClampRectTransform内に収まる位置を計算します。
        /// 配置の優先順位は右→左→下→上です。いずれも収まらない場合はClampRect内にクランプします。
        /// </summary>
        /// <param name="AnchorRT">配置基準となるRectTransformを指定します。</param>
        /// <param name="AnchorCamera">AnchorRTを描画しているカメラを指定します（Screen Space - Overlayの場合はnull）。</param>
        /// <param name="TooltipRT">配置対象となるツールチップコンテンツのRectTransformを指定します。</param>
        /// <param name="TooltipCamera">TooltipRTが属するCanvasのワールドカメラを指定します（Screen Space - Overlayの場合はnull）。</param>
        /// <param name="ClampRectTransform">見切れを防ぐ範囲を表すRectTransformを指定します（CanvasのRectTransformを指定すると画面全体になります）。</param>
        /// <returns>TooltipRTの親のローカル座標系における、配置すべきanchoredPositionを返します。</returns>
        public static Vector2 Calculate(RectTransform AnchorRT, Camera AnchorCamera, RectTransform TooltipRT, Camera TooltipCamera, RectTransform ClampRectTransform)
        {
            RectTransform ParentRT = TooltipRT.parent as RectTransform;
            if (ParentRT == null)
            {
                return TooltipRT.anchoredPosition;
            }

            Vector3[] AnchorCorners = new Vector3[4];
            AnchorRT.GetWorldCorners(AnchorCorners);
            Rect AnchorLocalRect = WorldCornersToLocalRect(AnchorCorners, ParentRT, AnchorCamera, TooltipCamera);

            Vector3[] ClampCorners = new Vector3[4];
            ClampRectTransform.GetWorldCorners(ClampCorners);
            Rect ClampLocalRect = WorldCornersToLocalRect(ClampCorners, ParentRT, TooltipCamera, TooltipCamera);

            return CalcPosition(AnchorLocalRect, TooltipRT.rect.size, ClampLocalRect, TooltipRT.pivot);
        }

        /// <summary>
        /// ワールド空間の頂点配列を、指定したRectTransformのローカル座標系のRectへ変換します。
        /// </summary>
        /// <param name="WorldCorners">変換元のワールド空間頂点配列を指定します。</param>
        /// <param name="ParentRT">変換先のRectTransformを指定します。</param>
        /// <param name="WorldCamera">WorldCorners側を描画しているカメラを指定します（Screen Space - Overlayの場合はnull）。</param>
        /// <param name="UICamera">ParentRT側のCanvasのワールドカメラを指定します（Screen Space - Overlayの場合はnull）。</param>
        /// <returns>ParentRTのローカル座標系におけるRectを返します。変換に失敗した場合はRect.zeroを返します。</returns>
        private static Rect WorldCornersToLocalRect(Vector3[] WorldCorners, RectTransform ParentRT, Camera WorldCamera, Camera UICamera)
        {
            float LocalXMin = float.MaxValue;
            float LocalXMax = float.MinValue;
            float LocalYMin = float.MaxValue;
            float LocalYMax = float.MinValue;
            foreach (Vector3 Corner in WorldCorners)
            {
                Vector2 ScreenPoint = RectTransformUtility.WorldToScreenPoint(WorldCamera, Corner);
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(ParentRT, ScreenPoint, UICamera, out Vector2 LocalPoint))
                {
                    continue;
                }
                if (LocalPoint.x < LocalXMin) LocalXMin = LocalPoint.x;
                if (LocalPoint.x > LocalXMax) LocalXMax = LocalPoint.x;
                if (LocalPoint.y < LocalYMin) LocalYMin = LocalPoint.y;
                if (LocalPoint.y > LocalYMax) LocalYMax = LocalPoint.y;
            }
            if (LocalXMin == float.MaxValue)
            {
                Debug.LogWarning("[AbyssLibU] TooltipPositioner: RectTransformの座標変換に失敗しました。Canvasの設定を確認してください。");
                return Rect.zero;
            }
            return Rect.MinMaxRect(LocalXMin, LocalYMin, LocalXMax, LocalYMax);
        }

        /// <summary>
        /// ツールチップの配置位置を計算します。右→左→下→上の優先順位で配置を試みます。
        /// </summary>
        /// <param name="AnchorLocalRect">基準矩形（ParentRTのローカル座標系）を指定します。</param>
        /// <param name="TooltipSize">ツールチップのサイズを指定します。</param>
        /// <param name="ClampRect">配置可能な矩形（ParentRTのローカル座標系）を指定します。</param>
        /// <param name="Pivot">ツールチップのpivotを指定します。</param>
        /// <returns>ツールチップのanchoredPositionを返します。</returns>
        private static Vector2 CalcPosition(Rect AnchorLocalRect, Vector2 TooltipSize, Rect ClampRect, Vector2 Pivot)
        {
            float W = TooltipSize.x;
            float H = TooltipSize.y;

            // anchoredPosition = pivot基準。各辺の位置からpivotオフセットで変換する。
            // 右: コンテンツ左端 = AnchorLocalRect.xMax
            if (AnchorLocalRect.xMax + W <= ClampRect.xMax && H <= ClampRect.height)
            {
                float X = AnchorLocalRect.xMax + Pivot.x * W;
                float Y = Mathf.Clamp(AnchorLocalRect.center.y + (Pivot.y - 0.5f) * H, ClampRect.yMin + Pivot.y * H, ClampRect.yMax - (1f - Pivot.y) * H);
                return new Vector2(X, Y);
            }
            // 左: コンテンツ右端 = AnchorLocalRect.xMin
            if (AnchorLocalRect.xMin - W >= ClampRect.xMin && H <= ClampRect.height)
            {
                float X = AnchorLocalRect.xMin - (1f - Pivot.x) * W;
                float Y = Mathf.Clamp(AnchorLocalRect.center.y + (Pivot.y - 0.5f) * H, ClampRect.yMin + Pivot.y * H, ClampRect.yMax - (1f - Pivot.y) * H);
                return new Vector2(X, Y);
            }
            // 下: コンテンツ上端 = AnchorLocalRect.yMin
            if (AnchorLocalRect.yMin - H >= ClampRect.yMin && W <= ClampRect.width)
            {
                float X = Mathf.Clamp(AnchorLocalRect.center.x + (Pivot.x - 0.5f) * W, ClampRect.xMin + Pivot.x * W, ClampRect.xMax - (1f - Pivot.x) * W);
                float Y = AnchorLocalRect.yMin - (1f - Pivot.y) * H;
                return new Vector2(X, Y);
            }
            // 上: コンテンツ下端 = AnchorLocalRect.yMax
            if (AnchorLocalRect.yMax + H <= ClampRect.yMax && W <= ClampRect.width)
            {
                float X = Mathf.Clamp(AnchorLocalRect.center.x + (Pivot.x - 0.5f) * W, ClampRect.xMin + Pivot.x * W, ClampRect.xMax - (1f - Pivot.x) * W);
                float Y = AnchorLocalRect.yMax + Pivot.y * H;
                return new Vector2(X, Y);
            }
            // フォールバック：ClampRect内にクランプ（被りは許容）
            return new Vector2(
                Mathf.Clamp(AnchorLocalRect.center.x + (Pivot.x - 0.5f) * W, ClampRect.xMin + Pivot.x * W, ClampRect.xMax - (1f - Pivot.x) * W),
                Mathf.Clamp(AnchorLocalRect.center.y + (Pivot.y - 0.5f) * H, ClampRect.yMin + Pivot.y * H, ClampRect.yMax - (1f - Pivot.y) * H)
            );
        }
    }
}