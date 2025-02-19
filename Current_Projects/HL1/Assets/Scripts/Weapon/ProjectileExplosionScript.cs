﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ProjectileExplosionScript : MonoBehaviour
{
    public float explosionRadius;
    public float explosionPower;
    public AudioClip explosionSFX;
    public AudioClip combineBallLaunch;
    public AudioClip combineBallBounce;
    public static AudioSource combineBallSource;

    public GameObject entity;

    public bool isCombineBall;
    private float timer;
    public float timerMax;

    private float floatTimer;
    public float floatTimeMax;

    public ParticleSystem explosionPrefab;

    // Start is called before the first frame update
    void Start()
    {
        timer = 0.0f;
        floatTimer = 0.0f;
        if (isCombineBall)
        {
            combineBallSource = this.GetComponent<AudioSource>();
            StartCoroutine(SoundController.gunSounds(explosionSFX, 0));
        }
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        // PROJECTILE EXPLODES ANYWAY AFTER 5 SECONDS
        if (timer >= timerMax)
        {
            Explosion();
            Destroy(this.gameObject);
        }

        // IF TARGET WAS HUMANOID AND HIT BY COMBINE BALL
        if (entity != null && entity.activeSelf)
        {
            if (EntityHealth.isFloating)
            {
                entity.GetComponent<EntityHealth>().neutralFace.SetActive(false);
                entity.GetComponent<EntityHealth>().hostileFace.SetActive(false);
                entity.GetComponent<EntityHealth>().deadFace.SetActive(true);
                floatTimer += Time.deltaTime;

                if (floatTimer >= floatTimeMax)
                {
                    Destroy(entity);
                }
            }
        }
    }
    private void OnCollisionEnter(Collision col)
    {
        if (isCombineBall)
        {
            StartCoroutine(BallSound(combineBallBounce, 0));

            Rigidbody rb = col.transform.GetComponent<Rigidbody>();

            if (col.transform.tag != "CombineBall")
            {
                if (rb != null)
                {
                    if (col.transform.gameObject.GetComponent<EntityHealth>() != null)
                    {
                        entity = col.transform.gameObject;
                        if (entity.GetComponent<EntityHealth>().isHumanoid)
                        {
                            StartCoroutine(Floating(entity));   //float up
                        }
                    }
                    else if (col.transform.parent.gameObject.GetComponent<EntityHealth>() != null)
                    {
                        entity = col.transform.parent.gameObject;
                        if (entity.GetComponent<EntityHealth>().isHumanoid)
                        {
                            StartCoroutine(Floating(entity));   //float up
                        }
                    }
                }
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Explosion();      
        Destroy(this.gameObject);  
    }

    public static IEnumerator BallSound(AudioClip SFX, float delay)
    {
        combineBallSource.PlayOneShot(SFX);
        yield return new WaitForSeconds(delay * 5);
    }

    private void Explosion()
    {
        Vector3 explosionPosition = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPosition, explosionRadius);
        StartCoroutine(SoundController.noiseSound(explosionSFX, 0));
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (!hit.transform.tag.Equals("SMGGrenade") && !hit.transform.tag.Equals("CombineBall"))
            {
                if (rb != null)
                {
                    rb.AddExplosionForce(explosionPower, explosionPosition, explosionRadius, explosionRadius);

                    if (hit.transform.gameObject.GetComponent<EntityHealth>() != null)
                    {
                        GameObject entity = hit.transform.gameObject;
                        int damage = Mathf.CeilToInt(CalculateDamage(entity.transform.position));
                        entity.GetComponent<EntityHealth>().entityCurrentHealth -= damage;
                        print("Explosive damage " + damage);
                    }
                    else if (hit.transform.parent.transform.gameObject.GetComponent<EntityHealth>() != null)
                    {
                        GameObject entity = hit.transform.parent.transform.gameObject;
                        int damage = Mathf.CeilToInt(CalculateDamage(entity.transform.position));
                        entity.GetComponent<EntityHealth>().entityCurrentHealth -= damage;
                        print("Explosive damage " + damage);
                    }
                    else
                    {
                        int damage = Mathf.CeilToInt(CalculateDamage(hit.transform.position));
                        print("Explosive damage " + damage);
                    }
                }
            }
        }
        if(!isCombineBall)
        {
            explosionPrefab.transform.parent = null;
            explosionPrefab.Play();
            Destroy(explosionPrefab.gameObject, explosionPrefab.main.duration);
        }
        else
        {
            explosionPrefab.Play();
        }
    }
    private float CalculateDamage(Vector3 targetPosition)
    {

        Vector3 explosionToTarget = targetPosition - transform.position;

        float explosionDistance = explosionToTarget.magnitude;

        float relativeDistance = (explosionRadius - explosionDistance) / explosionRadius;

        float damage = relativeDistance * explosionPower;

        damage = Mathf.Max(0f, damage);

        return damage;
    }

    IEnumerator Floating(GameObject entity)
    {
        if (entity.GetComponent<Rigidbody>() == null)
        {
            EntityHealth.isFloating = true;
            Destroy(entity.GetComponent<NavMeshAgent>());
            Destroy(entity.GetComponent<EntityHealth>().humanoidHead);
            Destroy(entity.GetComponent<EntityHealth>().humanoidTorso);
            entity.AddComponent<Rigidbody>();
            entity.GetComponent<Rigidbody>().mass = 1;
            entity.GetComponent<Rigidbody>().useGravity = false;
            entity.AddComponent<CapsuleCollider>();
            entity.GetComponent<CapsuleCollider>().center = new Vector3(0f, 0.5f, 0f);
            entity.GetComponent<CapsuleCollider>().height = 3;
        }
        float entityMass = entity.GetComponent<Rigidbody>().mass;
        entity.GetComponent<Rigidbody>().AddForce(0, entityMass * 20, 0);
        yield return null;
    }
}