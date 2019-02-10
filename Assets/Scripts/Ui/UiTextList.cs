using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Ui {
    public class UiTextList : MonoBehaviour {
        private const int MaxLength = 500;
        private const int RemoveLength = 50;
        
        public Text textObject;

        private StringBuilder _sb = new StringBuilder();
        
        public void Add(string text) {
            _sb = _sb.AppendLine(text);

            if (_sb.Length > MaxLength) {
                _sb.Remove(0, RemoveLength);
            }

            textObject.text = _sb.ToString();
        }
    }
}