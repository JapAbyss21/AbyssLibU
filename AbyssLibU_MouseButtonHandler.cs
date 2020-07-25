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
        [System.Serializable]
        public class ButtonEvents : UnityEvent { }
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
