using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Citrus.UI {

    /// <summary>
    /// ダイアログを生成する
    /// </summary>
    public class DialogBuilder {

        private readonly Dialog dialogPrefab;
        private string message;
        private bool isVisibleLeftButton;
        private bool isVisibleCenterButton;
        private bool isVisibleRightButton;
        private string leftText;
        private string centerText;
        private string rightText;
        private Action<Dialog> leftAction;
        private Action<Dialog> centerAction;
        private Action<Dialog> rightAction;
        private List<GameObject> extraContents = new List<GameObject>();

        /// <summary>
        /// ダイアログを生成するためのオブジェクトを生成する
        /// </summary>
        /// <param name="prefab">ダイアログのPrefab</param>
        public DialogBuilder(Dialog prefab)
        {
            this.dialogPrefab = prefab;
        }

        /// <summary>
        /// 表示するメッセージを設定する
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public DialogBuilder Message(string message)
        {
            this.message = message;
            return this;
        }

        /// <summary>
        /// 左ボタンを設定する
        /// </summary>
        /// <param name="text">ボタンテキスト</param>
        /// <param name="onClick">クリック時の動作</param>
        /// <returns></returns>
        public DialogBuilder LeftButton(string text, Action<Dialog> onClick)
        {
            this.isVisibleLeftButton = true;
            this.leftText = text;
            this.leftAction = onClick;
            return this;
        }

        /// <summary>
        /// 中央ボタンを設定する
        /// </summary>
        /// <param name="text">ボタンテキスト</param>
        /// <param name="onClick">クリック時の動作</param>
        /// <returns></returns>
        public DialogBuilder CenterButton(string text, Action<Dialog> onClick)
        {
            this.isVisibleCenterButton = true;
            this.centerText = text;
            this.centerAction = onClick;
            return this;
        }

        /// <summary>
        /// 右ボタンを設定する
        /// </summary>
        /// <param name="text">ボタンテキスト</param>
        /// <param name="onClick">クリック時の動作</param>
        /// <returns></returns>
        public DialogBuilder RightButton(string text, Action<Dialog> onClick)
        {
            this.isVisibleRightButton = true;
            this.rightText = text;
            this.rightAction = onClick;
            return this;
        }

        /// <summary>
        /// 追加のゲームオブジェクトをダイアログの部品として設定する
        /// </summary>
        /// <param name="contents">追加するゲームオブジェクト</param>
        /// <returns></returns>
        public DialogBuilder AddExtraContent(params GameObject[] contents)
        {
            extraContents.AddRange(contents);
            return this;
        }

        /// <summary>
        /// ダイアログオブジェクトを構築する
        /// </summary>
        /// <returns>構築されたダイアログ</returns>
        public Dialog Build()
        {
            Dialog dialog = GameObject.Instantiate(dialogPrefab);
            dialog.MessageText.text = message;

            dialog.LeftButtonGroup.alpha = isVisibleLeftButton ? 1f : 0f;
            dialog.LeftButtonGroup.blocksRaycasts = isVisibleLeftButton;
            dialog.LeftButton.GetComponentInChildren<Text>().text = leftText;
            dialog.LeftButton.onClick.AddListener(() => leftAction(dialog));

            dialog.CenterButtonGroup.alpha = isVisibleCenterButton ? 1f : 0f;
            dialog.CenterButtonGroup.blocksRaycasts = isVisibleCenterButton;
            dialog.CenterButton.GetComponentInChildren<Text>().text = centerText;
            dialog.CenterButton.onClick.AddListener(() => centerAction(dialog));

            dialog.RightButtonGroup.alpha = isVisibleRightButton ? 1f : 0f;
            dialog.RightButtonGroup.blocksRaycasts = isVisibleRightButton;
            dialog.RightButton.GetComponentInChildren<Text>().text = rightText;
            dialog.RightButton.onClick.AddListener(() => rightAction(dialog));

            dialog.ExtraContents = extraContents.ToArray();

            dialog.Setup();

            return dialog;
        }
    }
}
