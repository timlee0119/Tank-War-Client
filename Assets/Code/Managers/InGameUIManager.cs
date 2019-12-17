using Project.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Managers {
    public class InGameUIManager : Singleton<InGameUIManager> {
        [SerializeField]
        private Canvas UICanvas;
        [SerializeField]
        private Image Health;
        [SerializeField]
        private Text HealthText;
        [SerializeField]
        private Image Magic;
        [SerializeField]
        private Text MagicText;

        void Start() {
            UICanvas.enabled = false;
        }

        public void activateGameUICanvas() {
            UICanvas.enabled = true;
        }

        public void updateHealthBar(float fullHealth, float health) {
            float scale = health / fullHealth;
            Health.transform.localScale = new Vector3(
                scale,
                Health.transform.localScale.y,
                1
            );
            HealthText.text = string.Format("%s %", (scale * 100).ToString("0.0"));
        }
    }
}
