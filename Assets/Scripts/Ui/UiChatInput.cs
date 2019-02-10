using System;
using UnityEngine;
using UnityEngine.UI;

namespace Ui {
    [RequireComponent(typeof(InputField))]
    public class UiChatInput : MonoBehaviour {
        public InputField InputField { get; private set; }

        public event Action<string> TextEnterEvent;

        private void Awake() {
            InputField = GetComponent<InputField>();
        }

        public void OnEnd() {
            OnTextEnter(InputField.text);
            InputField.text = "";
        }

        private void OnTextEnter(string obj) {
            TextEnterEvent?.Invoke(obj);
        }
    }
}