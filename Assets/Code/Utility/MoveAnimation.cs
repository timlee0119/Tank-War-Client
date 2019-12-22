using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Utility {
    public class MoveAnimation : MonoBehaviour {
        private float t;
        private Vector3 startPosition;
        private Vector3 target;
        private float timeToReachTarget;

        void Start() {
            startPosition = target = transform.position;
        }

        void Update() {
            t += Time.deltaTime / timeToReachTarget;
            transform.position = new Vector3(Vector3.Lerp(startPosition, target, t).x,
                                             Vector3.Lerp(startPosition, target, t).y,
                                             transform.position.z);
        }
        public void SetDestination(Vector3 destination, float time) {
            t = 0;
            startPosition = transform.position;
            timeToReachTarget = time;
            target = destination;
        }
    }

}
