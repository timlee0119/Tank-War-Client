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
        [SerializeField]
        private GameObject BlueSafeBoxHealthPosition;
        [SerializeField]
        private Image BlueSafeBoxHealth;
        [SerializeField]
        private GameObject OrangeSafeBoxHealthPosition;
        [SerializeField]
        private Image OrangeSafeBoxHealth;
        [SerializeField]
        private Text Time;

        private float blueEnemyStatusXOffset = 0.7f;
        private float blueEnemyStatusYOffset = -0.9f;
        private float orangeEnemyStatusXOffset = -0.7f;
        private float orangeEnemyStatusYOffset = 0.9f;
        private float blueTeammateStatusXOffset = -0.63f;
        private float blueTeammateStatusYOffset = 0.9f;
        private float orangeTeammateStatusXOffset = 0.63f;
        private float orangeTeammateStatusYOffset = -0.9f;
        private float blueSafeBoxHealthXOffset = 1.65f;
        private float blueSafeBoxHealthYOffset = 1.1f;
        private float orangeSafeBoxHealthXOffset = 1.65f;
        private float orangeSafeBoxHealthYOffset = -1.5f;

        private GameObject blueSafeBoxStatusPosition;
        private GameObject orangeSafeBoxStatusPosition;

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
            foreach (GameObject go in statusBarList) {
                go.SetActive(false);
            }
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
            BlueSafeBoxHealthPosition.SetActive(false);
            OrangeSafeBoxHealthPosition.SetActive(false);

            blueSafeBoxStatusPosition = new GameObject();
            blueSafeBoxStatusPosition.name = "blueSafeBoxStatusPosition";
            orangeSafeBoxStatusPosition = new GameObject();
            orangeSafeBoxStatusPosition.name = "orangeSafeBoxStatusPosition";
        }

        public void Update() {
            // update other players' status bar
            string myteam = "";
            if (NetworkClient.playerIDtoStatusBarIndex != null && NetworkClient.ClientID != null
                && NetworkClient.serverObjects != null && NetworkClient.serverObjects.ContainsKey(NetworkClient.ClientID)) {
                myteam = NetworkClient.serverObjects[NetworkClient.ClientID].GetComponent<PlayerManager>().getTeam();
                foreach (KeyValuePair<string, int> item in NetworkClient.playerIDtoStatusBarIndex) {
                    int index = item.Value;
                    if (item.Key != NetworkClient.ClientID) {
                        NetworkIdentity ni = NetworkClient.serverObjects[item.Key];
                        PlayerManager pm = ni.GetComponent<PlayerManager>();
                        updateStatusBar(index, pm.getTeam(), myteam, ni.transform.position.x, ni.transform.position.y);
                    }
                }
            }

            // update safe boxes' position
            if (myteam == "blue") {
                BlueSafeBoxHealthPosition.transform.position = new Vector3(
                    blueSafeBoxStatusPosition.transform.position.x - blueSafeBoxHealthXOffset,
                    blueSafeBoxStatusPosition.transform.position.y - orangeSafeBoxHealthYOffset,
                    0
                );
                OrangeSafeBoxHealthPosition.transform.position = new Vector3(
                    orangeSafeBoxStatusPosition.transform.position.x - orangeSafeBoxHealthXOffset,
                    orangeSafeBoxStatusPosition.transform.position.y - blueSafeBoxHealthYOffset,
                    0
                );
            }
            else if (myteam == "orange") {
                BlueSafeBoxHealthPosition.transform.position = new Vector3(
                    blueSafeBoxStatusPosition.transform.position.x + blueSafeBoxHealthXOffset,
                    blueSafeBoxStatusPosition.transform.position.y + blueSafeBoxHealthYOffset,
                    0
                );
                OrangeSafeBoxHealthPosition.transform.position = new Vector3(
                    orangeSafeBoxStatusPosition.transform.position.x + orangeSafeBoxHealthXOffset,
                    orangeSafeBoxStatusPosition.transform.position.y + orangeSafeBoxHealthYOffset,
                    0
                );
            }
        }

        public void CleanUp() {
            foreach (GameObject go in statusBarList) {
                go.SetActive(false);
            }
            BlueSafeBoxHealthPosition.SetActive(false);
            OrangeSafeBoxHealthPosition.SetActive(false);
            updateSafeBoxHealth("blue", 1, 1);
            updateSafeBoxHealth("orange", 1, 1);
            for (int i = 0; i < healthBarList.Count; ++i) {
                updateStatusBarHealth(i, 1, 1);
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

        public void updateStatusBar(int index, string team, string myteam, float x, float y) {
            if (team == "blue") {
                if (myteam == "orange") {
                    statusBarList[index].transform.position = new Vector3(x + blueEnemyStatusXOffset, y + blueEnemyStatusYOffset, 0);
                }
                else {
                    statusBarList[index].transform.position = new Vector3(x + blueTeammateStatusXOffset, y + blueTeammateStatusYOffset, 0);
                }
            }
            else {
                if (myteam == "blue") {
                    statusBarList[index].transform.position = new Vector3(x + orangeEnemyStatusXOffset, y + orangeEnemyStatusYOffset, 0);
                }
                else {
                    statusBarList[index].transform.position = new Vector3(x + orangeTeammateStatusXOffset, y + orangeTeammateStatusYOffset, 0);
                }
            }
        }

        public void updateStatusBarUsername(int index, string username, string team) {
            Color color = new Color();
            if (team == "blue") {
                ColorUtility.TryParseHtmlString("#0053FF", out color);
            }
            else {
                ColorUtility.TryParseHtmlString("#FF5F00", out color);
            }
            usernameList[index].text = username;
            usernameList[index].color = color;
            usernameList[index].fontStyle = FontStyle.Bold;
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

        public void toggleSafeBoxHealthBar(string team) {
            if (team == "blue") {
                bool isActive = BlueSafeBoxHealthPosition.activeSelf;
                BlueSafeBoxHealthPosition.SetActive(!isActive);
            }
            else {
                bool isActive = OrangeSafeBoxHealthPosition.activeSelf;
                OrangeSafeBoxHealthPosition.SetActive(!isActive);
            }
        }

        public void setSafeBoxHealthBarPosition(string team, float x, float y) {
            if (team == "blue") {
                blueSafeBoxStatusPosition.transform.position = new Vector3(x, y, 0);
            }
            else if (team == "orange") {
                orangeSafeBoxStatusPosition.transform.position = new Vector3(x, y, 0);
            }
            else {
                Debug.LogError("undefined team");
            }
        }

        public void updateSafeBoxHealth(string team, float fullHealth, float health) {
            float scale = health / fullHealth;
            if (team == "blue") {
                BlueSafeBoxHealth.transform.localScale = new Vector3(
                    scale,
                    BlueSafeBoxHealth.transform.localScale.y,
                    1
                );
            }
            else if (team == "orange") {
                OrangeSafeBoxHealth.transform.localScale = new Vector3(
                    scale,
                    OrangeSafeBoxHealth.transform.localScale.y,
                    1
                );
            }
            else {
                Debug.LogError("undefined team");
            }
        }

        public void setTime(string time) {
            this.Time.text = time;
        }
    }
}
