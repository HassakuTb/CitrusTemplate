using DG.Tweening;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Citrus.UI {
    /// <summary>
    /// ダイアログオブジェクト
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class Dialog : MonoBehaviour{

        [SerializeField] internal Text MessageText;
        [SerializeField] internal Button LeftButton;
        [SerializeField] internal Button CenterButton;
        [SerializeField] internal Button RightButton;
        [SerializeField] internal CanvasGroup LeftButtonGroup;
        [SerializeField] internal CanvasGroup CenterButtonGroup;
        [SerializeField] internal CanvasGroup RightButtonGroup;
        
        [SerializeField] internal Transform Contents;
        [SerializeField] internal RectTransform Window;
        [SerializeField] internal CanvasGroup WindowGroup;

        [SerializeField] internal GameObject[] ExtraContents;

        internal void Setup()
        {
            ExtraContents.ForEach(c => c.transform.SetParent(Contents, false));
            
            foreach(Transform t in Contents)
            {
                t.gameObject.SetActive(false);
            }

            GetComponent<Canvas>().enabled = false;
        }

        /// <summary>
        /// ダイアログをアニメーション表示する
        /// </summary>
        public void Show()
        {
            GetComponent<Canvas>().enabled = true;

            var sequence = DOTween.Sequence()
                .Join(
                    Window.DOScale(0.8f, 0.2f)
                        .From()
                        .SetEase(Ease.OutQuad))
                .Join(
                    DOTween.To(
                            () => WindowGroup.alpha,
                            a => WindowGroup.alpha = a,
                            0f, 0.2f)
                        .From()
                        .SetEase(Ease.OutQuad))
                .OnComplete(() =>
                {
                    foreach (Transform t in Contents)
                    {
                        t.gameObject.SetActive(true);
                    }
                });
        }

        /// <summary>
        /// ダイアログをアニメーションしながら非表示にする
        /// </summary>
        /// <param name="destroy">trueのとき閉じた後GameObjectを破棄する</param>
        public void Hide(bool destroy = false)
        {
            foreach (Transform t in Contents)
            {
                t.gameObject.SetActive(false);
            }

            var sequence = DOTween.Sequence()
                .Join(
                    Window.DOScale(0.8f, 0.2f)
                        .SetEase(Ease.InQuad))
                .Join(
                    DOTween.To(
                            () => WindowGroup.alpha,
                            a => WindowGroup.alpha = a,
                            0f, 0.2f)
                        .SetEase(Ease.InQuad))
                .OnComplete(() =>
                {
                    GetComponent<Canvas>().enabled = false;

                    if(destroy) Destroy(gameObject);
                });

        }

        /// <summary>
        /// アニメーションなしで表示する
        /// </summary>
        [Button(ButtonSizes.Large)]
        public void ShowImmediate()
        {
            GetComponent<Canvas>().enabled = true;
            Window.localScale = Vector3.one;
            WindowGroup.alpha = 1f;
            foreach (Transform t in Contents)
            {
                t.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// アニメーションなしで非表示にする
        /// </summary>
        /// <param name="destroy">trueのとき閉じた後GameObjectを破棄する</param>
        [Button(ButtonSizes.Large)]
        public void HideImmediate(bool destroy = false)
        {
            foreach (Transform t in Contents)
            {
                t.gameObject.SetActive(false);
            }
            Window.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            WindowGroup.alpha = 0f;
            GetComponent<Canvas>().enabled = false;

            if (destroy) Destroy(gameObject);
        }
    }
}
