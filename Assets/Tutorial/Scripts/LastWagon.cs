namespace Dreamteck.Splines.Examples
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Dreamteck.Splines;
    using System;
    using System.Linq;

    public class LastWagon : MonoBehaviour
    {
        public SplineTracer _tracer = null;
        public TrainEngine engine;
        public GameObject checkPoint;
        public GameObject frontPoint;
        public GameObject backPoint;

        public BoxCollider boxCollider;

        private void Start()
        {
            Transform cubeChild = FindChildContainingName(transform, "cube");
            if (cubeChild != null)
            {
                boxCollider = cubeChild.GetComponent<BoxCollider>();
                Vector3 colliderCenter = boxCollider.center;
                colliderCenter.z = 0.35f;
                boxCollider.center = colliderCenter;
            }
        }

        Transform FindChildContainingName(Transform parent, string partialName)
        {
            foreach (Transform child in parent)
            {
                if (child.name.ToLower().Contains(partialName.ToLower()))
                {
                    return child;
                }
            }
            return null;
        }

        public void Set()
        {
            Invoke(nameof(InvokeDelay), Time.fixedDeltaTime);
        }
        void InvokeDelay()
        {
            _tracer.onMotionApplied += OnMotionApplied;

        }
        private void OnMotionApplied()
        {
            if (_tracer.GetPercent() == 0)
            {
                _tracer.onMotionApplied -= OnMotionApplied;
                engine.ResetPath();
            }
        }
    }
}
