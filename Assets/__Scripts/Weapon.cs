using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eWeaponType
{
    none,
    blaster,
    spread,
    phaser,
    missile,
    laser,
    shield
}

[System.Serializable]
public class WeaponDefinition
{
    public eWeaponType type = eWeaponType.none;
    public string letter;
    public Color powerUpColor = Color.white;
    public GameObject weaponModelPrefab;
    public GameObject projectilePrefab;
    public Color projectileColor = Color.white;

    public float damageOnHit = 0;
    public float damagePerSec = 0;

    public float delayBetweenShots = 0;
    public float velocity = 50;
}

public class Weapon : MonoBehaviour
{
    static public Transform PROJECTILE_ANCHOR;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip singleFiringSound;
    public AudioClip continuousFiringSound;

    [Header("Dynamic")]
    [SerializeField]
    private eWeaponType _type = eWeaponType.none;

    public WeaponDefinition def;
    public float nextShotTime;

    private GameObject weaponModel;
    private Transform shotPointTrans;

    private GameObject activeShield;
    private GameObject activeLaser;

    void Start()
    {
        if (PROJECTILE_ANCHOR == null)
        {
            GameObject go = new GameObject("_ProjectileAnchor");
            PROJECTILE_ANCHOR = go.transform;
        }

        shotPointTrans = transform.GetChild(0);

        SetType(_type);

        Hero hero = GetComponentInParent<Hero>();
        if (hero != null){
            hero.fireEvent += Fire;
            hero.stopFiringEvent += StopFiring;
        }
    }

    void Update()
    {
        if (type != eWeaponType.laser) DestoryLaser();
    }

    public eWeaponType type
    {
        get { return _type; }
        set { SetType(value); }
    }

    public void SetType(eWeaponType wt)
    {
        _type = wt;

        if (type == eWeaponType.none)
        {
            gameObject.SetActive(false);
            return;
        }
        else
        {
            gameObject.SetActive(true);
        }

        def = Main.GET_WEAPON_DEFINITION(_type);

        if (weaponModel != null) Destroy(weaponModel);

        weaponModel = Instantiate(def.weaponModelPrefab, transform);
        weaponModel.transform.localPosition = Vector3.zero;
        weaponModel.transform.localRotation = Quaternion.identity;
        weaponModel.transform.localScale = Vector3.one;

        nextShotTime = 0;
    }

    private void Fire()
    {
        if (!gameObject.activeInHierarchy) return;
        if (Time.time < nextShotTime) return;

        ProjectileHero p;
        Vector3 vel = shotPointTrans.up * def.velocity;

        switch (type)
        {
            // =================================================
            // BLASTER
            // =================================================
            case eWeaponType.blaster:
                p = MakeProjectile();
                p.vel = vel;
                break;

            // =================================================
            // SPREAD
            // =================================================
            case eWeaponType.spread:

                p = MakeProjectile();
                p.vel = vel;

                p = MakeProjectile();
                p.transform.rotation =
                    shotPointTrans.rotation *
                    Quaternion.Euler(0, 0, 12);
                p.vel = p.transform.up * def.velocity;

                p = MakeProjectile();
                p.transform.rotation =
                    shotPointTrans.rotation *
                    Quaternion.Euler(0, 0, -12);
                p.vel = p.transform.up * def.velocity;

                break;

            // =================================================
            // PHASER (rapid twin shots)
            // =================================================
            case eWeaponType.phaser:

                p = MakeProjectile(new Vector3(-0.25f, 0, 0));
                p.vel = vel;

                p = MakeProjectile(new Vector3(0.25f, 0, 0));
                p.vel = vel;

                break;

            // =================================================
            // MISSILE (slow heavy shot)
            // =================================================
            case eWeaponType.missile:

                p = MakeProjectile();
                p.vel = vel * 0.35f;

                SeekingMissile sm = p.GetComponent<SeekingMissile>();
                if (sm != null)
                    sm.speed = 12f;

                break;

            // =================================================
            // LASER (beam)
            // =================================================
            case eWeaponType.laser:

                FireLaser();
                break;

            // =================================================
            // SHIELD
            // =================================================
            case eWeaponType.shield:

                ActivateShield();
                break;
        }

        nextShotTime = Time.time + def.delayBetweenShots;
    }

    private void StopFiring(){
        if(type == eWeaponType.laser) DestoryLaser();
    }

    // =========================================================
    // PROJECTILES
    // =========================================================
    private ProjectileHero MakeProjectile()
    {
        return MakeProjectile(Vector3.zero);
    }

