using Project.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Gameplay {
    public class PortalCollision : MonoBehaviour {
        [SerializeField]
        private NetworkIdentity networkIdentity;

        public string pairedPortalID;

        public void OnCollisionEnter2D(Collision2D collision) {
            NetworkIdentity ni = collision.gameObject.GetComponent<NetworkIdentity>();
            if (ni.GetID() == NetworkClient.ClientID && ni.GetNiTeam() == networkIdentity.GetNiTeam()) {
                float xOffset = transform.position.x - ni.transform.position.x;
                float yOffset = transform.position.y - ni.transform.position.y;
                ni.transform.position = new Vector3(
                    NetworkClient.serverObjects[pairedPortalID].transform.position.x + xOffset,
                    NetworkClient.serverObjects[pairedPortalID].transform.position.y + yOffset,
                    0
                );
            }
        }
    }
}
