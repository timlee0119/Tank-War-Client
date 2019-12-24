using System.Collections;
using System.Collections.Generic;
using Project.Networking;
using Project.Utility;
using UnityEngine;

namespace Project.Player {
    public class PlayerManager : MonoBehaviour {

        const float BARREL_PIVOT_OFFSET = 90.0f;

        [SerializeField]
        private float rotation = 160;
        [SerializeField]
        private float speed;
        [SerializeField]
        private string username;
        [SerializeField]
        private string team;
        [SerializeField]
        private int startPosition;   // 0, 1, 2: blue123; 3, 4, 5: orange123
        [SerializeField]
        private float fullHealth;
        [SerializeField]
        private float health;
        [SerializeField]
        private float fullMp;
        [SerializeField]
        private float mp;
        [SerializeField]
        private int tankID;
        [SerializeField]
        private string passiveSkill;
        [SerializeField]
        private string super;

        [Header("Object References")]
        [SerializeField]
        private Transform barrelPivot;

        [Header("Class References")]
        [SerializeField]
        private NetworkIdentity networkIdentity;
        [SerializeField]
        private Transform bulletSpawnPoint;

        private Transform camTrans;
        private float lastRotation;

        // Shooting
        private BulletData bulletData;
        private Cooldown shootingCooldown;

        public void Start() {
            camTrans = Camera.main.GetComponent<Transform>();
            shootingCooldown = new Cooldown(0.5f);
            bulletData = new BulletData();
            bulletData.position = new Position();
            bulletData.direction = new Position();
        }

        public void Update() {
            if (networkIdentity.IsControlling()) {
                checkMovement();
                checkAiming();
                checkShooting();
                checkSuper();
            }
        }

        public void setInfo(JSONObject info) {
            speed = info["speed"].f;
            username = info["username"].RemoveQuotes();
            team = info["team"].RemoveQuotes();
            startPosition = info["startPosition"].i();
            fullHealth = info["fullHealth"].f;
            health = info["health"].f;
            fullMp = info["fullMp"].f;
            mp = info["mp"].f;
            tankID = info["tank"].i();
            passiveSkill = info["passiveSkill"].RemoveQuotes();
            super = info["super"].RemoveQuotes();
        }

        public string getTeam() { return this.team; }
        public float getHealth() { return this.health; }
        public float getFullHealth() { return this.fullHealth; }
        public float getMp() { return this.mp; }
        public void setMp(float mp) { this.mp = mp; }
        public float getFullMp() { return this.fullMp; }
        public void setHealth(float health) { this.health = health; }
        public string getUsername() { return this.username; }
        public int getTankID() { return tankID; }
        public void setSpeed(float speed) { this.speed = speed; }
        public Transform getBarrelPivot() { return this.barrelPivot; }
        public Transform getBulletSpawnPoint() { return this.bulletSpawnPoint; }

        public float GetLastRotation() {
            return lastRotation;
        }

        public void SetRotation(float Value) {
            barrelPivot.rotation = Quaternion.Euler(0, 0, Value - BARREL_PIVOT_OFFSET);
        }

        private void checkMovement() {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            transform.position += transform.up * vertical * speed * Time.deltaTime;
            transform.Rotate(0f, 0f, -horizontal * rotation * Time.deltaTime);
            camTrans.position = new Vector3(transform.position.x, transform.position.y, camTrans.position.z);
        }

        private void checkAiming() {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 dif = mousePosition - transform.position;
            dif.Normalize();
            float rot = Mathf.Atan2(dif.y, dif.x) * Mathf.Rad2Deg;

            lastRotation = rot;

            barrelPivot.rotation = Quaternion.Euler(0, 0, rot - BARREL_PIVOT_OFFSET);
        }

        private void checkShooting() {
            shootingCooldown.CooldownUpdate();

            if (Input.GetMouseButton(0) && !shootingCooldown.IsOnCooldown()) {
                shootingCooldown.StartCooldown();

                // Define Bullet
                bulletData.activator = NetworkClient.ClientID;
                bulletData.position.x = Mathf.Round(bulletSpawnPoint.position.x * 1000.0f) / 1000.0f;
                bulletData.position.y = Mathf.Round(bulletSpawnPoint.position.y * 1000.0f) / 1000.0f;
                bulletData.direction.x = bulletSpawnPoint.up.x;
                bulletData.direction.y = bulletSpawnPoint.up.y;

                // Send Bullet
                networkIdentity.GetSocket().Emit("fireBullet", new JSONObject(JsonUtility.ToJson(bulletData)));
            }
        }

        private void checkSuper() {
            if (Input.GetKeyDown("space")) {
                Debug.Log("I want to cast super");

                JSONObject j = new JSONObject();
                j.AddField("id", networkIdentity.GetID());
                networkIdentity.GetSocket().Emit("useSuper", j);
            }
        }
    }
}
