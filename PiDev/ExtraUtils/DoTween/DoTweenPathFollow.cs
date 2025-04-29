using UnityEngine;
using System.Collections;
using DG.Tweening;

using Tweener = DG.Tweening.Core.TweenerCore<UnityEngine.Vector3, DG.Tweening.Plugins.Core.PathCore.Path, DG.Tweening.Plugins.Options.PathOptions>;
using System;

namespace PiDev.Utilities
{
    [AddComponentMenu("Animation/DoTweenPathFollow")]
    public class DoTweenPathFollow : MonoBehaviour
    {
        // this may not be good thing!
        public bool autoFollow = false;
        public float autoFollowDelay = 1f;
        public bool resetOnEachEnable = true;

        public DoTweenPath path = null;

        public GameObject onStartTarget;
        public string onStart;
        public GameObject onCompleteTarget;
        public string onComplete;

        Tweener tween = null;
        [NonSerialized] public Vector3 initialPos;

        private void Awake()
        {
            initialPos = transform.position;
        }
        void Start()
        {
            if (autoFollow) Follow();
        }

        public void ResetPosition()
        {
            transform.position = initialPos;
        }

        private void OnEnable()
        {
            if (resetOnEachEnable)
            {
                ResetPosition();
                Follow();
            }
        }

        public void Follow()
        {
            StartCoroutine(DelayedFollowRoutine());
        }

        IEnumerator DelayedFollowRoutine()
        {
            yield return new WaitForSeconds(autoFollowDelay);
            ForceFollow();
        }

        public bool IsCompleted()
        {
            return tween == null ? true : tween.IsComplete();
        }

        public void ForceFollow(GameObject target = null)
        {
            if (tween != null && !tween.IsComplete()) { tween.Kill(); tween = null; }

            // Allocations?
            if (path == null) path = GetComponent<DoTweenPath>();
            tween = (target ? target : gameObject).transform.DOPath(path.GetWorldNodes().ToArray(), path.value, path.pathType, PathMode.Full3D);
            if (path.valueType == DoTweenPath.ValueType.Speed) tween.SetSpeedBased();
            if (path.loop) tween.SetLoops(-1, path.loopType);
            tween.SetEase(path.forwardsEase);

            if (!string.IsNullOrEmpty(onStart)) tween.OnStart(() => onStartTarget.SendMessage(onStart, SendMessageOptions.DontRequireReceiver));
            if (!string.IsNullOrEmpty(onComplete)) tween.OnComplete(() => onCompleteTarget.SendMessage(onComplete, SendMessageOptions.DontRequireReceiver));
        }
    }
}