using Project.Networking;
using Project.Player;
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
        [SerializeField]
        private Image Bullet1;
        [SerializeField]
        private Image Bullet2;
        [SerializeField]
        private Image Bullet3;
        [SerializeField]
        private Image Bullet4;
        [SerializeField]
        private Image Bullet5;
        [SerializeField]
        private GameObject StatusBar1;
        [SerializeField]
        private Image HealthBar1;
        [SerializeField]
        private Text Username1;
        [SerializeField]
        private GameObject StatusBar2;
        [SerializeField]
        private Image HealthBar2;
        [SerializeField]
        private Text Username2;
        [SerializeField]
        private GameObject StatusBar3;
        [SerializeField]
        private Image HealthBar3;
        [SerializeField]
        private Text Username3;
        [SerializeField]
        private GameObject StatusBar4;
        [SerializeField]
        private Image HealthBar4;
        [SerializeField]
        private Text Username4;
        [SerializeField]
        private GameObject StatusBar5;
        [SerializeField]
        private Image HealthBar5;
        [SerializeField]
        private Text Username5;
        [SerializeField]
        private GameObject StatusBar6;
        [SerializeField]
        private Image HealthBar6;
        [SerializeField]
        private Text Username6;

        private float blueStatusXOffset = 0.7f;
        private float blueStatusYOffset = -0.9f;
        private float orangeStatusXOffset = -0.7f;
        private float orangeStatusYOffset = 0.9f;

        private List<Image> bulletList;
        private List<GameObject> statusBarList;
        private List<Image> healthBarList;
        private List<Text> usernameList;

        void Start() {
            UICanvas.enabled = false;
            bulletList = new List<Image>();
            bulletList.Add(Bullet1);
            bulletList.Add(Bullet2);
            bulletList.Add(Bullet3);
            bulletList.Add(Bullet4);
            bulletList.Add(Bullet5);
            statusBarList = new List<GameObject>();
            statusBarList.Add(StatusBar1);
            statusBarList.Add(StatusBar2);
            statusBarList.Add(StatusBar3);
            statusBarList.Add(StatusBar4);
            statusBarList.Add(StatusBar5);
            statusBarList.Add(StatusBar6);
            healthBarList = new List<Image>();
            healthBarList.Add(HealthBar1);
            healthBarList.Add(HealthBar2);
            healthBarList.Add(HealthBar3);
            healthBarList.Add(HealthBar4);
            healthBarList.Add(HealthBar5);
            healthBarList.Add(HealthBar6);
            usernameList = new List<Text>();
            usernameList.Add(Username1);
            usernameList.Add(Username2);
            usernameList.Add(Username3);
            usernameList.Add(Username4);
            usernameList.Add(Username5);
            usernameList.Add(Username6);
        }

        public void Update() {
            // update other players' status bar
            if (NetworkClient.playerIDtoStatusBarIndex != null) {
                foreach (KeyValuePair<string, int> item in NetworkClient.playerIDtoStatusBarIndex) {
                    int index = item.Value;
                    if (item.Key != NetworkClient.ClientID) {
                        NetworkIdentity ni = NetworkClient.serverObjects[item.Key];
                        PlayerManager pm = ni.GetComponent<PlayerManager>();
                        updateStatusBar(index, pm.getTeam(), ni.transform.position.x, ni.transform.position.y);
                    }
                }
            }
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
            HealthText.text = health.ToString() + "/" + fullHealth.ToString();
        }

        public void updateMagicBar(float fullMp, float mp) {
            float scale = mp / fullMp;
            Magic.transform.localScale = new Vector3(
                scale,
                Magic.transform.localScale.y,
                1
            );
            MagicText.text = mp.ToString() + "/" + fullMp.ToString();
        }

        public void updateBulletNum(float bulletNum) {
            bulletList.ForEach(bullet => {
                bullet.transform.localScale = new Vector3(0, 1, 1);
            });
            int i = 0;
            for (; i < (int)bulletNum; ++i) {
                bulletList[i].transform.localScale = new Vector3(1, 1, 1);
            }
            float remain = bulletNum - i;
            bulletList[i].transform.localScale = new Vector3(remain, 1, 1);
        }

        public void updateStatusBar(int index, string team, float x, float y) {
            if (team == "blue") {
                statusBarList[index].transform.position = new Vector3(x + blueStatusXOffset, y + blueStatusYOffset, 0);
            }
            else {
                statusBarList[index].transform.position = new Vector3(x + orangeStatusXOffset, y + orangeStatusYOffset, 0);
            }
            
        }

        public void updateStatusBarUsername(int index, string username) {
            usernameList[index].text = username;
        }

        public void updateStatusBarHealth(int index, float fullHealth, float health) {
            float scale = health / fullHealth;
            healthBarList[index].transform.localScale = new Vector3(
                scale,
                healthBarList[index].transform.localScale.y,
                1
            );
        }

        public void toggleStatusBar(int index) {
            bool isActive = statusBarList[index].activeSelf;
            statusBarList[index].SetActive(!isActive);
        }
    }
}
