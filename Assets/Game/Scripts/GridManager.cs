using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private Button powerButton = null;
    [SerializeField] private Button startGameButton = null;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private GameObject playerNamePanel = null;
    [SerializeField] private GameObject newGamePanel = null;

    private float hightScore;
    public int width = 8, height = 8;
    public GameObject[] tilePrefabs;
    public float tileSize = 1f;
    private int counter = 0;
    private float timer = 0;
    public float maxTime = 120f;
    public Dictionary<int, int> targetSequence = new Dictionary<int, int>()
    {
        { 2, 3 },  // number 2 must appear 3 times
        { 3, 2 },  // number 3 must appear 2 times
        { 5, 1 },  // number 5 must appear 1 time
        { 7, 4 }   // number 7 must appear 4 times
    };

    private float score = 0f;
    private int newPower;
    private GameObject[,] tiles;
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
                    if (tiles[x, y] != null)
                    {
                        Destroy(tiles[x, y]);
                        tiles[x, y] = null;
                    }
                }
            }
         }

        timer = maxTime;
        hightScore = 100;
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
    }

    void CheckAndSetHighScore(float newScore)
    {
        float highScore = PlayerPrefs.GetFloat("highScore", 0); // Default is 0 if not set yet
        playerName.text = PlayerPrefs.GetString("playerName", "Amazing Player");
        if (newScore > highScore)
        {
            PlayerPrefs.SetFloat("highScore", newScore);
            PlayerPrefs.Save();
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

    // Example: Save name with a button
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
        timer -= Time.deltaTime;
        timerText.text = "Time: " + timer.ToString();
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
        // Always check for target sequence after matches are resolved
       // CheckTargetSequenceAtBottom();
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
                    score += 500; // Bonus for reaching goal
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
            if (counter == targetSequence.Count) Debug.Log("You Won");
        }
        
        // If we removed any tiles, trigger the fall and refill process
            if (removedAny)
            {
                StartCoroutine(HandleFallAndRefillAfterTarget());
            }
    }

    private void PowerButton()
    {
        counter *= 1;

        score -= 1000 * counter; // Optional: score bonus for using the button
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


    // Separate coroutine for handling fall after target removal to avoid conflicts
    IEnumerator HandleFallAndRefillAfterTarget()
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

        // Check for new matches that might have formed
        bool hasNewMatches = CheckMatches();
        if (!hasNewMatches)
        {
            // Check again for target sequence after everything settled
           // CheckTargetSequenceAtBottom();
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
            // Always check for target sequence after refilling
           // CheckTargetSequenceAtBottom();
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
        GenerateGrid();
    }
}