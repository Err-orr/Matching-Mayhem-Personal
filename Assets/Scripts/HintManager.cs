using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HintManager : MonoBehaviour
{
    private Board board;
    public float hintDelay;
    private float hintDelaySeconds;
    public GameObject hintParticle;
    public GameObject currentHint;

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
        hintDelaySeconds = hintDelay;
    }

    // Update is called once per frame
    void Update()
    {
        hintDelaySeconds -= Time.deltaTime;
        if (hintDelaySeconds <= 0 && currentHint == null) {
            MarkHint();
            hintDelaySeconds = hintDelay;
        }
    }

    List<GameObject> FindAllMatches() {
        List<GameObject> possibleMoves = new List<GameObject>();
        // Loop through every dot on the board and check if a valid move is possible
        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                if (board.allDots[i, j] != null)
                {
                    // Check for a valid move to the right
                    if (i < board.width - 1)
                    {
                        if (board.SwitchAndCheck(i, j, UnityEngine.Vector2.right))
                        {
                            possibleMoves.Add(board.allDots[i, j]);
                        }
                    }
                    // Check for a valid move upwards
                    if (j < board.height - 1)
                    {
                        if (board.SwitchAndCheck(i, j, UnityEngine.Vector2.up))
                        {
                            possibleMoves.Add(board.allDots[i, j]);
                        }
                    }
                }
            }
        }
        return possibleMoves;
    }

    GameObject PickOneRandomly() {
        List<GameObject> possibleMoves = new List<GameObject>();
        possibleMoves = FindAllMatches();
        if (possibleMoves.Count > 0) {
            int pieceToUse = Random.Range(0, possibleMoves.Count);
            return possibleMoves[pieceToUse];
        }
        return null;
    }

    private void MarkHint() {
        GameObject move = PickOneRandomly();
        if (move != null) {
            currentHint = Instantiate(hintParticle, move.transform.position, UnityEngine.Quaternion.identity);
        }
    }

    public void DestroyHint() {
        if (currentHint != null) {
            Destroy(currentHint);
            currentHint = null;
            hintDelaySeconds = hintDelay;
        }
    }
}