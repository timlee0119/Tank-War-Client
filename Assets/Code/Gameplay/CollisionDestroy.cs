using System.Collections;
using System.Collections.Generic;
using Project.Networking;
using UnityEngine;

namespace Project.Gameplay {

    public class CollisionDestroy : MonoBehaviour {

        [SerializeField]
        private NetworkIdentity networkIdentity;
        private string activator;

        public void setActivator(string activator) { this.activator = activator; }

        public void OnCollisionEnter2D(Collision2D collision) {
            NetworkIdentity ni = collision.gameObject.GetComponent<NetworkIdentity>();

            // when collide with the other object, if I'm not the activator, don't emit to server
            if (activator != NetworkClient.ClientID) { return; }

            // if ni == null: wall
            // ni.GetNiTeam(): collision object's team; this.networkIdentity.GetNiTeam(): my activator's team
            if (ni == null || ni.GetNiTeam() != this.networkIdentity.GetNiTeam() 
                || ni.GetNiType() == "SafeBox" || ni.GetNiType() == "Portal") {
                JSONObject j = new JSONObject();
                j.AddField("bulletID", networkIdentity.GetID());
                string hitObjectID = (ni == null) ? "" : ni.GetID();
                j.AddField("hitObjectID", hitObjectID);
                string hitObjectType = (ni == null) ? "" : ni.GetNiType();
                j.AddField("hitObjectType", hitObjectType);

                if (ni != null && ni.GetNiType() == "Tank") {
                    ni.GetComponent<AudioSource>().Play();
                }
                networkIdentity.GetSocket().Emit("collisionDestroy", j);
            }
        }
    }
}
