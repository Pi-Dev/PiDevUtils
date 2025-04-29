using UnityEngine;
using DG.Tweening;
using System.Collections;

/* Copyright (c) 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 * 
 * The MIT License (MIT)
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 * ============= Description =============
 * Dynamically plays friction and impact sounds based on velocity using customizable curves and SoundBankSets.
 * Supports Rigidbody, Transform, or custom velocity providers for friction calculation.
 * Integrates with DOTween for smooth fade-out of looping friction sounds on stop.
 * REQUIRES DoTween (Free) from Demigiant - https://dotween.demigiant.com/
 *
 * ============= Usage =============
 * Attach to an object with Rigidbody or implement IVelocityAudioSourceFrictionProvider.
 * Assign frictionSound and impactSound along with velocity-based curves.
 */

namespace PiDev.Utilities
{
    public interface IVelocityAudioSourceFrictionProvider { public Vector3 GetVelocity(); }
    public class VelocityBasedAudioSource : MonoBehaviour
    {
        [Header("Impact Settings")]
        public SoundBankSet impactSound;
        public AnimationCurve velocityBasedImpactCurve;

        [Header("Friction Settings")]
        public SoundBankSet frictionSound;
        public AnimationCurve velocityBasedFrictionCurve;
        public Vector3 velocityMultiplier = new Vector3(1, 1, 1);
        public enum FrictionMode { RigidbodyVelocity, TransformPosition, CustomProvider }
        public FrictionMode frictionMode = FrictionMode.RigidbodyVelocity;

        [Header("Debug")]
        [SerializeField] private bool isSliding;
        [SerializeField] private bool printFriction;
        [SerializeField] private bool printImpact;

        private Rigidbody rb;
        private IVelocityAudioSourceFrictionProvider cvp;
        private AudioSource frictionAudioSource;
        private Vector3 lastPosition;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            cvp = GetComponent<IVelocityAudioSourceFrictionProvider>();
            lastPosition = transform.position;

            if (frictionMode == FrictionMode.RigidbodyVelocity && rb == null)
                Debug.LogError("VelocityBasedAudioSource requires a Rigidbody component in RigidbodyVelocity mode.");

            if (frictionMode == FrictionMode.CustomProvider && cvp == null)
                Debug.LogError("VelocityBasedAudioSource requires a component implementing IVelocityAudioSourceFrictionProvider in CustomProvider mode.");
        }

        private void FixedUpdate()
        {
            float velocityMagnitude = GetVelocityMagnitude();

            if (printFriction && velocityMagnitude > float.Epsilon)
                Debug.Log("FRICTION: " + velocityMagnitude.ToString("0.000"));

            if (frictionSound.sounds.Length > 0)
                HandleFrictionSound(velocityMagnitude);
        }

        private float GetVelocityMagnitude()
        {
            switch (frictionMode)
            {
                case FrictionMode.RigidbodyVelocity:
                    return rb != null ? Vector3.Scale(rb.velocity, velocityMultiplier).magnitude : 0;

                case FrictionMode.TransformPosition:
                    Vector3 velocity = (transform.position - lastPosition) / Time.fixedDeltaTime;
                    lastPosition = transform.position;
                    return Vector3.Scale(velocity, velocityMultiplier).magnitude;

                case FrictionMode.CustomProvider:
                    return cvp != null
                        ? Vector3.Scale(cvp.GetVelocity(), velocityMultiplier).magnitude
                        : 0;

                default:
                    return 0;
            }
        }

        private void HandleFrictionSound(float velocityMagnitude)
        {
            float frictionVolume = velocityBasedFrictionCurve.Evaluate(velocityMagnitude);

            if (frictionVolume > 0)
            {
                if (frictionAudioSource == null)
                    frictionAudioSource = frictionSound.PlayLooping(transform, frictionVolume);

                if (frictionAudioSource != null)
                    frictionAudioSource.volume = frictionVolume;

                isSliding = true;
            }
            else
            {
                if (frictionAudioSource != null)
                {
                    StartCoroutine(FadeOutAndStop(frictionAudioSource));
                    frictionAudioSource = null;
                }

                isSliding = false;
            }
        }

        private IEnumerator FadeOutAndStop(AudioSource source, float fadeDuration = 0.5f)
        {
            if (source == null) yield break;

            float startVolume = source.volume;
            float time = 0f;

            while (time < fadeDuration)
            {
                time += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, time / fadeDuration);
                yield return null;
            }

            source.Stop();
            Destroy(source.gameObject);
        }


        private void OnCollisionEnter(Collision collision)
        {
            if (printImpact) Debug.Log("IMPACT: " + collision.relativeVelocity.magnitude.ToString("0.000"));
            if (impactSound.sounds.Length == 0) return;

            float impactVolume = velocityBasedImpactCurve.Evaluate(collision.relativeVelocity.magnitude);
            if (impactVolume > 0) impactSound.Play(transform.position, impactVolume);
        }
    }
}