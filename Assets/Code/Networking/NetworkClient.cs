using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using System;
using Project.Player;
using Project.Scriptable;
using Project.Gameplay;
using Project.Utility;
using Project.Managers;

namespace Project.Networking {
    public class NetworkClient : SocketIOComponent {

        public const float SERVER_UPDATE_TIME = 10;

        [Header("Network Client")]
        [SerializeField]
        private Transform blue1Container;
        [SerializeField]
        private Transform blue2Container;
        [SerializeField]
        private Transform blue3Container;
        [SerializeField]
        private Transform orange1Container;
        [SerializeField]
        private Transform orange2Container;
        [SerializeField]
        private Transform orange3Container;
        [SerializeField]
        private ServerObjects serverSpawnables;
        [SerializeField]
        private NetworkPrefabs networkPrefabs;

        public static string ClientID { get; private set; }
        public static int RoomID { get; private set; }
        public static Dictionary<string, UserInGameRoom> usersInGameRoom { get; private set; }

        public static Dictionary<string, NetworkIdentity> serverObjects;

        public static Dictionary<int, GameRoomInfo> gameRoomsInfo { get; private set; }

        private List<GameObject> tankIDtoPrefab;

        private List<Transform> positionIDToContainer;

        public static Dictionary<string, int> playerIDtoStatusBarIndex;

        // Use this for initialization
        public override void Start() {
            base.Start();
            initialize();
            setupEvents();
        }

        // Update is called once per frame
        public override void Update() {
            base.Update();
        }

        private void initialize() {
            serverObjects = new Dictionary<string, NetworkIdentity>();
            gameRoomsInfo = new Dictionary<int, GameRoomInfo>();
            usersInGameRoom = new Dictionary<string, UserInGameRoom>();
            tankIDtoPrefab = new List<GameObject>();
            tankIDtoPrefab.Add(networkPrefabs.lifeTankPrefab);
            tankIDtoPrefab.Add(networkPrefabs.fireTankPrefab);
            tankIDtoPrefab.Add(networkPrefabs.shieldTankPrefab);
            tankIDtoPrefab.Add(networkPrefabs.oceanTankPrefab);
            tankIDtoPrefab.Add(networkPrefabs.sandTankPrefab);
            tankIDtoPrefab.Add(networkPrefabs.shadowTankPrefab);
            positionIDToContainer = new List<Transform>();
            positionIDToContainer.Add(blue1Container);
            positionIDToContainer.Add(blue2Container);
            positionIDToContainer.Add(blue3Container);
            positionIDToContainer.Add(orange1Container);
            positionIDToContainer.Add(orange2Container);
            positionIDToContainer.Add(orange3Container);
            playerIDtoStatusBarIndex = new Dictionary<string, int>();
        }

