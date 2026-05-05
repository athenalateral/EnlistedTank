using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour
{
    static public Hero  S { get; private set; }

    [Header("Inscribed")]
    public float speed = 30;
    public float rollMult = -45;
    public float pitchMult = 30;
    public GameObject projectilePrefab;
    public float projectileSpeed = 40;
    public Weapon[] weapons;

    [Header("Dynamic")] [Range(0,4)] [SerializeField]
    private float _shieldLevel = 1;
    //public float shieldLevel = 1;

    [Tooltip( "This field holds a reference to the last triggering GameObject" )]
    private GameObject lastTriggerGo = null;
    public delegate void WeaponFireDelegate();
    public event WeaponFireDelegate fireEvent;

    public event WeaponFireDelegate stopFiringEvent;

    [Header("Turret")]
    public Transform turret;

    [Header("Shield Visual")]
    public Renderer shieldRenderer;
    public float offsetMin = 0f;
    public float offsetMax = 0.8f;
    void Awake()
    {
        if (S == null)
        {
            S = this;
        } else
        {
            Debug.LogError("Hero.Awake() - Attempted to assign second Hero.S!");
        }
        ClearWeapons();
        weapons[0].SetType(eWeaponType.blaster);
        UpdateShieldVisual();
        //fireEvent += TempFire;
    }

    // Update is called once per frame
    void Update()
    {
        float hAxis = Input.GetAxis("Horizontal");
        float vAxis = Input.GetAxis("Vertical");

        Vector3 pos = transform.position;
        pos.x += hAxis * speed * Time.deltaTime;
        pos.y += vAxis *speed * Time.deltaTime;
        transform.position = pos;

        transform.rotation = Quaternion.Euler(vAxis*pitchMult,hAxis*rollMult,0);

        AimTurret();

        if (Input.GetMouseButton(0))
        {
            if(fireEvent != null) fireEvent();
        }
        else{
            if(stopFiringEvent != null) stopFiringEvent();
        }
    }

    void OnTriggerEnter(Collider other) {
        Transform rootT = other.gameObject.transform.root;
        GameObject go = rootT.gameObject;

        Debug.Log("Shield trigger hit by: " + go.name);

        if (go == lastTriggerGo) return;
        lastTriggerGo = go;

        Enemy enemy = go.GetComponent<Enemy>();
        PowerUp pUp = go.GetComponent<PowerUp>();
        ProjectileEnemy proj = go.GetComponent<ProjectileEnemy>();

        if (enemy != null) {
            shieldLevel--;
        } 
        else if (proj != null) {
            shieldLevel -= proj.damage;
            Destroy(proj.gameObject);
        } 
        else if (pUp != null) {
            AbsorbPowerUp(pUp);
        } 
        else {
            Debug.LogWarning("Shield trigger hit by non-Enemy: " + go.name);
        }
    }

    public void AbsorbPowerUp( PowerUp pUp ) {
        Debug.Log( "Absorbed PowerUp: " + pUp.type);
        switch (pUp.type) {
            case eWeaponType.shield:
                shieldLevel++;
                break;
            default:
                if (pUp.type == weapons[0].type) {
                    Weapon weap = GetEmptyWeaponSlot();
                    if (weap != null) {
                        weap.SetType(pUp.type);
                    }
                } else {
                    ClearWeapons();
                    weapons[0].SetType(pUp.type);
                }
                break;
        }
        pUp.AbsorbedBy( this.gameObject );  
    }

    public float shieldLevel {
        get { return ( _shieldLevel ); }
        private set {
            _shieldLevel = Mathf.Min( value, 4);

            UpdateShieldVisual();

            if (_shieldLevel < 0) {
                if(stopFiringEvent != null) stopFiringEvent();
                Destroy(this.gameObject);
                Main.HERO_DIED();
            }
        }
    }

    void UpdateShieldVisual() {
        int level = Mathf.RoundToInt(_shieldLevel);

        float step = 1f / 5f; // 0.2 per slice
        float offsetX = level * step;

        Vector2 offset = shieldRenderer.material.mainTextureOffset;
        offset.x = offsetX;
        shieldRenderer.material.mainTextureOffset = offset;
    }

    Weapon GetEmptyWeaponSlot() {
        for (int i=0; i <weapons.Length; i++) {
            if ( weapons[i].type == eWeaponType.none ) {
                return (weapons[i]);
            }
        }
        return (null);
    }

    void ClearWeapons() {
        foreach(Weapon w in weapons) {
            w.SetType(eWeaponType.none);
        }
    }

    public void TakeDamage(float dmg)
    {
        float finalDamage = dmg;


        shieldLevel -= finalDamage;

        Debug.Log("Hero took damage: " + finalDamage);
    }

    void AimTurret()
    {
        Vector3 mousePos = Input.mousePosition;

        mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        Vector3 dir = worldPos - turret.position;
        dir.z = 0;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // absolute jank hack
        turret.rotation = Quaternion.Euler(-180, 0, -(angle - 90f));
    }
}
