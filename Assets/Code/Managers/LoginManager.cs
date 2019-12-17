using System;
using System.Collections;
using System.Collections.Generic;
using Project.Networking;
using Project.Utility;
using SocketIO;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Managers {
    public class LoginManager : MonoBehaviour {

        [SerializeField]
        public InputField inputField;

        private SocketIOComponent socketReference;

        public SocketIOComponent SocketReference {
            get {
                return socketReference = (socketReference == null) ? FindObjectOfType<NetworkClient>() : socketReference;
            }
        }

        public void Start() {
            inputField.interactable = false;

            SceneManagementManager.Instance.LoadLevel(SceneList.ONLINE, (levelName) => {
                inputField.interactable = true;
            });
        }

        public void Update() {
            if (inputField.text != "" && Input.GetKeyDown(KeyCode.Return)) {
                UserData userData = new UserData();
                userData.username = inputField.text;
                SocketReference.Emit("joinBaseLobby", new JSONObject(JsonUtility.ToJson(userData)));
                //SocketReference.Emit("joinGame", new JSONObject(JsonUtility.ToJson(userData)));
                inputField.text = "";
            }
        }
    }

    [Serializable]
    public class UserData {
        public string username;
    }
}