        private void setupEvents() {
            On("open", (E) => {
                Debug.Log("Connection made!");
            });

            On("register", (E) => {
                ClientID = E.data["id"].RemoveQuotes();
                RoomID = -1;

                Debug.LogFormat("Our Client's ID ({0})", ClientID);
            });

            On("disconnected", (E) => {
                
            });

            On("updatePosition", (E) => {
                string id = E.data["id"].RemoveQuotes();
                float x = E.data["position"]["x"].f;
                float y = E.data["position"]["y"].f;

                NetworkIdentity ni = serverObjects[id];
                ni.transform.position = new Vector3(x, y, 0);
            });

            On("updateRotation", (E) => {
                string id = E.data["id"].RemoveQuotes();
                float tankRotation = E.data["tankRotation"].f;
                float barrelRotation = E.data["barrelRotation"].f;

                NetworkIdentity ni = serverObjects[id];
                ni.transform.localEulerAngles = new Vector3(0, 0, tankRotation);
                ni.GetComponent<PlayerManager>().SetRotation(barrelRotation);
            });

            On("serverSpawn", (E) => {
                string name = E.data["name"].str;
                string id = E.data["id"].RemoveQuotes();
                float x = E.data["position"]["x"].f;
                float y = E.data["position"]["y"].f;
                Debug.LogFormat("Server wants us to spawn a '{0}'", name);

                if (!serverObjects.ContainsKey(id)) {
                    ServerObjectData sod = serverSpawnables.GetObjectByName(name);
                    var spawnedObject = Instantiate(sod.Prefab, blue1Container);
                    spawnedObject.transform.position = new Vector3(x, y, 0);
                    var ni = spawnedObject.GetComponent<NetworkIdentity>();
                    ni.SetControllerID(id);
                    ni.SetSocketReference(this);

                    // If bullet apply direction as well
                    if (name == "Bullet") {
                        float directionX = E.data["direction"]["x"].f;
                        float directionY = E.data["direction"]["y"].f;
                        string activator = E.data["activator"].RemoveQuotes();
                        float speed = E.data["speed"].f;

                        float rot = Mathf.Atan2(directionY, directionX) * Mathf.Rad2Deg;
                        Vector3 currentRotation = new Vector3(0, 0, rot - 90);
                        spawnedObject.transform.rotation = Quaternion.Euler(currentRotation);

                        WhoActivatedMe whoActivatedMe = spawnedObject.GetComponent<WhoActivatedMe>();
                        whoActivatedMe.SetActivator(activator);

                        Projectile projectile = spawnedObject.GetComponent<Projectile>();
                        projectile.Direction = new Vector2(directionX, directionY);
                        projectile.Speed = speed;
                    }

                    serverObjects.Add(id, ni);
                }
            });

            On("serverUnspawn", (E) => {
                string id = E.data["id"].RemoveQuotes();
                NetworkIdentity ni = serverObjects[id];
                serverObjects.Remove(id);
                DestroyImmediate(ni.gameObject);
            });

            On("loadGameRoom", callback: (E) => {
                Debug.Log(message: "Switching to gameRoom");
                RoomID = E.data["id"].i();

                SceneManagementManager.Instance.LoadLevel(SceneList.GAMEROOM, (levelName) => {
                    SceneManagementManager.Instance.UnLoadLevel(SceneList.LOBBY);
                });
            });

            On("loadBaseLobby", callback: (E) => {
                Debug.Log(message: "Switching to lobby");

                SceneManagementManager.Instance.LoadLevel(SceneList.LOBBY, (levelName) => {
                    SceneManagementManager.Instance.UnLoadLevel(SceneList.LOGIN);
                });
            });

            On("updateBaseLobby", (E) => {
                int len = E.data["gameRoomsInfo"].list.Count;
                for (int i = 0; i < len; i++) {
                    int id = E.data["gameRoomsInfo"][i]["id"].i();
                    int player_num = E.data["gameRoomsInfo"][i]["player_num"].i();
                    int maxPlayers = E.data["gameRoomsInfo"][i]["maxPlayers"].i();
                    string gameMode = E.data["gameRoomsInfo"][i]["gameMode"].RemoveQuotes();
                    bool playing = E.data["gameRoomsInfo"][i]["playing"].b;
                    if (gameRoomsInfo.ContainsKey(id)) {
                        gameRoomsInfo[id].player_num = player_num;
                    } else {
                        GameRoomInfo gameRoom = new GameRoomInfo();
                        gameRoom.id = id;
                        gameRoom.player_num = player_num;
                        gameRoom.maxPlayers = maxPlayers;
                        gameRoom.gameMode = gameMode;
                        gameRoom.playing = playing;
                        gameRoomsInfo.Add(id, gameRoom);
                    }
                }
            });

            On("spawnToGameRoom", (E) => {

                string id = E.data["id"].RemoveQuotes();
                string username = E.data["username"].RemoveQuotes();
                int tank = E.data["tank"].i();
                string team = E.data["team"].RemoveQuotes();
                bool ready = E.data["ready"].b;

                if (usersInGameRoom.ContainsKey(id)) {
                    usersInGameRoom[id].team = team;
                    usersInGameRoom[id].ready = ready;
                    usersInGameRoom[id].tank = tank;
                } else {
                    UserInGameRoom u = new UserInGameRoom();
                    u.id = id;
                    u.username = username;
                    u.tank = tank;
                    u.team = team;
                    u.ready = ready;
                    usersInGameRoom.Add(u.id, u);
                }
            });

            On("updateGameRoom", (E) => {
                int len = E.data["playersData"].list.Count;

                for (int i = 0; i < len; i++) {
                    string id = E.data["playersData"][i]["id"].RemoveQuotes();
                    string username = E.data["playersData"][i]["username"].RemoveQuotes();
                    int tank = E.data["playersData"][i]["tank"].i();
                    bool ready = E.data["playersData"][i]["ready"].b;
                    string team = E.data["playersData"][i]["team"].RemoveQuotes();

                    if (usersInGameRoom.ContainsKey(id)) {
                        usersInGameRoom[id].team = team;
                        usersInGameRoom[id].ready = ready;
                        usersInGameRoom[id].tank = tank;
                    } else {
                        UserInGameRoom u = new UserInGameRoom();
                        u.id = E.data["id"].RemoveQuotes();
                        u.username = E.data["username"].RemoveQuotes();
                        u.tank = E.data["tank"].i();
                        u.team = E.data["team"].RemoveQuotes();
                        u.ready = E.data["ready"].b;

                        usersInGameRoom.Add(u.id, u);
                    }
                }
            });

            On("leaveGameRoom", (E) => {
                string id = E.data["id"].RemoveQuotes();
                // maintain usersInGameRoom, serverObjects, playersIDtoStatusBarIndex
                if (E.data["playing"].b) {
                    // hide statusbar
                    InGameUIManager.Instance.toggleStatusBar(playerIDtoStatusBarIndex[id]);

                    GameObject go = serverObjects[id].gameObject;
                    Destroy(go);
                    serverObjects.Remove(id);
                    playerIDtoStatusBarIndex.Remove(id);
                }

                usersInGameRoom.Remove(id);
            });

            On("gameStart", (E) => {
                string mode = E.data["gameMode"].RemoveQuotes();
                if (mode == "Heist") {
                    Debug.Log("Start Heist");
                    SceneManagementManager.Instance.LoadLevel(SceneList.HEIST, (levelName) => {
                        SceneManagementManager.Instance.UnLoadLevel(SceneList.GAMEROOM);
                        InGameUIManager.Instance.activateGameUICanvas();
                    });
                }
                else if (mode == "Showdown") {
                    Debug.Log("Start Showdown");
                    SceneManagementManager.Instance.LoadLevel(SceneList.SHOWDOWN, (levelName) => {
                        SceneManagementManager.Instance.UnLoadLevel(SceneList.GAMEROOM);
                        InGameUIManager.Instance.activateGameUICanvas();
                    });
                }
                else {
                    Debug.Log("Unknown mode");
                }
            });

            On("spawnPlayers", (E) => {
                int playerNum = E.data["playersInfo"].list.Count;
                JSONObject playersInfo = E.data["playersInfo"];
                for (int i = 0; i < playerNum; ++i) {
                    string id = playersInfo[i]["id"].RemoveQuotes();
                    GameObject go = Instantiate(tankIDtoPrefab[playersInfo[i]["tank"].i()]
                                                , positionIDToContainer[playersInfo[i]["startPosition"].i()]);
                    // if I am orange team, rotate the camera and tank
                    if (id == ClientID && playersInfo[i]["team"].RemoveQuotes() == "orange") {
                        Transform cameraTransform = Camera.main.transform;
                        cameraTransform.eulerAngles = new Vector3(
                            cameraTransform.eulerAngles.x,
                            cameraTransform.eulerAngles.y,
                            cameraTransform.eulerAngles.z + 180
                        );
                        go.transform.eulerAngles = new Vector3(
                            go.transform.eulerAngles.x,
                            go.transform.eulerAngles.y,
                            go.transform.eulerAngles.z + 180
                        );
                    }

                    // Map ID to networkidentity
                    go.name = playersInfo[i]["username"].RemoveQuotes();
                    NetworkIdentity ni = go.GetComponent<NetworkIdentity>();
                    ni.SetControllerID(id);
                    ni.SetSocketReference(this);
                    serverObjects.Add(id, ni);

                    // initialize all players' info
                    PlayerManager pm = go.GetComponent<PlayerManager>();
                    pm.setInfo(playersInfo[i]);

                    // map all players' id to status bar index
                    playerIDtoStatusBarIndex.Add(id, i);
                    // update other players' status bar username
                    if (id != ClientID) {
                        InGameUIManager.Instance.updateStatusBarUsername(i, go.name);
                    }
                }

                // initialize my status bar, rotate camera if orange team
                PlayerManager p = serverObjects[ClientID].GetComponent<PlayerManager>();
                InGameUIManager.Instance.updateHealthBar(p.getFullHealth(), p.getHealth());
                InGameUIManager.Instance.updateMagicBar(p.getFullMp(), p.getMp());
            });

            On("setPlayerHealth", (E) => {
                string id = E.data["id"].RemoveQuotes();
                NetworkIdentity ni = serverObjects[id];
                float health = E.data["health"].f;
                Debug.Log(health);

                PlayerManager pm = ni.GetComponent<PlayerManager>();
                // update my UI Canvas health bar
                if (id == ClientID) {
                    pm.setHealth(health);
                    InGameUIManager.Instance.updateHealthBar(pm.getFullHealth(), health);
                }
                // update all players' health bar on head
                else {
                    InGameUIManager.Instance.updateStatusBarHealth(playerIDtoStatusBarIndex[id]
                                                                  , pm.getFullHealth(), health);
                }
            });

            On("playerDied", (E) => {
                string id = E.data["id"].RemoveQuotes();
                NetworkIdentity ni = serverObjects[id];
                ni.gameObject.SetActive(false);

                // hide status bar
                InGameUIManager.Instance.toggleStatusBar(playerIDtoStatusBarIndex[id]);
            });

            On("playerRespawn", (E) => {
                string id = E.data["id"].RemoveQuotes();
                NetworkIdentity ni = serverObjects[id];
                ni.transform.position = positionIDToContainer[E.data["startPosition"].i()].position;
                ni.gameObject.SetActive(true);

                // show status bar
                InGameUIManager.Instance.toggleStatusBar(playerIDtoStatusBarIndex[id]);
            });

            On("updateBulletNum", (E) => {
                float bulletNum = E.data["bulletNum"].f;
                InGameUIManager.Instance.updateBulletNum(bulletNum);
            });
        }

        public void AttemptToJoinLobby() {
            Emit("joinGame");
        }

    }

    [Serializable]
    public class Player {
        public string id;
        public Position position;
    }

    [Serializable]
    public class Position {
        public float x;
        public float y;
    }

    [Serializable]
    public class PlayerRotation {
        public float tankRotation;
        public float barrelRotation;
    }

    [Serializable]
    public class BulletData {
        public string id;
        public string activator;
        public Position position;
        public Position direction;
    }

    [Serializable]
    public class IDData {
        public string id;
    }

    [Serializable]
    public class GameRoomInfo {
        public int id;
        public int player_num;
        public int maxPlayers;
        public string gameMode;
        public bool playing;
    }

    [Serializable]
    public class UserInGameRoom {
        public string id;
        public string username;
        public string team;
        public int tank;
        public bool ready;
    }
}
