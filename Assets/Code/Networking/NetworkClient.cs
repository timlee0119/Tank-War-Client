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
using UB.Simple2dWeatherEffects.Standard;

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
        private Transform blueSafeBoxContainer;
        [SerializeField]
        private Transform orangeSafeBoxContainer;
        [SerializeField]
        private Transform outSideContainer;
        [SerializeField]
        private ServerObjects serverSpawnables;
        [SerializeField]
        private NetworkPrefabs networkPrefabs;
        [SerializeField]
        private Sprite deadSafeBox;
        [SerializeField]
        private Camera cameraForMoving;

        public static string ClientID { get; private set; }
        public static int RoomID { get; private set; }
        public static Dictionary<string, UserInGameRoom> usersInGameRoom { get; private set; }

        public static Dictionary<string, NetworkIdentity> serverObjects;

        public static Dictionary<int, GameRoomInfo> gameRoomsInfo { get; private set; }

        public static List<GameObject> tankIDtoPrefab;

        private List<Transform> positionIDToContainer;

        public static Dictionary<string, int> playerIDtoStatusBarIndex;

        private string currentGameMode;

        public static Dictionary<string, List<KeyValuePair<string, GameObject>>> specialStatus;

        private List<string> sandStormStatus;

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
            specialStatus = new Dictionary<string, List<KeyValuePair<string, GameObject>>>();
            sandStormStatus = new List<string>();
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
                        gameRoomsInfo[id].playing = playing;
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
                currentGameMode = mode;
                if (mode == "Heist") {
                    Debug.Log("Start Heist");
                    SceneManagementManager.Instance.LoadLevel(SceneList.HEIST, (levelName) => {
                        SceneManagementManager.Instance.UnLoadLevel(SceneList.GAMEROOM);
                        InGameUIManager.Instance.activateGameUICanvas();
                    });

                    // tell server to spawn safe boxes
                    JSONObject j = new JSONObject();
                    j.AddField("bluex", blueSafeBoxContainer.transform.position.x);
                    j.AddField("bluey", blueSafeBoxContainer.transform.position.y);
                    j.AddField("orangex", orangeSafeBoxContainer.transform.position.x);
                    j.AddField("orangey", orangeSafeBoxContainer.transform.position.y);
                    Emit("initSafeBoxes", j);
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

            On("updateTime", (E) => {
                string time = E.data["time"].str;
                InGameUIManager.Instance.setTime(time);
            });

            On("spawnPlayers", (E) => {
                int playerNum = E.data["playersInfo"].list.Count;
                JSONObject playersInfo = E.data["playersInfo"];

                for (int i = 0; i < playerNum; ++i) {
                    string id = playersInfo[i]["id"].RemoveQuotes();
                    GameObject go = Instantiate(tankIDtoPrefab[playersInfo[i]["tank"].i()]
                                                , positionIDToContainer[playersInfo[i]["startPosition"].i()]);
                    // initialize my aiming line
                    if (id == ClientID) {
                        Instantiate(networkPrefabs.aimingLine, go.GetComponent<PlayerManager>().getBarrelPivot());
                        Instantiate(networkPrefabs.guideArrow, go.gameObject.transform);
                    }

                    // if I am orange team, rotate the camera and tank
                    string team = playersInfo[i]["team"].str;
                    if (id == ClientID && team == "orange") {
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
                    go.name = playersInfo[i]["username"].str;
                    NetworkIdentity ni = go.GetComponent<NetworkIdentity>();
                    ni.SetControllerID(id);
                    ni.SetSocketReference(this);
                    ni.SetNiType("Tank");
                    ni.SetNiTeam(team);
                    serverObjects.Add(id, ni);

                    // initialize all players' info
                    PlayerManager pm = go.GetComponent<PlayerManager>();
                    pm.setInfo(playersInfo[i]);

                    // initialize special status list
                    specialStatus[id] = new List<KeyValuePair<string, GameObject>>();

                    // map all players' id to status bar index
                    playerIDtoStatusBarIndex.Add(id, i);
                    // update other players' status bar username
                    if (id != ClientID) {
                        InGameUIManager.Instance.toggleStatusBar(i);
                        InGameUIManager.Instance.updateStatusBarUsername(i, go.name);
                    }
                }

                // initialize my status bar, rotate camera if orange team
                PlayerManager p = serverObjects[ClientID].GetComponent<PlayerManager>();
                InGameUIManager.Instance.updateHealthBar(p.getFullHealth(), p.getHealth());
                InGameUIManager.Instance.updateMagicBar(p.getFullMp(), p.getMp());

                // remove all teamamates' collider
                for (int i = 0; i < playerNum; ++i) {
                    string id = playersInfo[i]["id"].RemoveQuotes();
                    if (playersInfo[i]["team"].str == serverObjects[ClientID].GetNiTeam()
                        && id != ClientID) {
                        Destroy(serverObjects[id].GetComponent<Rigidbody2D>());
                        Destroy(serverObjects[id].GetComponent<CircleCollider2D>());
                    }
                }
            });

            On("spawnGameObject", (E) => {
                string name = E.data["name"].str;
                string id = E.data["id"].RemoveQuotes();
                float x = E.data["position"]["x"].f;
                float y = E.data["position"]["y"].f;
                Debug.LogFormat("Server wants us to spawn a '{0}' at {1}, {2}", name, x, y);

                if (!serverObjects.ContainsKey(id)) {
                    NetworkIdentity ni;
                    if (name == "Bullet") {
                        ServerObjectData sod = serverSpawnables.GetObjectByName(name);
                        // instantiate bullet position with random container because position would be updated immediately
                        var spawnedObject = Instantiate(sod.Prefab, outSideContainer);
                        spawnedObject.transform.position = new Vector3(x, y, 0);
                        ni = spawnedObject.GetComponent<NetworkIdentity>();
                        ni.SetControllerID(id);
                        ni.SetSocketReference(this);
                        ni.SetNiType(name);
                        string activator = E.data["activator"].str;
                        ni.SetNiTeam(serverObjects[activator].GetComponent<PlayerManager>().getTeam());
                        // set bullet's activator in collisionDestroy
                        ni.GetComponent<CollisionDestroy>().setActivator(activator);

                        // if this bullet is spanwed by teammates, remove it's collider
                        if (ni.GetNiTeam() == serverObjects[ClientID].GetNiTeam() && activator != ClientID) {
                            Destroy(ni.GetComponent<Rigidbody2D>());
                            Destroy(ni.GetComponent<CircleCollider2D>());
                        }
                        serverObjects.Add(id, ni);

                        float directionX = E.data["direction"]["x"].f;
                        float directionY = E.data["direction"]["y"].f;
                        float speed = E.data["speed"].f;
                        float rot = Mathf.Atan2(directionY, directionX) * Mathf.Rad2Deg;
                        Vector3 currentRotation = new Vector3(0, 0, rot - 90);
                        spawnedObject.transform.rotation = Quaternion.Euler(currentRotation);

                        Projectile projectile = spawnedObject.GetComponent<Projectile>();
                        projectile.Direction = new Vector2(directionX, directionY);
                        projectile.Speed = speed;
                    }
                    else if (name == "SafeBox") {
                        GameObject go;
                        string team = E.data["team"].str;
                        if (team == "blue") {
                            go = Instantiate(networkPrefabs.blueSafeBoxPrefab, blueSafeBoxContainer);
                        }
                        else {
                            go = Instantiate(networkPrefabs.orangeSafeBoxPrefab, orangeSafeBoxContainer);
                        }
                        go.name = string.Format("{0} safe box", team);
                        ni = go.GetComponent<NetworkIdentity>();
                        ni.SetControllerID(id);
                        ni.SetSocketReference(this);
                        ni.SetNiType(name);
                        ni.SetNiTeam(team);
                        serverObjects.Add(id, ni);
                        InGameUIManager.Instance.toggleSafeBoxHealthBar(team);
                        InGameUIManager.Instance.setSafeBoxHealthBarPosition(team, x, y);
                    }
                    else if (name == "PortalPair") {
                        string team = E.data["team"].str;
                        string id2 = E.data["id2"].RemoveQuotes();
                        float x2 = E.data["position2"]["x"].f;
                        float y2 = E.data["position2"]["y"].f;
                        // Instantiate two portals
                        GameObject portal = Instantiate(networkPrefabs.superPortal, outSideContainer);
                        GameObject portal2 = Instantiate(networkPrefabs.superPortal, outSideContainer);
                        portal.transform.position = new Vector3(x, y, 0);
                        portal2.transform.position = new Vector3(x2, y2, 0);
                        portal.name = string.Format("Portal {0}", id);
                        portal2.name = string.Format("Portal {0}", id2);
                        ni = portal.GetComponent<NetworkIdentity>();
                        ni.SetNiType("Portal");
                        ni.SetNiTeam(team);
                        serverObjects.Add(id, ni);
                        ni = portal2.GetComponent<NetworkIdentity>();
                        ni.SetNiType("Portal");
                        ni.SetNiTeam(team);
                        serverObjects.Add(id2, ni);
                        // set portal pair
                        portal.GetComponent<PortalCollision>().pairedPortalID = id2;
                        portal2.GetComponent<PortalCollision>().pairedPortalID = id;
                    }
                    else {
                        Debug.LogError("undefined object name");
                    }
                }
            });

            On("unspawnGameObject", (E) => {
                string id = E.data["id"].RemoveQuotes();
                NetworkIdentity ni = serverObjects[id];
                serverObjects.Remove(id);
                DestroyImmediate(ni.gameObject);
            });

            On("setPlayerHealth", (E) => {
                string id = E.data["id"].RemoveQuotes();
                NetworkIdentity ni = serverObjects[id];
                float health = E.data["health"].f;

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

            On("setPlayerSpeed", (E) => {
                float speed = E.data["speed"].f;
                serverObjects[ClientID].GetComponent<PlayerManager>().setSpeed(speed);
            });

            On("setSafeBoxHealth", (E) => {
                string team = E.data["team"].str;
                float health = E.data["health"].f;
                float fullHealth = E.data["fullHealth"].f;
                InGameUIManager.Instance.updateSafeBoxHealth(team, fullHealth, health);
            });

            On("playerDied", (E) => {
                string id = E.data["id"].RemoveQuotes();
                NetworkIdentity ni = serverObjects[id];
                ni.gameObject.SetActive(false);

                // clear all special status
                RemoveSpecialStatus.removeAllStatus(id, this);

                // hide status bar
                if (id != ClientID) {
                    InGameUIManager.Instance.toggleStatusBar(playerIDtoStatusBarIndex[id]);
                }
            });

            On("playerRespawn", (E) => {
                string id = E.data["id"].RemoveQuotes();
                NetworkIdentity ni = serverObjects[id];
                ni.transform.position = positionIDToContainer[E.data["startPosition"].i()].position;
                ni.gameObject.SetActive(true);
                // rotate back player
                ni.transform.eulerAngles = new Vector3(0, 0, (ni.GetNiTeam() == "blue" ? 0 : 180));

                PlayerManager pm = ni.GetComponent<PlayerManager>();

                // update my health bar and others' status bar
                if (id == ClientID) {
                    pm.setHealth(pm.getFullHealth());
                    InGameUIManager.Instance.updateHealthBar(pm.getFullHealth(), pm.getHealth());
                }
                else {
                    pm.setHealth(pm.getFullHealth());
                    InGameUIManager.Instance.updateStatusBarHealth(playerIDtoStatusBarIndex[id]
                                                                   , pm.getFullHealth(), pm.getHealth());
                    InGameUIManager.Instance.Update();
                    // show status bar
                    InGameUIManager.Instance.toggleStatusBar(playerIDtoStatusBarIndex[id]);
                }
            });

            On("updateBulletNum", (E) => {
                float bulletNum = E.data["bulletNum"].f;
                InGameUIManager.Instance.updateBulletNum(bulletNum);
            });

            On("updateMp", (E) => {
                float mp = E.data["mp"].f;
                float fullMp = E.data["fullMp"].f;
                serverObjects[ClientID].GetComponent<PlayerManager>().setMp(mp);
                InGameUIManager.Instance.updateMagicBar(fullMp, mp);
            });

            On("safeBoxExplode", (E) => {
                string explodeSafeBoxID = E.data["explodeSafeBoxID"].str;

                // move camera to exploded safe box
                // disable player managers first
                foreach (KeyValuePair<string, UserInGameRoom> item in usersInGameRoom) {
                    string id = item.Value.id;
                    serverObjects[id].GetComponent<PlayerManager>().enabled = false;
                }
                Transform explodeSafeBoxContainer;
                explodeSafeBoxContainer = (serverObjects[explodeSafeBoxID].GetNiTeam() == "blue"
                                           ? blueSafeBoxContainer : orangeSafeBoxContainer);
                serverObjects[ClientID].setControlling(false);
                cameraForMoving.transform.position = Camera.main.transform.position;
                cameraForMoving.transform.rotation = Camera.main.transform.rotation;
                cameraForMoving.enabled = true;
                cameraForMoving.GetComponent<MoveAnimation>().SetDestination(explodeSafeBoxContainer.position, 1);
                StartCoroutine(explodeWaiter(explodeSafeBoxContainer, explodeSafeBoxID));
            });

            On("gameOver", (E) => {
                string winTeam = E.data["winTeam"].str;
                Debug.Log(string.Format("Game over. Win team: {0}", winTeam));

                // clean up
                InGameUIManager.Instance.CleanUp();
                playerIDtoStatusBarIndex.Clear();
                List<string> keysToDelete = new List<string>();
                // clean server objects
                foreach (KeyValuePair<string, NetworkIdentity> item in serverObjects) {
                    // retain win team players
                    if (item.Value.GetNiType() == "Tank" && item.Value.GetNiTeam() == winTeam) {

                    }
                    else {
                        Destroy(item.Value.gameObject);
                        keysToDelete.Add(item.Key);
                    }
                }
                foreach (string key in keysToDelete) {
                    serverObjects.Remove(key);
                }
                // clean sand storm
                Camera.main.GetComponent<D2FogsPE>().enabled = false;
                sandStormStatus.Clear();

                Debug.Log("Switching to GameOver");
                SceneManagementManager.Instance.LoadLevel(SceneList.GAMEOVER, (levelName) => {
                    // disable camera for moving
                    cameraForMoving.enabled = false;
                    // reset main camera rotation
                    Transform cameraTransform = Camera.main.transform;
                    cameraTransform.eulerAngles = new Vector3(
                        cameraTransform.eulerAngles.x,
                        cameraTransform.eulerAngles.y,
                        0
                    );

                    if (currentGameMode == "Heist") {
                        Debug.Log("Unload HEIST scene");
                        SceneManagementManager.Instance.UnLoadLevel(SceneList.HEIST);
                    }
                    else if (currentGameMode == "Showdown") {
                        Debug.Log("Unload SHOWDOWN scene");
                        SceneManagementManager.Instance.UnLoadLevel(SceneList.SHOWDOWN);
                    }
                    else {
                        Debug.LogError("undefined current game mode");
                    }
                });
            });

            On("useSuper", (E) => {
                string id = E.data["id"].RemoveQuotes();
                string team = E.data["team"].str;
                string super = E.data["super"].str;

                Debug.Log("Server request " + id + " cast super: " + super);
                GameObject effectPrefab;
                switch (super) {
                    case "freeze":
                        foreach (KeyValuePair<string, UserInGameRoom> item in usersInGameRoom) {
                            if (item.Value.team != team && serverObjects[item.Key].gameObject.activeSelf) {
                                effectPrefab = Instantiate(networkPrefabs.superFreeze, serverObjects[item.Key].transform);
                                if (item.Key == ClientID) {
                                    serverObjects[ClientID].setControlling(false);
                                }

                                specialStatus[item.Key].Add(new KeyValuePair<string, GameObject>("freeze", effectPrefab));
                                StartCoroutine(removeStatusWaiter(5, item.Key, "freeze"));
                            }
                        }
                        break;

                    case "lifeTree":
                        foreach (KeyValuePair<string, UserInGameRoom> item in usersInGameRoom) {
                            if (item.Value.team == team) {
                                effectPrefab = Instantiate(networkPrefabs.superLifeTree, serverObjects[item.Key].transform);

                                specialStatus[item.Key].Add(new KeyValuePair<string, GameObject>("lifeTree", effectPrefab));
                                StartCoroutine(removeStatusWaiter(5, item.Key, "lifeTree"));
                            }
                        }
                        break;

                    case "portal":
                        break;

                    case "lightShield":
                        effectPrefab = Instantiate(networkPrefabs.superLightShield, serverObjects[id].transform);
                        specialStatus[id].Add(new KeyValuePair<string, GameObject>("lightShield", effectPrefab));
                        StartCoroutine(removeStatusWaiter(6, id, "lightShield"));
                        break;

                    case "sandStorm":
                        D2FogsPE sandStormEffect = Camera.main.GetComponent<D2FogsPE>();
                        sandStormEffect.enabled = true;
                        if (serverObjects[ClientID].GetNiTeam() == team) {
                            // small density
                            sandStormStatus.Add("s");
                            bool existsLarge = sandStormStatus.Exists(x => x == "l");
                            if (!existsLarge) {
                                sandStormEffect.Density = 0.8f;
                            }
                            StartCoroutine(removeSandStormWaiter(15, "s"));
                        }
                        else {
                            // large density
                            sandStormStatus.Add("l");
                            sandStormEffect.Density = 1.5f;
                            StartCoroutine(removeSandStormWaiter(15, "l"));
                        }
                        break;

                    case "fireBall":
                        Transform bulletSpawnPoint = serverObjects[id].GetComponent<PlayerManager>().getBulletSpawnPoint();
                        GameObject fireBall = Instantiate(networkPrefabs.superFireBall, outSideContainer);
                        fireBall.GetComponent<FireBallCollision>().activator = id;
                        fireBall.GetComponent<FireBallCollision>().setSocket(this);
                        float directionX = bulletSpawnPoint.up.x;
                        float directionY = bulletSpawnPoint.up.y;
                        float speed = 0.5f;
                        float rot = Mathf.Atan2(directionY, directionX) * Mathf.Rad2Deg;
                        Vector3 currentRotation = new Vector3(0, 0, rot - 90);
                        fireBall.transform.position = new Vector3(
                            bulletSpawnPoint.position.x + 0.8f * directionX,
                            bulletSpawnPoint.position.y + 0.8f * directionY,
                            0
                        );
                        fireBall.transform.rotation = Quaternion.Euler(currentRotation);

                        StartCoroutine(fireBallWaiter(1, fireBall, directionX, directionY, speed));

                        // if this bullet is spanwed by teammates, remove it's collider
                        /*if (ni.GetNiTeam() == serverObjects[ClientID].GetNiTeam() && activator != ClientID) {
                            Destroy(ni.GetComponent<Rigidbody2D>());
                            Destroy(ni.GetComponent<CircleCollider2D>());
                        }*/
                        break;

                    default:
                        Debug.LogError("WTF is this super");
                        break;
                }
            });
        }

        private IEnumerator explodeWaiter(Transform explodeSafeBoxContainer, string safeBoxID) {
            yield return new WaitForSeconds(1);
            // explode animation
            GameObject explodeGO = Instantiate(networkPrefabs.explosionSafeBox, explodeSafeBoxContainer);
            serverObjects[safeBoxID].GetComponent<SpriteRenderer>().sprite = deadSafeBox;
            yield return new WaitForSeconds(1);
            Destroy(explodeGO);
        }
        private IEnumerator removeStatusWaiter(float time, string id, string status) {
            yield return new WaitForSeconds(time);
            RemoveSpecialStatus.removeSpecialStatus(id, status, this);
        }
        private IEnumerator removeSandStormWaiter(float time, string size) {
            yield return new WaitForSeconds(time);
            sandStormStatus.Remove(size);
            D2FogsPE sandStormEffect = Camera.main.GetComponent<D2FogsPE>();
            bool existsLarge = sandStormStatus.Exists(x => x == "l");
            if (!existsLarge) {
                bool existsSmall = sandStormStatus.Exists(x => x == "s");
                if (existsSmall) {
                    sandStormEffect.Density = 0.8f;
                }
                else {
                    sandStormEffect.enabled = false;
                }
            }
        }
        private IEnumerator fireBallWaiter(float time, GameObject fireBall, float x, float y, float speed) {
            yield return new WaitForSeconds(time);
            Projectile projectile = fireBall.GetComponent<Projectile>();
            projectile.Direction = new Vector2(x, y);
            projectile.Speed = speed;
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
