using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText = null;
    [SerializeField] private TextMeshProUGUI targetNumbers = null;
    [SerializeField] private TextMeshProUGUI timerText = null;
    [SerializeField] private TextMeshProUGUI playerName = null;
    [SerializeField] private TextMeshProUGUI highScoreText = null;
    [SerializeField] private TextMeshProUGUI targetText = null;
    [SerializeField] private Button powerButton = null;
    [SerializeField] private Button startGameButton = null;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private GameObject playerNamePanel = null;
    [SerializeField] private GameObject newGamePanel = null;
    [SerializeField] private GameObject plusButtonPrefab = null; // New: Plus button prefab (with SpriteRenderer)
    [SerializeField] private List<TargetSequenceData> selectedSequence = new List<TargetSequenceData>();
    
    private float hightScore;
    public int width = 8, height = 8;
    public GameObject[] tilePrefabs;
    public float tileSize = 1f;
    private int counter = 0;
    private float timer = 0;
    public float maxTime = 0f;
    public Dictionary<int, int> targetSequence = new Dictionary<int, int>();

    private float score = 0f;
    private int newPower;
    private GameObject[,] tiles;
    private GameObject currentPlusButton = null; // New: Track single plus button
    private Vector2Int plusButtonPosition; // New: Track plus button position
    private int index;
    
    void Start()
    {
       //PlayerPrefs.SetFloat("highScore", 0); // Default is 0 if not set yet
        powerButton.onClick.RemoveAllListeners();
        powerButton.onClick.AddListener(() => PowerButton());
        startGameButton.onClick.RemoveAllListeners();
        startGameButton.onClick.AddListener(() => StartGame());
    }

    private void StartGame()
    {
        if (tiles != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Destroy(tiles[x, y]); // safely destroy
                    tiles[x, y] = null;   // reset reference
                }
            }
        }

        // Clean up existing plus button
        if (currentPlusButton != null)
        {
            Destroy(currentPlusButton);
            currentPlusButton = null;
        }

        targetSequence.Clear();
        index = Random.Range(1, selectedSequence.Count);
        targetText.text = $"{selectedSequence[index].description}";

        foreach (var entry in selectedSequence[index].targets)
        {
            targetSequence[entry.number] = entry.count;
        }

        timer = maxTime;
        CheckAndSetHighScore(hightScore);
        tiles = new GameObject[width, height];
        targetNumbers.text = "Target Numbers: Amount x Target";

        foreach (KeyValuePair<int, int> kvp in targetSequence)
        {
            targetNumbers.text += $"\n{kvp.Value} x {kvp.Key}";
        }

        GenerateGrid();
        playerName.text = PlayerPrefs.GetString("playerName");
        highScoreText.text = PlayerPrefs.GetFloat("highScore").ToString();

        newPower = Random.Range(1, 10);
        powerButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = newPower.ToString();
        
        // Spawn initial plus button
        SpawnRandomPlusButton();
    }

    void CheckAndSetHighScore(float newScore)
    {
        float highScore = PlayerPrefs.GetFloat("highScore", 0); // Default is 0 if not set yet
        playerName.text = PlayerPrefs.GetString("playerName", "Amazing Player");
        if (newScore > highScore)
        {
            PlayerPrefs.SetFloat("highScore", newScore);
            PlayerPrefs.Save();
            newGamePanel.SetActive(true);
            playerNamePanel.SetActive(true);
        }
        else if (timer < 0)
        {
            PlayerPrefs.SetFloat("highScore", highScore);
            highScoreText.text = PlayerPrefs.GetFloat("highScore").ToString();
            newGamePanel.SetActive(true);
            playerName.text = PlayerPrefs.GetString("playerName");
        }
    }

    public string GetPlayerName()
    {
        return playerNameInput.text;
    }

    public void SavePlayerName()
    {
        string name = GetPlayerName();
        if (name.Length == 0)
        {
            name = "Amazing Player";
        }
        newGamePanel.SetActive(true);
        PlayerPrefs.SetString("playerName", name);
        playerName.text = PlayerPrefs.GetString("playerName");
        highScoreText.text = PlayerPrefs.GetFloat("highScore").ToString();

        PlayerPrefs.Save();
    }

    void Update()
    {
        timer += Time.deltaTime;
        timerText.text = "Time: " + timer.ToString();
    
        foreach (var entry in selectedSequence[index].targets)
        {
            targetSequence[entry.number] = entry.count;
        }
        if (timer <= 0)
        {
            CheckAndSetHighScore(score);
        }
    }

    void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                SpawnTileAt(x, y);
            }
        }

        StartCoroutine(WaitAndMatch());
    }

    IEnumerator WaitAndMatch()
    {
        yield return new WaitForSeconds(0.1f);
        bool hasMatch = true;
        while (hasMatch)
        {
            yield return new WaitForSeconds(0.1f);
            hasMatch = CheckMatches();
        }
        CheckTargetSequenceAtBottom();
    }

    // New: Spawn a plus button at a random valid location (avoiding edges)
    void SpawnRandomPlusButton()
    {
        if (currentPlusButton != null) return; // Only one plus button at a time
        
        List<Vector2Int> validPositions = new List<Vector2Int>();
        
        // Find positions where we can place a plus button (between two number tiles, avoiding edges)
        for (int x = 1; x < width - 2; x++) // Start from 1 and end before width-2 to avoid edges
        {
            for (int y = 1; y < height - 1; y++) // Avoid top and bottom edges
            {
                if (tiles[x, y] != null && tiles[x + 1, y] != null)
                {
                    validPositions.Add(new Vector2Int(x, y));
                }
            }
        }

        if (validPositions.Count > 0)
        {
            Vector2Int randomPos = validPositions[Random.Range(0, validPositions.Count)];
            SpawnPlusButtonAt(randomPos.x, randomPos.y);
        }
    }

    // New: Spawn plus button at specific position
    void SpawnPlusButtonAt(int x, int y)
    {
        if (plusButtonPrefab != null && currentPlusButton == null)
        {
            Vector3 position = new Vector3(x + 0.5f, y, -1f); // Position between tiles
            currentPlusButton = Instantiate(plusButtonPrefab, position, Quaternion.identity);
            currentPlusButton.transform.SetParent(transform);
            currentPlusButton.name = $"PlusButton_{x}_{y}";
            plusButtonPosition = new Vector2Int(x, y);
            
            // Add click detection component
            PlusButtonClick clickHandler = currentPlusButton.GetComponent<PlusButtonClick>();
            if (clickHandler == null)
            {
                clickHandler = currentPlusButton.AddComponent<PlusButtonClick>();
            }
            clickHandler.Initialize(this, x, y);
        }
    }

    // New: Handle plus button click (called by PlusButtonClick component)
    public void OnPlusButtonClicked(int x, int y)
    {
        if (tiles[x, y] != null && tiles[x + 1, y] != null && currentPlusButton != null)
        {
            // Get the values of the two adjacent tiles
            int leftValue = tiles[x, y].GetComponent<Tile>().type;
            int rightValue = tiles[x + 1, y].GetComponent<Tile>().type;
            int sum = (leftValue + rightValue) % 9; 
            
            // If sum is 0, make it 10 (or handle as you prefer)
            if (sum == 0) sum = 10;
            
            // Ensure sum doesn't exceed the maximum tile type
            if (sum > tilePrefabs.Length)
            {
                sum = tilePrefabs.Length;
            }
            
            // Destroy the plus button
            if (currentPlusButton != null)
            {
                Destroy(currentPlusButton);
                currentPlusButton = null;
            }
            
            // Destroy the right tile
            DestroyTile(x + 1, y);
            
            // Update the left tile to the sum value
            tiles[x, y].GetComponent<Tile>().type = sum;
            UpdateTileVisual(x, y, sum);
            
            // Add score bonus for using plus button
            score += 200;
            scoreText.text = $"Score: {score}";
            
            // Start the fall and refill process
            StartCoroutine(HandleFallAndRefillAfterPlus());
            
            // Spawn a new plus button elsewhere after a short delay
            StartCoroutine(SpawnNewPlusButtonDelayed());
        }
    }

    // New: Update tile visual after addition
    void UpdateTileVisual(int x, int y, int newType)
    {
        if (tiles[x, y] != null)
        {
            // Destroy current tile
            Destroy(tiles[x, y]);
            
            // Create new tile with the sum type
            int prefabIndex = Mathf.Clamp(newType - 1, 0, tilePrefabs.Length - 1);
            GameObject newTile = Instantiate(tilePrefabs[prefabIndex], new Vector2(x, y), Quaternion.identity);
            newTile.name = $"Tile_{x}_{y}";
            newTile.transform.SetParent(transform);
            newTile.GetComponent<Tile>().type = newType;
            newTile.GetComponent<Tile>().SetPosition(x, y);
            tiles[x, y] = newTile;
        }
    }

    // New: Coroutine to spawn new plus button after delay
    IEnumerator SpawnNewPlusButtonDelayed()
    {
        yield return new WaitForSeconds(1f);
        SpawnRandomPlusButton();
    }

    // New: Handle fall and refill after plus button usage
    IEnumerator HandleFallAndRefillAfterPlus()
    {
        yield return new WaitForSeconds(0.2f);

        // Make tiles fall down
        for (int x = 0; x < width; x++)
        {
            for (int y = 1; y < height; y++)
            {
                if (tiles[x, y] != null && tiles[x, y - 1] == null)
                {
                    int fallTo = y;
                    while (fallTo > 0 && tiles[x, fallTo - 1] == null)
                        fallTo--;

                    if (fallTo != y)
                    {
                        tiles[x, fallTo] = tiles[x, y];
                        tiles[x, y] = null;
                        tiles[x, fallTo].transform.position = new Vector2(x, fallTo);
                        tiles[x, fallTo].GetComponent<Tile>().SetPosition(x, fallTo);
                    }
                }
            }
        }

        // Refill empty spaces
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (tiles[x, y] == null)
                    SpawnTileAt(x, y);
            }
        }

        yield return new WaitForSeconds(0.2f);

        // Check for new matches
        bool hasNewMatches = CheckMatches();
        if (!hasNewMatches)
        {
            CheckTargetSequenceAtBottom();
        }
    }

    void CheckTargetSequenceAtBottom()
    {
        bool removedAny = false;
        
        for (int x = 0; x < width; x++)
        {
            GameObject tile = tiles[x, 0];
            if (tile != null)
            {
                int type = tile.GetComponent<Tile>().type;
                
                if (targetSequence.ContainsKey(type) && targetSequence[type] > 0)
                {
                    targetSequence[type] -= 1;
                    DestroyTile(x, 0);
                    score += 500;
                    scoreText.text = $"Score: {score}";
                    targetNumbers.text = "Target Numbers: Amount x Target";

                    foreach (KeyValuePair<int, int> kvp in targetSequence)
                    {
                         targetNumbers.text += $"\n{kvp.Value} x {kvp.Key}";
                    }
                    removedAny = true;
                }
            }
        }

        int counter = 0;
        foreach (KeyValuePair<int, int> kvp in targetSequence)
        {
            if (kvp.Value == 0)
            {
                counter++;
            }
            if (counter == targetSequence.Count)
            {
                newGamePanel.SetActive(true);
                score -= timer;
                CheckAndSetHighScore(score);
            }
        }
        
        if (removedAny)
        {
            StartCoroutine(HandleFallAndRefillAfterTarget());
        }
    }

    private void PowerButton()
    {
        counter *= 1;
        score -= 1000 * counter;
        RemoveAllTilesOfType(newPower);
        newPower = Random.Range(1, 10);
        powerButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = newPower.ToString();
    }

    public void RemoveAllTilesOfType(int targetType)
    {
        bool removedAny = false;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject tile = tiles[x, y];
                if (tile != null)
                {
                    int type = tile.GetComponent<Tile>().type;
                    if (type == targetType)
                    {
                        DestroyTile(x, y);
                        removedAny = true;
                    }
                }
            }
        }

        if (removedAny)
        {
            scoreText.text = $"Score: {score}";
            StartCoroutine(HandleFallAndRefillAfterTarget());
        }
    }

    IEnumerator HandleFallAndRefillAfterTarget()
    {
        yield return new WaitForSeconds(0.2f);

        for (int x = 0; x < width; x++)
        {
            for (int y = 1; y < height; y++)
            {
                if (tiles[x, y] != null && tiles[x, y - 1] == null)
                {
                    int fallTo = y;
                    while (fallTo > 0 && tiles[x, fallTo - 1] == null)
                        fallTo--;

                    if (fallTo != y)
                    {
                        tiles[x, fallTo] = tiles[x, y];
                        tiles[x, y] = null;
                        tiles[x, fallTo].transform.position = new Vector2(x, fallTo);
                        tiles[x, fallTo].GetComponent<Tile>().SetPosition(x, fallTo);
                    }
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (tiles[x, y] == null)
                    SpawnTileAt(x, y);
            }
        }

        yield return new WaitForSeconds(0.2f);

        bool hasNewMatches = CheckMatches();
        if (!hasNewMatches)
        {
            CheckTargetSequenceAtBottom();
        }
    }

    void SpawnTileAt(int x, int y)
    {
        int index = Random.Range(0, tilePrefabs.Length);
        GameObject tile = Instantiate(tilePrefabs[index], new Vector2(x, y), Quaternion.identity);
        tile.name = $"Tile_{x}_{y}";
        tile.transform.SetParent(transform);
        tile.GetComponent<Tile>().type = index + 1;
        tile.GetComponent<Tile>().SetPosition(x, y);
        tiles[x, y] = tile;
    }

    public void TrySwap(int x1, int y1, int x2, int y2)
    {
        if (!IsValid(x2, y2)) return;

        GameObject a = tiles[x1, y1];
        GameObject b = tiles[x2, y2];

        tiles[x1, y1] = b;
        tiles[x2, y2] = a;

        a.GetComponent<Tile>().SetPosition(x2, y2);
        b.GetComponent<Tile>().SetPosition(x1, y1);

        Vector3 posA = a.transform.position;
        Vector3 posB = b.transform.position;

        a.transform.position = posB;
        b.transform.position = posA;

        if (!CheckMatches())
        {
            tiles[x1, y1] = a;
            tiles[x2, y2] = b;

            a.GetComponent<Tile>().SetPosition(x1, y1);
            b.GetComponent<Tile>().SetPosition(x2, y2);

            a.transform.position = posA;
            b.transform.position = posB;
        }
    }

    bool IsValid(int x, int y) => x >= 0 && y >= 0 && x < width && y < height;

    bool CheckMatches()
    {
        bool foundMatch = false;
        bool[,] toDestroy = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var tile = tiles[x, y];
                if (tile == null) continue;

                int type = tile.GetComponent<Tile>().type;

                if (x < width - 2 &&
                    tiles[x + 1, y]?.GetComponent<Tile>().type == type &&
                    tiles[x + 2, y]?.GetComponent<Tile>().type == type)
                {
                    toDestroy[x, y] = true;
                    toDestroy[x + 1, y] = true;
                    toDestroy[x + 2, y] = true;
                    foundMatch = true;
                }

                if (y < height - 2 &&
                    tiles[x, y + 1]?.GetComponent<Tile>().type == type &&
                    tiles[x, y + 2]?.GetComponent<Tile>().type == type)
                {
                    toDestroy[x, y] = true;
                    toDestroy[x, y + 1] = true;
                    toDestroy[x, y + 2] = true;
                    foundMatch = true;
                }
            }
        }

        int destroyed = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (toDestroy[x, y])
                {
                    DestroyTile(x, y);
                    destroyed++;
                }
            }
        }

        if (destroyed > 0)
        {
            score += destroyed * 100;
            scoreText.text = $"Score: {score}";
        }

        if (foundMatch)
            StartCoroutine(HandleFallAndRefill());

        return foundMatch;
    }

    void DestroyTile(int x, int y)
    {
        if (tiles[x, y])
        {
            Destroy(tiles[x, y]);
            tiles[x, y] = null;
        }
    }

    IEnumerator HandleFallAndRefill()
    {
        yield return new WaitForSeconds(0.2f);

        for (int x = 0; x < width; x++)
        {
            for (int y = 1; y < height; y++)
            {
                if (tiles[x, y] != null && tiles[x, y - 1] == null)
                {
                    int fallTo = y;
                    while (fallTo > 0 && tiles[x, fallTo - 1] == null)
                        fallTo--;

                    if (fallTo != y)
                    {
                        tiles[x, fallTo] = tiles[x, y];
                        tiles[x, y] = null;
                        tiles[x, fallTo].transform.position = new Vector2(x, fallTo);
                        tiles[x, fallTo].GetComponent<Tile>().SetPosition(x, fallTo);
                    }
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (tiles[x, y] == null)
                    SpawnTileAt(x, y);
            }
        }

        yield return new WaitForSeconds(0.2f);

        if (!CheckMatches() && !HasAvailableMoves())
        {
            Debug.Log("No more moves. Resetting grid.");
            ResetGrid();
        }
        else
        {
            CheckTargetSequenceAtBottom();
        }
    }

    bool HasAvailableMoves()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (tiles[x, y] == null) continue;
                
                int currentType = tiles[x, y].GetComponent<Tile>().type;

                if (x < width - 1 && tiles[x + 1, y] != null && tiles[x + 1, y].GetComponent<Tile>().type == currentType)
                    return true;
                if (y < height - 1 && tiles[x, y + 1] != null && tiles[x, y + 1].GetComponent<Tile>().type == currentType)
                    return true;
            }
        }
        return false;
    }

    void ResetGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (tiles[x, y]) Destroy(tiles[x, y]);
            }
        }
        if (currentPlusButton != null)
        {
            Destroy(currentPlusButton);
            currentPlusButton = null;
        }
        GenerateGrid();
        SpawnRandomPlusButton();
    }
}