using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using UnityEngine.UI;
using Project.Networking;
using System;

namespace Project.Managers {
    public class LobbyManager : MonoBehaviour {

        [SerializeField]
        private Button showdownButton;
        [SerializeField]
        private Button heistButton;

        [SerializeField]
        private string gameMode;
        [SerializeField]
        private string capacity;

        [SerializeField]
        private Text GameRoomID_1;
        [SerializeField]
        private Text GameMode_1;
        [SerializeField]
        private Text Status_1;
        [SerializeField]
        private Text Empty_1;

        [SerializeField]
        private Text GameRoomID_2;
        [SerializeField]
        private Text GameMode_2;
        [SerializeField]
        private Text Status_2;
        [SerializeField]
        private Text Empty_2;

        [SerializeField]
        private Text GameRoomID_3;
        [SerializeField]
        private Text GameMode_3;
        [SerializeField]
        private Text Status_3;
        [SerializeField]
        private Text Empty_3;

        [SerializeField]
        private Text GameRoomID_4;
        [SerializeField]
        private Text GameMode_4;
        [SerializeField]
        private Text Status_4;
        [SerializeField]
        private Text Empty_4;

        public InputField inputField;

        private SocketIOComponent socketReference;

        public SocketIOComponent SocketReference {
            get {
                return socketReference = (socketReference == null) ? FindObjectOfType<NetworkClient>() : socketReference;
            }
        }

        void Update() {
            capacity = inputField.text;

            //Debug.Log(message: rooms);
            int count = 0;

            foreach (KeyValuePair<int, GameRoomInfo> item in NetworkClient.gameRoomsInfo) {

                if (count > 3) {
                    break;
                }

                if (item.Value.player_num >= 1) {
                    count++;
                    if (count == 1) {
                        GameRoomID_1.text = item.Value.id.ToString();
                        GameMode_1.text = item.Value.gameMode;
                        Empty_1.text = "";
                        if (item.Value.playing) {
                            Status_1.text = "Playing";
                        } else {
                            Status_1.text = item.Value.player_num.ToString() + " / " + item.Value.maxPlayers.ToString();
                        }
                    } else if (count == 2) {
                        GameRoomID_2.text = item.Value.id.ToString();
                        GameMode_2.text = item.Value.gameMode;
                        Empty_2.text = "";
                        if (item.Value.playing) {
                            Status_2.text = "Playing";
                        } else {
                            Status_2.text = item.Value.player_num.ToString() + " / " + item.Value.maxPlayers.ToString();
                        }
                    } else if (count == 3) {
                        GameRoomID_3.text = item.Value.id.ToString();
                        GameMode_3.text = item.Value.gameMode;
                        Empty_3.text = "";
                        if (item.Value.playing) {
                            Status_3.text = "Playing";
                        } else {
                            Status_3.text = item.Value.player_num.ToString() + " / " + item.Value.maxPlayers.ToString();
                        }
                    } else if (count == 4) {
                        GameRoomID_4.text = item.Value.id.ToString();
                        GameMode_4.text = item.Value.gameMode;
                        Empty_4.text = "";
                        if (item.Value.playing) {
                            Status_4.text = "Playing";
                        } else {
                            Status_4.text = item.Value.player_num.ToString() + " / " + item.Value.maxPlayers.ToString();
                        }
                    }
                }
            }

            if (count < 4) {
                GameRoomID_4.text = "";
                GameMode_4.text = "";
                Status_4.text = "";
                Empty_4.text = "Empty";
            }

            if (count < 3) {
                GameRoomID_3.text = "";
                GameMode_3.text = "";
                Status_3.text = "";
                Empty_3.text = "Empty";
            }

            if (count < 2) {
                GameRoomID_2.text = "";
                GameMode_2.text = "";
                Status_2.text = "";
                Empty_2.text = "Empty";
            }

            if (count < 1) {
                GameRoomID_1.text = "";
                GameMode_1.text = "";
                Status_1.text = "";
                Empty_1.text = "Empty";
            }
        }

        public void PressShowdownButton() {
            gameMode = "Showdown";
            Color color = new Color();
            ColorUtility.TryParseHtmlString("#F8FD00", out color);
            showdownButton.image.color = color;
            ColorUtility.TryParseHtmlString("#FF8C69", out color);
            heistButton.image.color = color;
        }

        public void PressHeistButton() {
            gameMode = "Heist";
            Color color = new Color();
            ColorUtility.TryParseHtmlString("#FF8C69", out color);
            showdownButton.image.color = color;
            ColorUtility.TryParseHtmlString("#F8FD00", out color);
            heistButton.image.color = color;
        }

        public void PressCreateButton() {
            int number;
            if ((gameMode == "Showdown" || gameMode == "Heist") && int.TryParse(capacity, out number)) {
                if (number >= 2 && number <= 6) {
                    GameSettings gameSettings = new GameSettings();
                    gameSettings.maxPlayers = number;
                    gameSettings.gameMode = gameMode;
                    SocketReference.Emit("createGameRoom", new JSONObject(JsonUtility.ToJson(gameSettings)));
                }
            }
        }

        public void EnterGameRoom1() {
            if (GameRoomID_1.text == "") { return; }
            int id = int.Parse(GameRoomID_1.text);
            if (!NetworkClient.gameRoomsInfo[id].playing && NetworkClient.gameRoomsInfo[id].player_num < NetworkClient.gameRoomsInfo[id].maxPlayers) {
                GameRoomID gameRoomID = new GameRoomID();
                gameRoomID.id = id;
                SocketReference.Emit("joinGameRoom", new JSONObject(JsonUtility.ToJson(gameRoomID)));
            }
        }

        public void EnterGameRoom2() {
            if (GameRoomID_2.text == "") { return; }
            int id = int.Parse(GameRoomID_2.text);
            if (!NetworkClient.gameRoomsInfo[id].playing && NetworkClient.gameRoomsInfo[id].player_num < NetworkClient.gameRoomsInfo[id].maxPlayers) {
                GameRoomID gameRoomID = new GameRoomID();
                gameRoomID.id = id;
                SocketReference.Emit("joinGameRoom", new JSONObject(JsonUtility.ToJson(gameRoomID)));
            }
        }

        public void EnterGameRoom3() {
            if (GameRoomID_3.text == "") { return; }
            int id = int.Parse(GameRoomID_3.text);
            if (!NetworkClient.gameRoomsInfo[id].playing && NetworkClient.gameRoomsInfo[id].player_num < NetworkClient.gameRoomsInfo[id].maxPlayers) {
                GameRoomID gameRoomID = new GameRoomID();
                gameRoomID.id = id;
                SocketReference.Emit("joinGameRoom", new JSONObject(JsonUtility.ToJson(gameRoomID)));
            }
        }

        public void EnterGameRoom4() {
            if (GameRoomID_4.text == "") { return; }
            int id = int.Parse(GameRoomID_4.text);
            if (!NetworkClient.gameRoomsInfo[id].playing && NetworkClient.gameRoomsInfo[id].player_num < NetworkClient.gameRoomsInfo[id].maxPlayers) {
                GameRoomID gameRoomID = new GameRoomID();
                gameRoomID.id = id;
                SocketReference.Emit("joinGameRoom", new JSONObject(JsonUtility.ToJson(gameRoomID)));
            }
        }
    }

    [Serializable]
    public class GameSettings {
        public int maxPlayers;
        public string gameMode;
    }

    [Serializable]
    public class GameRoomID {
        public int id;
    }

}
