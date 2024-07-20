using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace AbyssLibU
{
    /// <summary>
    /// マウスイベントを追加します。
    /// UIコンポーネントに追加してください。
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("Event/MouseButton")]
    public class AbyssLibU_MouseButtonHandler : MonoBehaviour, IPointerClickHandler
    {
#pragma warning disable CA1034 // Nested types should not be visible
        [Serializable]public class ButtonEvents : UnityEvent { }
#pragma warning restore CA1034 // Nested types should not be visible
        public ButtonEvents onLeftClick;
        public ButtonEvents onRightClick;
        public ButtonEvents onMiddleClick;

        public void OnPointerClick(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    onLeftClick.Invoke();
                    break;
                case PointerEventData.InputButton.Right:
                    onRightClick.Invoke();
                    break;
                case PointerEventData.InputButton.Middle:
                    onMiddleClick.Invoke();
                    break;
                default:
                    break;
            }
        }
    }
}