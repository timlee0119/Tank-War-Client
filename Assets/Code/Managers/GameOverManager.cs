﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Project.Networking;
using SocketIO;
using Project.Utility;
using Project.Player;

namespace Project.Managers {

    public class GameOverManager : MonoBehaviour {
        [SerializeField]
        AudioClip menuBackground;
        [SerializeField]
        private Transform[] winnerPositions;
        [SerializeField]
        private Text[] winnerNames;

        private float timer = 0;
        private bool timeout = false;

        private SocketIOComponent socketReference;
        public SocketIOComponent SocketReference {
            get {
                return socketReference = (socketReference == null) ? FindObjectOfType<NetworkClient>() : socketReference;
            }
        }

        private void Start() {
            int i = 0;
            foreach (KeyValuePair<string, NetworkIdentity> item in NetworkClient.serverObjects) {
                int tankID = item.Value.GetComponent<PlayerManager>().getTankID();
                GameObject go = Instantiate(NetworkClient.tankIDtoPrefab[tankID], winnerPositions[i], false);
                Destroy(go.GetComponent<Rigidbody2D>());
                Destroy(go.GetComponent<CircleCollider2D>());
                Destroy(go.GetComponent<PlayerManager>());
                winnerNames[i].text = item.Value.GetComponent<PlayerManager>().getUsername();

                Destroy(item.Value.gameObject);
                i++;
            }
            NetworkClient.serverObjects.Clear();
        }

        private void Update() {
            if (!timeout) {
                timer += Time.deltaTime;
                if (timer >= 8) {
                    backToGameRoom();
                    timeout = true;
                }
            }
        }

        public void backToGameRoom() {
            AudioSource audio = Camera.main.GetComponent<AudioSource>();
            audio.clip = menuBackground;
            audio.Play();
            SceneManagementManager.Instance.LoadLevel(SceneList.GAMEROOM, (levelName) => {
                SocketReference.Emit("finishPlaying");
                SceneManagementManager.Instance.UnLoadLevel(SceneList.GAMEOVER);
            });
        }
    }
}
