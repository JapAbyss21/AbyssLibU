using UnityEngine;
using UnityEngine.UI;

namespace AbyssLibU
{

    /// <summary>
    /// InputFieldの全角文字入力バグ対応
    /// InputFieldに追加してください。
    /// </summary>
    [RequireComponent(typeof(InputField))]
    public class AbyssLibU_InputfieldZenkakuBSFix : MonoBehaviour
    {
        private InputField T1;
        private string S = "";
        private int pos = 0;
        private int Tpos = 0;

        /// <summary>
        /// 初期化処理です。
        /// </summary>
        private void Start()
        {
            T1 = gameObject.GetComponent<InputField>();
        }

        /// <summary>
        /// フレーム毎処理です。
        /// </summary>
        private void Update()
        {
            //日本語入力の全角変換中に確定させない状態でInputFieldから
            //フォーカスを外すと、変換中の文字が倍加するバグに対応
            if(Input.GetMouseButtonDown(0))
            {
                if(string.IsNullOrEmpty(T1.text))
                {
                    if (S != T1.text)
                    {
                        if (string.IsNullOrEmpty(S))
                        {
                            if (T1.text.Length - Tpos > 1)
                            {
                                if (Tpos + (pos - Tpos) * 2 == T1.text.Length)
                                {
                                    string hantei = T1.text.Substring(Tpos);
                                    if (hantei.Length > 1 && hantei.Length % 2 == 0)
                                    {
                                        int cun = hantei.Length;
                                        if (hantei.Substring(0, cun / 2) == hantei.Substring(cun / 2, cun / 2))
                                        {
                                            //未確定時
                                            T1.text = T1.text.Substring(0, Tpos) + hantei.Substring(0, cun / 2);
                                        }
                                    }
                                }
                                else
                                {
                                    int usiro = (T1.text.Length - (Tpos + (pos - Tpos) * 2)) / 2;
                                    int mae = Tpos - usiro;
                                    string hantei = T1.text.Remove(T1.text.Length - usiro).Substring(mae);
                                    if (hantei.Length > 1 && hantei.Length % 2 == 0)
                                    {
                                        int cun = hantei.Length;
                                        if (hantei.Substring(0, cun / 2) == hantei.Substring(cun / 2, cun / 2))
                                        {
                                            T1.text = T1.text.Substring(0, mae) + hantei.Substring(0, cun / 2) + T1.text.Substring(T1.text.Length - usiro, usiro);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (T1.text.Length > 1 && T1.text.Length % 2 == 0)
                            {
                                int cun = T1.text.Length;
                                if (T1.text.Substring(0, cun / 2) == T1.text.Substring(cun / 2, cun / 2))
                                {
                                    //未確定時
                                    T1.text = T1.text.Substring(0, cun / 2);
                                }
                            }
                        }
                    }
                }
            }
            S = T1.text;
            Tpos = T1.text.Length;
            pos = T1.selectionAnchorPosition;
        }
        /// <summary>
        /// フレーム毎処理です（Update後）
        /// </summary>
        private void LateUpdate()
        {
            if(T1.GetComponent<InputField>().isFocused)
            {
                T1.ForceLabelUpdate();
            }
        }
    }

}
