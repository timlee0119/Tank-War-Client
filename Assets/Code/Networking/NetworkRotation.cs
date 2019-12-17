﻿using System.Collections;
using System.Collections.Generic;
using Project.Player;
using Project.Utility.Attributes;
using UnityEngine;

namespace Project.Networking {
    [RequireComponent(typeof(NetworkIdentity))]
    public class NetworkRotation : MonoBehaviour {

        [Header("Referenced Values")]
        [SerializeField]
        [GreyOut]
        private float oldTankRotation;
        [SerializeField]
        [GreyOut]
        private float oldBarrelRotation;

        [Header("Class References")]
        [SerializeField]
        private PlayerManager playerManager;

        private NetworkIdentity networkIdentity;
        private PlayerRotation player;

        private float stillCounter = 0;

        public void Start() {
            networkIdentity = GetComponent<NetworkIdentity>();

            player = new PlayerRotation();
            player.tankRotation = 0;
            player.barrelRotation = 0;

            if (!networkIdentity.IsControlling()) {
                enabled = false;
            }
        }

        public void Update() {
            if(networkIdentity.IsControlling()) {
                if (oldTankRotation != transform.localEulerAngles.z || oldBarrelRotation != playerManager.GetLastRotation()) {
                    oldTankRotation = transform.localEulerAngles.z;
                    oldBarrelRotation = playerManager.GetLastRotation();
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
            player.tankRotation = Mathf.Round(transform.localEulerAngles.z * 1000.0f) / 1000.0f;
            player.barrelRotation = Mathf.Round(playerManager.GetLastRotation() * 1000.0f) / 1000.0f;

            networkIdentity.GetSocket().Emit("updateRotation", new JSONObject(JsonUtility.ToJson(player)));
        }
    }
}
