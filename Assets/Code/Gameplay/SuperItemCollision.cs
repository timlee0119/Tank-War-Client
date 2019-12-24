using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.Networking;
using Project.Player;

namespace Project.Gameplay {
    public class SuperItemCollision : MonoBehaviour {
        [SerializeField]
        NetworkIdentity networkIdentity;

        public int superID;

        public void OnCollisionEnter2D(Collision2D collision) {
            NetworkIdentity ni = collision.gameObject.GetComponent<NetworkIdentity>();
            if (ni != null && ni.GetID() == NetworkClient.ClientID) {
                ni.GetComponent<PlayerManager>().setObtainedSuper(superID);
                networkIdentity.GetSocket().Emit("superItemCollision");
            }
        }
    }
}
