using System.Collections;
using TMPro;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText = null;
    public int width = 8, height = 8;
    public GameObject[] tilePrefabs;
    public float tileSize = 1f;
    private float score = 0f;

    private GameObject[,] tiles;

    void Start()
    {
        tiles = new GameObject[width, height];
        GenerateGrid();
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
        yield return new WaitForSeconds(0.5f);
        CheckMatches();
    }

    void SpawnTileAt(int x, int y)
    {
        int index = Random.Range(0, tilePrefabs.Length);
        GameObject tile = Instantiate(tilePrefabs[index], new Vector2(x, y), Quaternion.identity);
        tile.name = $"Tile_{x}_{y}";
        tile.transform.SetParent(transform);
        tile.GetComponent<Tile>().type = index;
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
            // Swap back if no match
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

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var tile = tiles[x, y];
                if (tile == null) continue;

                int type = tile.GetComponent<Tile>().type;

                // Horizontal
                if (x < width - 2 &&
                    tiles[x + 1, y]?.GetComponent<Tile>().type == type &&
                    tiles[x + 2, y]?.GetComponent<Tile>().type == type)
                {
                    DestroyTile(x, y);
                    DestroyTile(x + 1, y);
                    DestroyTile(x + 2, y);
                    foundMatch = true;
                }

                // Vertical
                if (y < height - 2 &&
                    tiles[x, y + 1]?.GetComponent<Tile>().type == type &&
                    tiles[x, y + 2]?.GetComponent<Tile>().type == type)
                {
                    DestroyTile(x, y);
                    DestroyTile(x, y + 1);
                    DestroyTile(x, y + 2);
                    foundMatch = true;
                }
            }
        }

        if (foundMatch)
        {
            IncreaseScore();
            StartCoroutine(HandleFallAndRefill());
        }

        return foundMatch;
    }

    private void IncreaseScore()
    {
        score += 300;
        scoreText.text = $"Score: {score}";
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

        CheckMatches();
    }
}
