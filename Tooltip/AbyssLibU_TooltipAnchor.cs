using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AbyssLibU
{
    /// <summary>
    /// ツールチップのアンカークラスです。
    /// </summary>
    public class AbyssLibU_TooltipAnchor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>
        /// ツールチップのコンテンツ
        /// </summary>
        public AbyssLibU_TooltipContent Content => _Content;
        [SerializeField] private AbyssLibU_TooltipContent _Content;

        /// <summary>
        /// ツールチップのコンテンツ表示直前に呼び出されるコールバックです。
        /// コンテンツへのデータセットをここで行います。UI更新タイミングで登録してください。
        /// </summary>
        public Action OnBeforeShow;

        /// <summary>
        /// ツールチップコンテンツの表示要求を出します。
        /// </summary>
        public void Show()
        {
            OnBeforeShow?.Invoke();
            Content.Show(this);
        }
        /// <summary>
        /// ツールチップコンテンツの非表示要求を出します。
        /// </summary>
        public void Hide() => Content.Hide();

        /// <summary>
        /// ツールチップの配置基準となるRectTransformを取得します。
        /// 既定では自身のRectTransformを返しますが、
        /// 別のRectTransformを基準にしたい場合は派生クラスでオーバーライドしてください。
        /// </summary>
        /// <returns>ツールチップの配置基準となるRectTransformを返します。</returns>
        public virtual RectTransform GetRectTransform() => transform as RectTransform;
        /// <summary>
        /// GetRectTransform()が属するCanvasのワールドカメラを取得します。
        /// Screen Space - Overlayの場合や、Canvas配下にない場合はnullを返します。
        /// </summary>
        /// <returns>ワールドカメラを返します。</returns>
        public virtual Camera GetCamera() => GetRectTransform().GetComponentInParent<Canvas>()?.rootCanvas.worldCamera;

        //マウスホバー関連
        public void OnPointerEnter(PointerEventData e) => Show();
        public void OnPointerExit(PointerEventData e) => Hide();
    }
}