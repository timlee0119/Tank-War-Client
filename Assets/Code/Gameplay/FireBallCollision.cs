using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.Networking;
using SocketIO;

namespace Project.Gameplay {
    public class FireBallCollision : MonoBehaviour {
        [SerializeField]
        GameObject explosionFireBall;

        [SerializeField]
        AudioClip fireballExplosionBackground;

        private SocketIOComponent socketReference;
        public void setSocket(SocketIOComponent s) { socketReference = s; }

        public string activator;

        public void OnCollisionEnter2D(Collision2D collision) {
            NetworkIdentity ni = collision.gameObject.GetComponent<NetworkIdentity>();
            bool destroy = false;
            // if ni == null: wall
            if (ni == null || (ni.GetNiType() == "SafeBox" && ni.GetNiTeam() == NetworkClient.serverObjects[activator].GetNiTeam())) {
                destroy = true;
            }
            //else if ((ni.GetNiType() == "Tank" && ni.GetID() != activator) || ni.GetNiType() == "SafeBox") {
            else if ((ni.GetNiType() == "Tank" || ni.GetNiType() == "SafeBox") &&
                      ni.GetNiTeam() != NetworkClient.serverObjects[activator].GetNiTeam()) {

                if (activator == NetworkClient.ClientID) {
                    JSONObject j = new JSONObject();
                    j.AddField("hitObjectType", ni.GetNiType());
                    j.AddField("hitObjectID", ni.GetID());
                    j.AddField("activator", activator);
                    socketReference.Emit("fireBallCollision", j);
                }

                if (ni.GetNiType() == "SafeBox") {
                    destroy = true;
                }
            }

            if (destroy) {
                this.GetComponent<AudioSource>().clip = fireballExplosionBackground;
                this.GetComponent<AudioSource>().Play();

                GameObject explosion = Instantiate(explosionFireBall);
                explosion.transform.position = transform.position;
                transform.position = new Vector3(1000, 1000, 0);
                Destroy(this.GetComponent<Projectile>());
                StartCoroutine(removeExplosionWaiter(1, explosion));
            }
        }

        private IEnumerator removeExplosionWaiter(float time, GameObject explosion) {
            yield return new WaitForSeconds(time);
            Destroy(explosion);
            Destroy(this.gameObject);
        }
    }
}
