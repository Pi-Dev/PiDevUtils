using PiDev;
using PiDev.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Copyright (c) 2025 Petar Petrov (PeterSvP)
 * Provided for Reference & example usage
 * 
 * Part of ColorBlind FX: Desaturation, which you can purchase here: 
 * https://store.steampowered.com/app/670510/ColorBlend_FX_Desaturation/
 * 
 * ============= Description =============
 * This file is provided as an example to use Sound Bank Sets.
 * Please remove the "CBFX" part from the script name if you are using it in your own project.
 */

public class PhysicsProp : MonoBehaviour /*, CBFXChildObjectsPaintManager.IDynamicPaintable*/
{
    // This is a physics prop that can be destroyed by the player or other objects.
    // It can also be respawned or destroyed on collision with a death trigger.
    // This script is used for the CBFX platform controller and other physics props.
    public SoundBankSet explosionSound;
    public SoundBankSet hitSound;
    public SoundBankSet landSound;
    public float landVelocityMultiplier = 1;

    public enum State
    {
        Idle, Moving, Falling
    }

    public bool destroyOnKill, respawnOnKill, killPlayerOnDestroy;
    public bool destroyWithPickaxe;
    public bool fractureOnLand;
    public void SetFractureOnLand(bool f) => fractureOnLand = f;
    public int pickaxeHP = 3;
    public string killReason = "mf object destroyed";
    public bool stackOnMovingObjects = false;
    public float killPlayerMagnitude = 4;
    public BoxCollider2D allowedArea = null;

    Vector3 resetPosition;
    Quaternion resetRotation;
    public GameObject brokenObject, DestroyEffect;

    bool exploded = false, shouldRespawn = false;

    private void Awake()
    {
        resetPosition = transform.position;
        resetRotation = transform.rotation;
    }

    public State state;

    private void Start()
    {
        state = State.Idle;
    }

    private void Update()
    {
        if(stackOnMovingObjects && lastLanded != null)
        {
            var tp = transform.position;
            var newpos = lastLanded.position;
            var posdiff = newpos - lastLandedPos;
            if (posdiff.magnitude > 0)
            {
                tp += posdiff;
                lastLandedPos = newpos;
                transform.position = tp;
            }

        }
    }

    private void FixedUpdate()
    {
        var rb = GetComponent<Rigidbody>();
        if (rb)
        {
            // Out of bounds?
            if (allowedArea != null)
            {
                Vector2 position2D = (Vector2)transform.position;
                Bounds bounds = allowedArea.bounds;

                bool insideX = position2D.x >= bounds.min.x && position2D.x <= bounds.max.x;
                bool insideY = position2D.y >= bounds.min.y && position2D.y <= bounds.max.y;

                if (!(insideX && insideY))
                {
                    if (respawnOnKill) shouldRespawn = true;
                    else Explode();
                }
            }

            // handle respawn
            if (shouldRespawn)
            {
                transform.position = resetPosition;
                transform.rotation = resetRotation;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                // GetComponentInParent<CBFXChildObjectsPaintManager>()?.WashObject(gameObject);
                shouldRespawn = false;
                exploded = false;
                return;
            }

            var v = rb.velocity;
            if (v.y < -1)
            {
                state = State.Falling;
                lastLanded = null;
            }

            if (state == State.Falling)
            {
                if (v.y > 0)
                {
                    landSound.Play(transform.position, landVelocityMultiplier * v.y);
                    state = State.Idle;
                }
            }

        }
    }

    Transform lastLanded = null;
    Vector3 lastLandedPos;
    private void OnCollisionEnter(Collision collision)
    {
        var rb = GetComponent<Rigidbody>();
        if (rb)
        {
            var v = collision.relativeVelocity;
            if (state == State.Falling)
            {
                float f = Mathf.Clamp01(Mathf.Abs(v.magnitude).RemapRanges(2, 12, 0, 1));
                if (fractureOnLand)
                {
                    // Removed to provide the example of how to use SoundBankSets
                    // GetComponent<Fracture>().BreakToParts();
                }
                else
                {
                    landSound.Play(transform.position, f);
                    lastLandedPos = collision.transform.position;
                    lastLanded = collision.transform;
                }
                //Debug.Log("Collision -> " + collision.gameObject.name + " with v.y = " + v.magnitude.ToString("0.00"));
            }
            else if(rb.velocity.y > 0.1f)
            {
                float f = Mathf.Clamp01(Mathf.Abs(v.magnitude).RemapRanges(2, 12, 0, 1));
                landSound.Play(transform.position, f);
            }

            // Removed to provide the example of how to use SoundBankSets
            // var pl = GameManager.GetPlayer();
            // if (pl && pl.gameObject == collision.gameObject && state == State.Falling && v.magnitude > killPlayerMagnitude && transform.position.y - 2 > pl.transform.position.y)
            // {
            //     pl.Die(CBFXPlatformController.DeathType.Collision, collision.relativeVelocity * 3);
            // }
        }
    }

    public void Explode()
    {
        // spawn effects
        if ((brokenObject || DestroyEffect) && !exploded)
        {
            if (brokenObject)
            {
                var bo = Instantiate(brokenObject, transform.parent);
                bo.transform.position = transform.position;
                bo.transform.rotation = transform.rotation;
                bo.transform.localScale = transform.localScale;
            }
            if (DestroyEffect) Instantiate(DestroyEffect, transform.position, Quaternion.identity);
            exploded = true;

            // var pl = GameManager.GetPlayer();
            // if (pl && killPlayerOnDestroy)
            // {
            //     if(killReason!="")
            //     {
            //         GameMenu.instance.MissionFailedReason = killReason;
            //         pl.Die(CBFXPlatformController.DeathType.MissionFailure, Vector3.zero);
            //     }
            //     else pl.Die(CBFXPlatformController.DeathType.Collision, (pl.transform.position - transform.position).normalized * 5);
            // }

            if (respawnOnKill) shouldRespawn = true;
            else
            {
                // GetComponent<SaveKeyRequirement>()?.MarkInSaveAndCancelEffect();
                Destroy(gameObject);
            }
        }
    }

    // void OnTriggerEnter(Collider other)
    // {
    //     if (destroyOnKill || respawnOnKill)
    //     {
    //         var dt = other.GetComponent<DeathTrigger>();
    //         if (dt)
    //         {
    //             Explode();
    //         }
    //     }
    // }

    // public void OnHitWithPickaxe()
    // {
    //     if(destroyWithPickaxe)
    //     {
    //         pickaxeHP--;
    //         if (pickaxeHP <= 0) Explode();
    //     }
    // }

}
