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
        [SerializeField]
        private Image howToPlay;
        [SerializeField]
        private Image btnImg;
        [SerializeField]
        private Text btnText;
        [SerializeField]
        private Text pressEnter;
        [SerializeField]
        private GameObject clickPlayer;

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

        public void PressHowToPlay() {
            clickPlayer.GetComponent<AudioSource>().Play();
            howToPlay.enabled = true;
            btnImg.enabled = true;
            btnText.enabled = true;
            pressEnter.enabled = false;
        }

        public void PressClose() {
            clickPlayer.GetComponent<AudioSource>().Play();
            howToPlay.enabled = false;
            btnImg.enabled = false;
            btnText.enabled = false;
            pressEnter.enabled = true;
        }
    }

    [Serializable]
    public class UserData {
        public string username;
    }
}