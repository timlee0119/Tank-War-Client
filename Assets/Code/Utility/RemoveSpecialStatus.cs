using Project.Networking;
using SocketIO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Project.Utility {
    public class RemoveSpecialStatus : MonoBehaviour {
        public static void removeSpecialStatus(string id, string status, SocketIOComponent socket) {
            List<KeyValuePair<string, GameObject>> statusList = NetworkClient.specialStatus[id];
            int toRemove = -1;
            for (int i = 0; i < statusList.Count; ++i) {
                if (statusList[i].Key == status) {
                    toRemove = i;
                    break;
                }
            }
            if (toRemove == -1) { return; }

            // Remove GameObject, Pop status from status list
            Destroy(statusList[toRemove].Value);
            statusList.RemoveAt(toRemove);

            // Remove buffs and debuffs
            switch (status) {
                case "freeze":
                    if (id == NetworkClient.ClientID) {
                        NetworkClient.serverObjects[NetworkClient.ClientID].setControlling(true);
                    }
                    break;

                case "lifeTree":
                    break;

                default:

                    break;
            }
        }

        public static void removeAllStatus(string id, SocketIOComponent socket) {
            List<KeyValuePair<string, GameObject>> statusList = NetworkClient.specialStatus[id];
            List<string> allStatus = new List<string>();
            foreach (KeyValuePair<string, GameObject> item in statusList) {
                allStatus.Add(item.Key);
            }
            foreach (string status in allStatus) {
                removeSpecialStatus(id, status, socket);
            }

            Assert.IsTrue(statusList.Count == 0);
        }
    }
}
