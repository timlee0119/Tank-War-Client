using System.Collections;
using System.Collections.Generic;
using Project.Networking;
using UnityEngine;

namespace Project.Gameplay {

    public class CollisionDestroy : MonoBehaviour {

        [SerializeField]
        private NetworkIdentity networkIdentity;

        public void OnCollisionEnter2D(Collision2D collision) {
            NetworkIdentity ni = collision.gameObject.GetComponent<NetworkIdentity>();
            // if ni == null: wall
            // ni.GetNiTeam(): collision object's team; this.networkIdentity.GetNiTeam(): my activator's team
            if (ni == null || ni.GetNiTeam() != this.networkIdentity.GetNiTeam() || ni.GetNiType() == "SafeBox") {
                JSONObject j = new JSONObject();
                j.AddField("bulletID", networkIdentity.GetID());
                string hitObjectID = (ni == null) ? "" : ni.GetID();
                j.AddField("hitObjectID", hitObjectID);
                string hitObjectType = (ni == null) ? "" : ni.GetNiType();
                j.AddField("hitObjectType", hitObjectType);
                networkIdentity.GetSocket().Emit("collisionDestroy", j);
            }
        }
    }
}
