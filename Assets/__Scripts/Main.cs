using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Main : MonoBehaviour
{
    static private Main S;
    static private Dictionary<eWeaponType, WeaponDefinition> WEAP_DICT;
    
    [Header("Inscribed")]
    public bool spawnEnemies = true;
    public GameObject[] prefabEnemies;
    public float enemySpawnPerSecond = 0.5f;
    public float enemyInsetDefault = 1.5f;
    public float gameRestartDelay = 2;
    public GameObject prefabPowerUp;
    public WeaponDefinition[] weaponDefinitions;
    public eWeaponType[] powerUpFrequency = new eWeaponType[] {
                            eWeaponType.blaster, eWeaponType.blaster,
                            eWeaponType.spread, eWeaponType.shield   };

    private BoundsCheck bndCheck;

    [Header("UI")]
    public TMP_Text scoreText;

    [Header("Audio")]
    public AudioSource musicSource;
    public AudioClip backgroundMusic;

    private int score = 0;

    void Awake() {
        S = this;
        bndCheck = GetComponent<BoundsCheck>();

        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.Play();

        Invoke( nameof(SpawnEnemy), 1f/enemySpawnPerSecond );

        WEAP_DICT = new Dictionary<eWeaponType, WeaponDefinition>();
        foreach ( WeaponDefinition def in weaponDefinitions ) {
            WEAP_DICT[def.type] = def;
        }
    }

    public void SpawnEnemy() {
        if ( !spawnEnemies ) {
            Invoke( nameof( SpawnEnemy ), 1f / enemySpawnPerSecond );
            return;
        }
        int ndx = Random.Range(0, prefabEnemies.Length);
        GameObject go = Instantiate<GameObject>( prefabEnemies[ ndx ] );

        float enemyInset = enemyInsetDefault;
        if (go.GetComponent<BoundsCheck>() != null) {
            enemyInset = Mathf.Abs( go.GetComponent<BoundsCheck>().radius );
        }

        Vector3 pos = Vector3.zero;
        float xMin = -bndCheck.camWidth + enemyInset;
        float xMax = bndCheck.camWidth - enemyInset;
        pos.x = Random.Range( xMin, xMax );
        pos.y = bndCheck.camHeight + enemyInset;
        go.transform.position = pos;

        Invoke( nameof(SpawnEnemy), 1f/enemySpawnPerSecond );
    }
    void DelayedRestart() {
        Invoke( nameof(Restart), gameRestartDelay );
    }

    void Restart() {
        SceneManager.LoadScene( "__Scene_0" );
    }

    public void AddScore(int points) {
        score += points;
        scoreText.text = "Money: " + score;
    }

    IEnumerator FadeOutMusic(float duration) {
        float startVolume = musicSource.volume;

        while (musicSource.volume > 0) {
            musicSource.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        musicSource.Stop();
    }

    static public void HERO_DIED() {
        S.StartCoroutine(S.FadeOutMusic(1f));
        S.DelayedRestart();
    }

    static public WeaponDefinition GET_WEAPON_DEFINITION( eWeaponType wt ) {
        if (WEAP_DICT.ContainsKey(wt)) {
            return( WEAP_DICT[wt]);
        }
        return (new WeaponDefinition());
    }

    static public void SHIP_DESTROYED(Enemy e) {
        if (e.deathSFX != null) {
            AudioSource.PlayClipAtPoint(e.deathSFX, e.transform.position);
        }
        
        if (Random.value <= e.powerUpDropChance) {
            int ndx = Random.Range( 0, S.powerUpFrequency.Length );
            eWeaponType pUpType = S.powerUpFrequency[ndx];

            GameObject go = Instantiate<GameObject>( S.prefabPowerUp );
            PowerUp pUp = go.GetComponent<PowerUp>();
            pUp.SetType( pUpType );
            pUp.transform.position = e.transform.position;
        }
        S.AddScore(e.score);
    }
}
