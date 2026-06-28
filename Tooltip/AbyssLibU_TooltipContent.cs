using System;
using UnityEngine;
using UnityEngine.UI;

namespace AbyssLibU
{
    /// <summary>
    /// ツールチップのコンテンツクラスです。
    /// シーン上の実体は常に1つであることを前提とします（2つ同時ホバーは非対応）
    /// </summary>
    public class AbyssLibU_TooltipContent : MonoBehaviour
    {
        /// <summary>
        /// 見切れを防ぐ範囲を表すRectTransform。
        /// CanvasのRectTransformを指定すると画面全体が範囲になります。
        /// この実体が常に収まるべき範囲のため、Anchor単位ではなくContent単位で1つだけ保持します。
        /// </summary>
        [SerializeField] private RectTransform ClampRectTransform;

        /// <summary>
        /// ツールチップコンテンツの描画を更新するコールバックです。
        /// 初期化タイミングで登録してください。
        /// </summary>
        public Action OnUpdateView;

        /// <summary>
        /// ツールチップの配置基準となるRectTransformを取得します。
        /// 既定では自身のRectTransformを返しますが、
        /// 別のRectTransformを基準にしたい場合は派生クラスでオーバーライドしてください。
        /// </summary>
        /// <returns>ツールチップの配置基準となるRectTransformを返します。</returns>
        protected virtual RectTransform GetRectTransform() => transform as RectTransform;

        /// <summary>
        /// GetRectTransform()が属するCanvasのワールドカメラを取得します。
        /// Screen Space - Overlayの場合や、Canvas配下にない場合はnullを返します。
        /// </summary>
        /// <returns>ワールドカメラを返します。</returns>
        protected virtual Camera GetCamera() => GetRectTransform().GetComponentInParent<Canvas>()?.rootCanvas.worldCamera;

        /// <summary>
        /// 指定したアンカーからの表示要求を処理します。
        /// ツールチップコンテンツの描画を更新・表示・位置計算までを一括で行います。
        /// </summary>
        /// <param name="Source">ツールチップのアンカーを指定します。</param>
        public void Show(AbyssLibU_TooltipAnchor Source)
        {
            // 1. ツールチップコンテンツの描画を更新
            OnUpdateView?.Invoke();
            // 2. 表示（サイズ確定のためSetActiveを先行）
            gameObject.SetActive(true);
            // 3. レイアウト強制更新（OnUpdateViewでRebuildしていない場合のフォールバック）
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetRectTransform());
            // 4. 位置計算（見切れ回避・Anchorへの隣接配置）
            RectTransform RT = GetRectTransform();
            RT.anchoredPosition = AbyssLibU_TooltipPositioner.Calculate(Source.GetRectTransform(), Source.GetCamera(), RT, GetCamera(), ClampRectTransform);
        }

        /// <summary>
        /// 指定したアンカーからの非表示要求を処理します。
        /// </summary>
        public void Hide() => gameObject.SetActive(false);
    }
}