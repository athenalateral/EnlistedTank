using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Adding an enemy 
[System.Serializable]
public class StageDefinition
{

    [Header("Enemy Spawning")]
    public GameObject[] enemiesForThisStage;
    // Only these enemies will spawn during this stage.

    public float enemySpawnPerSecond = 0.5f;
    // How many enemies spawn per second during this stage.

    [Header("Boss")]
    public GameObject bossPrefab;
    // The boss that appears at the end of this stage.

    [Header("Timing")]
    public float stageLength = 30f;
    // How long this stage lasts before the boss appears.
}

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

    // this section is for adding boss and changing levels

    [Header("Stage System")]
    public StageDefinition[] stages;
    // This is your array of 5 stages.
    // Each one has a background, enemy list, boss, and timer.

    public SpriteRenderer backgroundRenderer;
    // Drag your background SpriteRenderer here in the Unity Inspector.

    private int currentStageIndex = 0;
    // Keeps track of which stage we are currently on.

    private float stageTimer = 0f;
    // Counts how long we have been in the current stage.

    private bool bossSpawned = false;
    // Makes sure the boss only spawns once per stage.

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

        ApplyStage(currentStageIndex);
        
    }

    public void SpawnEnemy() {
        // Make sure the stages array exists and has something inside it.
        if (stages == null || stages.Length == 0)
        // If spawning is turned off, wait and try again later.
        if ( !spawnEnemies ) {
            Invoke( nameof( SpawnEnemy ), 1f / enemySpawnPerSecond );
            return;
        }

        // Get the current stage.
        StageDefinition currentStage = stages[currentStageIndex];

        // Get the enemy array for this stage.
        GameObject[] currentEnemies = currentStage.enemiesForThisStage;

        // Safety check.
        // If this stage has no enemies, do not spawn anything.
        if (currentEnemies == null || currentEnemies.Length == 0)
        {
            Invoke( nameof( SpawnEnemy ), 1f / enemySpawnPerSecond );
            return;
        }

        // Pick a random enemy from the current stage enemy array.
        int ndx = Random.Range(0, currentEnemies.Length);

        // Create that enemy.
        GameObject go = Instantiate<GameObject>( currentEnemies[ ndx ] );

        // Figure out how far inside the screen the enemy should spawn.
        float enemyInset = enemyInsetDefault;

        if (go.GetComponent<BoundsCheck>() != null) {
            enemyInset = Mathf.Abs( go.GetComponent<BoundsCheck>().radius );
        }

        // Pick a random x position at the top of the screen.
        Vector3 pos = Vector3.zero;

        float xMin = -bndCheck.camWidth + enemyInset;
        float xMax = bndCheck.camWidth - enemyInset;

        pos.x = Random.Range( xMin, xMax );
        pos.y = bndCheck.camHeight + enemyInset;

        go.transform.position = pos;

        // Schedule the next enemy spawn using the current stage spawn rate.
        Invoke( nameof(SpawnEnemy), 1f / enemySpawnPerSecond );
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

    void Update()
    {
        // If there are no stages, stop this code so it does not crash.
        if (stages == null || stages.Length == 0) return;

        // Add time to the current stage timer.
        stageTimer += Time.deltaTime;

        // Get the current stage from the stages array.
        StageDefinition currentStage = stages[currentStageIndex];

        // If enough time has passed, spawn the boss.
        if (stageTimer >= currentStage.stageLength && !bossSpawned)
        {
            SpawnBossForCurrentStage();
        }
    }

    void ApplyStage(int stageIndex)
    {
        // Safety check so we do not go outside the array.
        if (stageIndex < 0 || stageIndex >= stages.Length) return;

        // Get the stage we are switching into.
        StageDefinition stage = stages[stageIndex];

        // Reset the timer for this new stage.
        stageTimer = 0f;

        // Allow this stage to spawn its boss later.
        bossSpawned = false;

        // Turn enemy spawning back on.
        spawnEnemies = true;

        // Change the enemy spawn speed for this stage.
        enemySpawnPerSecond = stage.enemySpawnPerSecond;

        Debug.Log("Now entering stage: " + stageIndex);
    }

void SpawnBossForCurrentStage()
{
    // Mark boss as spawned so it does not spawn every frame.
    bossSpawned = true;

    // Stop regular enemies while the boss is alive.
    spawnEnemies = false;

    // Get the current stage.
    StageDefinition currentStage = stages[currentStageIndex];

    // If this stage has no boss, immediately move to the next stage.
    if (currentStage.bossPrefab == null)
    {
        AdvanceToNextStage();
        return;
    }

    // Create the boss.
    GameObject boss = Instantiate<GameObject>(currentStage.bossPrefab);

    // Put the boss near the top center of the screen.
    Vector3 pos = Vector3.zero;
    pos.x = 0;
    pos.y = bndCheck.camHeight - 2f;
    pos.z = 0;

    boss.transform.position = pos;

    Debug.Log("Boss spawned for stage: " + currentStageIndex);
}

void AdvanceToNextStage()
{
    // Move to the next stage in the array.
    currentStageIndex++;

    // If we reached the end of the stage array, stop advancing.
    if (currentStageIndex >= stages.Length)
    {
        Debug.Log("All stages complete!");
        spawnEnemies = false;
        return;
    }

    // Apply the next stage.
    ApplyStage(currentStageIndex);
}
static public void BOSS_DESTROYED()
{
    // Go to the next stage after the boss dies.
    S.AdvanceToNextStage();
}
}
