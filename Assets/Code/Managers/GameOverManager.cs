using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Project.Networking;
using SocketIO;
using Project.Utility;

namespace Project.Managers {

    public class GameOverManager : MonoBehaviour {
        [SerializeField]
        private Button toGameRoomButton;

        private float timer = 0;

        private SocketIOComponent socketReference;
        public SocketIOComponent SocketReference {
            get {
                return socketReference = (socketReference == null) ? FindObjectOfType<NetworkClient>() : socketReference;
            }
        }

        private void Update() {
            timer += Time.deltaTime;
            if (timer >= 8) {
                backToGameRoom();
            }
        }

        public void backToGameRoom() {
            foreach (KeyValuePair<string, NetworkIdentity> item in NetworkClient.serverObjects) {
                Destroy(item.Value.gameObject);
            }
            NetworkClient.serverObjects.Clear();

            SceneManagementManager.Instance.LoadLevel(SceneList.GAMEROOM, (levelName) => {
                SceneManagementManager.Instance.UnLoadLevel(SceneList.GAMEOVER);
            });
        }
    }
}