    private ProjectileHero MakeProjectile(Vector3 localOffset)
    {
        // Play firing sound
        PlaySingleFiringSound();
        GameObject go =
            Instantiate(def.projectilePrefab, PROJECTILE_ANCHOR);

        ProjectileHero p = go.GetComponent<ProjectileHero>();

        Vector3 pos = shotPointTrans.position;
        pos += transform.TransformDirection(localOffset);
        pos.z = 0;

        p.transform.position = pos;
        p.transform.rotation = shotPointTrans.rotation;

        p.type = type;

        return p;
    }

    // =========================================================
    // LASER
    // =========================================================
    public float laserLength = 500f;
    public float laserWidth = 0.4f;
    public float laserDPS = 20f;

    void FireLaser()
    {
        // Create beam once
        if (activeLaser == null)
        {
            // Turn on continuous firing sound
            PlayContinuousFiringSound(true);

            activeLaser = GameObject.CreatePrimitive(PrimitiveType.Cube);
            activeLaser.name = "LaserBeam";
            activeLaser.transform.SetParent(PROJECTILE_ANCHOR);

            Destroy(activeLaser.GetComponent<Collider>());

            // Override default primitive cube renderer. It uses a Univeral
            // Renderer matierial that doesn't render correctly in WebGL builds
            Renderer laserRend = activeLaser.GetComponent<Renderer>();
            Renderer projPrefabRend = def.projectilePrefab.GetComponent<Renderer>();
            if(laserRend != null && projPrefabRend != null){
                laserRend.material = projPrefabRend.sharedMaterial;
                laserRend.material.color = def.projectileColor;
            }
        }

        UpdateLaser();
    }

    void UpdateLaser()
    {
        float halfLength = laserLength * 0.5f;

        // Position beam starting at gun, extending forward
        activeLaser.transform.position =
            shotPointTrans.position + shotPointTrans.up * halfLength;

        activeLaser.transform.rotation = shotPointTrans.rotation;

        activeLaser.transform.localScale =
            new Vector3(laserWidth, laserLength, laserWidth);

        DamageEnemiesInBeam();
    }

    void DamageEnemiesInBeam()
{
    Vector3 center = activeLaser.transform.position;

    Vector3 halfExtents = new Vector3(
        laserWidth * 0.5f,
        laserLength * 0.5f,
        0.5f
    );

    Collider[] hits = Physics.OverlapBox(
        center,
        halfExtents,
        activeLaser.transform.rotation
    );

    foreach (Collider c in hits)
        {
            Enemy enemy = c.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.health -= laserDPS * Time.deltaTime;

                if (enemy.health <= 0)
                {
                    Main.SHIP_DESTROYED(enemy);
                    Destroy(enemy.gameObject);
                }
            }
        }
    }

    void DestoryLaser(){
        if (activeLaser != null){
            PlayContinuousFiringSound(false);
            Destroy(activeLaser);
            activeLaser = null;
        }
    }

    // =========================================================
    // SHIELD
    // =========================================================
    void ActivateShield()
    {
        if (activeShield != null) return;

        activeShield = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        activeShield.name = "Shield";

        activeShield.transform.SetParent(transform.parent);
        activeShield.transform.localPosition = Vector3.zero;
        activeShield.transform.localScale = Vector3.one * 2.2f;

        Destroy(activeShield.GetComponent<Collider>());

        Renderer r = activeShield.GetComponent<Renderer>();
        if (r != null)
        {
            r.material.color = new Color(
                def.projectileColor.r,
                def.projectileColor.g,
                def.projectileColor.b,
                0.4f
            );
        }

        StartCoroutine(ShieldRoutine());
    }

    IEnumerator ShieldRoutine()
    {
        yield return new WaitForSeconds(4f);

        if (activeShield != null)
            Destroy(activeShield);

        activeShield = null;
    }

    // Plays the single firing sound once (can play while other
    // sounds are playing)
    void PlaySingleFiringSound(){
        if(audioSource == null) return;
        audioSource.PlayOneShot(singleFiringSound);
    }

    // Plays the continuous fireing sound on loop if setPlaying
    // is set to true. If setPlaying is set to false, then it
    // stops the continuous looping sound.
    // NOTE: It isn't recommended to use this to stop a looping
    // sound while other oneshot plays are playing as it would
    // stop them too.
    void PlayContinuousFiringSound(bool setPlaying){
        if(audioSource == null) return;
        if(setPlaying){
            audioSource.clip = continuousFiringSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        else if(audioSource.clip == continuousFiringSound){
            audioSource.Stop();
            audioSource.clip = null;
            audioSource.loop = false;
        }
    }
}