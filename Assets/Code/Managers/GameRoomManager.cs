using System.Collections;
using System.Collections.Generic;
using Project.Networking;
using Project.Utility;
using SocketIO;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Managers {
    public class GameRoomManager : MonoBehaviour {
        

        [SerializeField]
        private Text roomID;

        private List<GameObject> prefabsToDelete;

        private Color color;

        [SerializeField]
        private Button returnButton;

        [SerializeField]
        private Text readyOrNot;
        [SerializeField]
        private GameObject readyPrefab;

        [SerializeField]
        private GameObject lifeTankPrefab;
        [SerializeField]
        private GameObject fireTankPrefab;
        [SerializeField]
        private GameObject shieldTankPrefab;
        [SerializeField]
        private GameObject oceanTankPrefab;
        [SerializeField]
        private GameObject sandTankPrefab;
        [SerializeField]
        private GameObject shadowTankPrefab;

        [SerializeField]
        private Transform bigTankContainer;
        [SerializeField]
        private Image teamColor;

        [SerializeField]
        private Text blueTeamUsername_1;
        [SerializeField]
        private Transform blueTeamContainer_1;

        [SerializeField]
        private Text blueTeamUsername_2;
        [SerializeField]
        private Transform blueTeamContainer_2;

        [SerializeField]
        private Text blueTeamUsername_3;
        [SerializeField]
        private Transform blueTeamContainer_3;

        [SerializeField]
        private Text orangeTeamUsername_1;
        [SerializeField]
        private Transform orangeTeamContainer_1;

        [SerializeField]
        private Text orangeTeamUsername_2;
        [SerializeField]
        private Transform orangeTeamContainer_2;

        [SerializeField]
        private Text orangeTeamUsername_3;
        [SerializeField]
        private Transform orangeTeamContainer_3;

        [SerializeField]
        private GameObject TankDescription;
        [SerializeField]
        private Sprite[] TankDescriptionSprites;

        private SocketIOComponent socketReference;

        // map tmpBlue/Orange to position
        private List<Transform> blueTeamPositionMap;
        private List<Transform> orangeTeamPositionMap;
        private List<Text> blueTeamTextMap;
        private List<Text> orangeTeamTextMap;

        // map tank id to prefab
        private List<GameObject> tankIDtoPrefab;

        // canvas scale value (for bigtank position)
        [SerializeField]
        private Canvas canvas;
        private float canvasScale;

        public SocketIOComponent SocketReference {
            get {
                return socketReference = (socketReference == null) ? FindObjectOfType<NetworkClient>() : socketReference;
            }
        }

        void Start() {
            returnButton.GetComponent<AudioSource>().Play();
            roomID.text = NetworkClient.RoomID.ToString();
            prefabsToDelete = new List<GameObject>();
            color = new Color();

            UserInGameRoom me = NetworkClient.usersInGameRoom[NetworkClient.ClientID];
            if (me.team == "blue") {
                ColorUtility.TryParseHtmlString("#61D3FF", out color);
                teamColor.color = color;
            } else if (me.team == "orange") {
                ColorUtility.TryParseHtmlString("#FDB174", out color);
                teamColor.color = color;
            }

            // initialize position and tank prefab maps
            blueTeamPositionMap = new List<Transform>();
            blueTeamPositionMap.Add(blueTeamContainer_1);
            blueTeamPositionMap.Add(blueTeamContainer_2);
            blueTeamPositionMap.Add(blueTeamContainer_3);
            orangeTeamPositionMap = new List<Transform>();
            orangeTeamPositionMap.Add(orangeTeamContainer_1);
            orangeTeamPositionMap.Add(orangeTeamContainer_2);
            orangeTeamPositionMap.Add(orangeTeamContainer_3);
            blueTeamTextMap = new List<Text>();
            blueTeamTextMap.Add(blueTeamUsername_1);
            blueTeamTextMap.Add(blueTeamUsername_2);
            blueTeamTextMap.Add(blueTeamUsername_3);
            orangeTeamTextMap = new List<Text>();
            orangeTeamTextMap.Add(orangeTeamUsername_1);
            orangeTeamTextMap.Add(orangeTeamUsername_2);
            orangeTeamTextMap.Add(orangeTeamUsername_3);
            tankIDtoPrefab = new List<GameObject>();
            tankIDtoPrefab.Add(lifeTankPrefab);
            tankIDtoPrefab.Add(fireTankPrefab);
            tankIDtoPrefab.Add(shieldTankPrefab);
            tankIDtoPrefab.Add(oceanTankPrefab);
            tankIDtoPrefab.Add(sandTankPrefab);
            tankIDtoPrefab.Add(shadowTankPrefab);

            // get canvas scale (for bigtank position)
            canvasScale = canvas.transform.localScale.y;
        }

        void Update() {

            int blueNum = 0;
            int orangeNum = 0;

            blueTeamUsername_1.text = "";
            blueTeamUsername_2.text = "";
            blueTeamUsername_3.text = "";
            orangeTeamUsername_1.text = "";
            orangeTeamUsername_2.text = "";
            orangeTeamUsername_3.text = "";

            foreach (GameObject go in prefabsToDelete) {
                Destroy(go);
            }

            foreach (KeyValuePair<string, UserInGameRoom> item in NetworkClient.usersInGameRoom) {
                UserInGameRoom player = item.Value;

                if (player.team == "blue") {
                    blueTeamTextMap[blueNum].text = player.username;
                    GameObject go = Instantiate(tankIDtoPrefab[player.tank], blueTeamPositionMap[blueNum]);
                    prefabsToDelete.Add(go);
                    if (player.id == NetworkClient.ClientID) {
                        // change team color of bigtank
                        ColorUtility.TryParseHtmlString("#61D3FF", out color);
                        teamColor.color = color;
                    }

                    if (player.ready) {
                        go = Instantiate(readyPrefab, blueTeamPositionMap[blueNum]);
                        prefabsToDelete.Add(go);
                    }

                    blueNum++;
                }
                else if (player.team == "orange") {
                    orangeTeamTextMap[orangeNum].text = player.username;
                    GameObject go = Instantiate(tankIDtoPrefab[player.tank], orangeTeamPositionMap[orangeNum]);
                    prefabsToDelete.Add(go);
                    if (player.id == NetworkClient.ClientID) {
                        // change team color of bigtank
                        ColorUtility.TryParseHtmlString("#FDB174", out color);
                        teamColor.color = color;
                    }

                    if (player.ready) {
                        go = Instantiate(readyPrefab, orangeTeamPositionMap[orangeNum]);
                        prefabsToDelete.Add(go);
                    }

                    orangeNum++;
                }
                else {
                    Debug.Log("Unknown team"); return;
                }
                
                if (player.id == NetworkClient.ClientID) {
                    // Instantiate big tank
                    GameObject bigtank = Instantiate(tankIDtoPrefab[player.tank], bigTankContainer);
                    bigtank.transform.localScale = new Vector3(120, 120, 1);
                    bigtank.transform.position -= new Vector3(0, 5 * canvasScale, 0);
                    prefabsToDelete.Add(bigtank);

                    // update tank descriptions
                    TankDescription.GetComponent<SpriteRenderer>().sprite = TankDescriptionSprites[player.tank];
                }
            }
        }

        public void PressBlueTeam() {
            returnButton.GetComponent<AudioSource>().Play();
            JSONObject j = new JSONObject();
            j.AddField("team", "blue");
            SocketReference.Emit("switchTeam", j);
        }
        public void PressOrangeTeam() {
            returnButton.GetComponent<AudioSource>().Play();
            JSONObject j = new JSONObject();
            j.AddField("team", "orange");
            SocketReference.Emit("switchTeam", j);
        }

        public void PressReady() {
            returnButton.GetComponent<AudioSource>().Play();
            SocketReference.Emit("switchReady");
            UserInGameRoom me = NetworkClient.usersInGameRoom[NetworkClient.ClientID];
            if (me.ready) {
                readyOrNot.text = "READY";
            } else {
                readyOrNot.text = "CANCEL";
            }
        }

        public void PressSelectLeft() {
            returnButton.GetComponent<AudioSource>().Play();
            UserInGameRoom me = NetworkClient.usersInGameRoom[NetworkClient.ClientID];
            if (!me.ready) {
                JSONObject j = new JSONObject();
                j.AddField("direction", "left");
                SocketReference.Emit("switchTank", j);
            }
        }

        public void PressSelectRight() {
            returnButton.GetComponent<AudioSource>().Play();
            UserInGameRoom me = NetworkClient.usersInGameRoom[NetworkClient.ClientID];
            if (!me.ready) {
                JSONObject j = new JSONObject();
                j.AddField("direction", "right");
                SocketReference.Emit("switchTank", j);
            }
        }

        public void PressExit() {
            SocketReference.Emit("switchToBaseLobby");
            NetworkClient.usersInGameRoom.Clear();
            SceneManagementManager.Instance.LoadLevel(SceneList.LOBBY, (levelName) => {
                SceneManagementManager.Instance.UnLoadLevel(SceneList.GAMEROOM);
            });
        }

        public void PressAttributes() {
            returnButton.GetComponent<AudioSource>().Play();
            SpriteRenderer s = TankDescription.GetComponent<SpriteRenderer>();
            var tempColor = s.color;
            tempColor.a = (tempColor.a + 1) % 2;
            s.color = tempColor;
        }
    }
}
