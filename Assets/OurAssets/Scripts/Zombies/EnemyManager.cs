using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [Header("Spawning")]
    [SerializeField]
    Zombie m_ZombiePrefab;
    [SerializeField]
    Transform[] m_SpawnPoints;

    [Header("Waves")]
    [SerializeField, Min(1)]
    int m_BaseCount = 6;
    [SerializeField, Min(1f)]
    float m_CountRate = 2f;
    [SerializeField, Min(1f)]
    float m_CountExponent = 1.3f;
    [SerializeField]
    float m_SpawnInterval = 2f;
    [SerializeField, Min(10)]
    int m_MaxAlive = 500;
    [SerializeField, Min(1f)]
    float m_SecondsBetweenWaves = 10f;
    [SerializeField, Min(0.001f)]
    float m_HealthGrowthPerWave = 0.12f;
    [SerializeField, Min(0.001f)]
    float m_DamageGrowthPerWave = 0.05f;
    [SerializeField, Min(1f)]
    float m_MaxDamageMultiplier = 3f;

    [Header("Chunking")]
    [SerializeField, Min(1f)]
    float m_ChunkSize = 30f;
    [SerializeField, Min(0)]
    int m_MinChunkDistanceFromPlayer = 2;

    [Header("XP")]
    [SerializeField, Min(1)]
    int m_BaseExperiencePerZombie = 10;
    [SerializeField, Min(0.001f)]
    float m_ExtraExperiencePerUpgrade = 0.1f; // 10%

    readonly HashSet<Zombie> m_SpawnedEnemies = new HashSet<Zombie>();
    readonly Dictionary<Vector2Int, List<Transform>> m_ChunkMap = new Dictionary<Vector2Int, List<Transform>>();
    FirstPersonPlayerCharacter m_Player;
    int m_Wave = 0;

    void Awake()
    {
        if (Instance && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        m_Player = FindAnyObjectByType<FirstPersonPlayerCharacter>();
        BuildChunkMap();
        StartCoroutine(WaveCooldown()); // Small break at the start of the game
    }

    void FixedUpdate()
    {
        foreach (Zombie zombie in m_SpawnedEnemies) zombie.ZombieFixedUpdate();
    }

    public void ZombieDied(Zombie zombie)
    {
        m_SpawnedEnemies.Remove(zombie);
        if (!m_Player) return;
        PlayerLevelComponent levelComponent = m_Player.GetComponent<PlayerLevelComponent>();
        if (!levelComponent) return;
        int xpGained = Mathf.RoundToInt(
            m_BaseExperiencePerZombie
            * (1f + m_ExtraExperiencePerUpgrade * UpgradeSystem.Instance.GetUpgradeLevel(UpgradeType.ExperienceGain))
        );
        levelComponent.AddXP(xpGained);
    }

    int EnemiesForWave(int wave) => m_BaseCount + Mathf.RoundToInt(m_CountRate * Mathf.Pow(wave - 1, m_CountExponent));
    float HealthMultiplier(int wave) => Mathf.Pow(1f + m_HealthGrowthPerWave, wave - 1);
    float DamageMultiplier(int wave) => Mathf.Min(Mathf.Pow(1f + m_DamageGrowthPerWave, wave - 1), m_MaxDamageMultiplier);

    void BuildChunkMap()
    {
        m_ChunkMap.Clear();
        foreach (Transform point in m_SpawnPoints)
        {
            if (!point) continue;
            Vector2Int chunk = ChunkCoordFor(point.position);
            if (!m_ChunkMap.TryGetValue(chunk, out List<Transform> pointsInChunk))
            {
                pointsInChunk = new List<Transform>();
                m_ChunkMap[chunk] = pointsInChunk;
            }
            pointsInChunk.Add(point);
        }
    }

    Vector2Int ChunkCoordFor(Vector3 worldPosition) => new Vector2Int(
        Mathf.FloorToInt(worldPosition.x / m_ChunkSize),
        Mathf.FloorToInt(worldPosition.z / m_ChunkSize)
    );

    IEnumerator SpawnLoop()
    {
        int enemiesSpawnedThisWave = 0, enemiesToSpawn = EnemiesForWave(m_Wave);
        while (enemiesSpawnedThisWave < enemiesToSpawn)
        {
            yield return new WaitWhile(() => m_SpawnedEnemies.Count >= m_MaxAlive);
            SpawnEnemy();
            yield return new WaitForSeconds(m_SpawnInterval);
        }
        StartCoroutine(WaveCooldown());
    }

    IEnumerator WaveCooldown()
    {
        yield return new WaitForSeconds(m_SecondsBetweenWaves);
        ++m_Wave;
        StartCoroutine(SpawnLoop());
    }

    void SpawnEnemy()
    {
        Transform point = ChooseSpawnPoint();
        if (!point) return;
        Zombie zombie = Instantiate(m_ZombiePrefab, point.position, point.rotation);
        m_SpawnedEnemies.Add(zombie);
        if (zombie && m_Player)
            zombie.Init(m_Player, HealthMultiplier(m_Wave), DamageMultiplier(m_Wave));
    }

    Transform ChooseSpawnPoint()
    {
        if (m_ChunkMap.Count == 0) return null;
        Vector2Int playerChunk = m_Player ? ChunkCoordFor(m_Player.transform.position) : default;
        List<Vector2Int> candidates = ChunksMatching(playerChunk, distance => distance >= m_MinChunkDistanceFromPlayer, minimizeDistance: true);
        if (candidates.Count == 0)
            candidates = ChunksMatching(playerChunk, distance => true, minimizeDistance: false);
        if (candidates.Count == 0) return null;
        List<Transform> pointsInChunk = m_ChunkMap[candidates[Random.Range(0, candidates.Count)]];
        return pointsInChunk[Random.Range(0, pointsInChunk.Count)];
    }

    List<Vector2Int> ChunksMatching(Vector2Int playerChunk, System.Func<int, bool> distanceFilter, bool minimizeDistance)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        int bestDistance = minimizeDistance ? int.MaxValue : int.MinValue;
        foreach (Vector2Int chunk in m_ChunkMap.Keys)
        {
            int distance = Mathf.Max(Mathf.Abs(chunk.x - playerChunk.x), Mathf.Abs(chunk.y - playerChunk.y));
            if (!distanceFilter(distance)) continue;
            bool bIsBetter = minimizeDistance ? distance < bestDistance : distance > bestDistance;
            if (bIsBetter)
            {
                bestDistance = distance;
                result.Clear();
                result.Add(chunk);
            }
            else if (distance == bestDistance) result.Add(chunk);
        }
        return result;
    }
}
