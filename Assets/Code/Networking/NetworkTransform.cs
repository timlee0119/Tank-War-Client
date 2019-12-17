using System.Collections;
using System.Collections.Generic;
using Project.Utility.Attributes;
using UnityEngine;

namespace Project.Networking {
    [RequireComponent(typeof(NetworkIdentity))]
    public class NetworkTransform : MonoBehaviour {

        [SerializeField]
        [GreyOut]
        private Vector3 oldPosition;

        private NetworkIdentity networkIdentity;
        private Player player;

        private float stillCounter = 0;

        public void Start() {
            networkIdentity = GetComponent<NetworkIdentity>();
            oldPosition = transform.position;
            player = new Player();
            player.position = new Position();
            player.position.x = 0;
            player.position.y = 0;

            if (!networkIdentity.IsControlling()) {
                enabled = false;
            }
        }

        public void Update() {
            if (networkIdentity.IsControlling()) {
                if (oldPosition != transform.position) {
                    oldPosition = transform.position;
                    stillCounter = 0;
                    sendData();
                } else {
                    stillCounter += Time.deltaTime;

                    if (stillCounter >= 1) {
                        stillCounter = 0;
                        sendData();
                    }
                }
            }
        }

        private void sendData() {
            // Update player information
            player.position.x = Mathf.Round(transform.position.x * 1000.0f) / 1000.0f;
            player.position.y = Mathf.Round(transform.position.y * 1000.0f) / 1000.0f;

            networkIdentity.GetSocket().Emit("updatePosition", new JSONObject(JsonUtility.ToJson(player)));
        }
    }
}
