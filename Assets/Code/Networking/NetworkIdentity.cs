using System.Collections;
using System.Collections.Generic;
using Project.Utility.Attributes;
using UnityEngine;
using SocketIO;

namespace Project.Networking {
    [RequireComponent(typeof(NetworkPrefabs))]
    public class NetworkIdentity : MonoBehaviour {

        [Header("Helpful Values")]
        [SerializeField]
        [GreyOut]
        private string id;
        [SerializeField]
        [GreyOut]
        private bool isControlling;

        private string niTeam;
        private string niType;

        private SocketIOComponent socket;

        public void Awake() {
            isControlling = false;
        }

        public void SetControllerID(string ID) {
            id = ID;
            // Check incoming id versuses the one we haved saved from the server
            isControlling = (NetworkClient.ClientID == ID) ? true : false;
        }

        public void SetSocketReference(SocketIOComponent Socket) {
            socket = Socket;
        }

        public string GetID() {
            return id;
        }

        public void SetNiType(string type) { this.niType = type; }
        public string GetNiType() { return niType; }
        public void SetNiTeam(string team) { this.niTeam = team; }
        public string GetNiTeam() { return niTeam; }

        public bool IsControlling() {
            return isControlling;
        }
        public void setControlling(bool b) {
            isControlling = b;
        }

        public SocketIOComponent GetSocket() {
            return socket;
        }
    }
}
