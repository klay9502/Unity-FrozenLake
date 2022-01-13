using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    private enum Arrow { RIGHT, LEFT, UP, DOWN, STAY, ERROR};
    private Arrow arrow;
    private Arrow slipArrow;
    private Collider2D startTile;
    private Collider2D nowTile;
    private Collider2D oldTile;
    private Vector3 startPos;

    private List<int> logList = new List<int>();
    private List<Arrow> logArrowList = new List<Arrow>();
    LearningMgr learningMgr = new LearningMgr();

    // Initialize
    private void Awake()
    {
        arrow = Arrow.STAY;
        slipArrow = Arrow.STAY;
        startTile = null;
        nowTile = null;
        oldTile = null;
        logList.Clear();
        logArrowList.Clear();
    }

    void Update()
    {
        if (Time.time > learningMgr.nextTime)
        {
            learningMgr.nextTime = Time.time + learningMgr.timeLeft;
            Process();
        }

        // Debug
        // KeyController();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null)
        {
            Debug.Log("Func:OnTriggerEnter2D parameter value is null");
            return;
        }

        if (startTile == null)
        {
            startTile = collision;
            oldTile = collision;
            startPos = gameObject.transform.position;
        }

        if (collision.gameObject.name == "Wall")
        {
            OnContectWall();
        }
        else
        {
            if (collision.gameObject.GetComponent<Tile>().type == Tile.TileType.Hole)
            {
                transform.position = startPos;
                learningMgr.tryTotal++;
            }
            else if (collision.gameObject.GetComponent<Tile>().type == Tile.TileType.Goal)
            {
                transform.position = startPos;
                learningMgr.tryTotal++;
            }

            nowTile = collision;

            // Debug.Log(collision.gameObject.GetComponent<Tile>().type.ToString());
            logList.Add(nowTile.GetComponent<Tile>().tileID);

            if (learningMgr.bIsWindyFrozen)
                UpdateTileUI(learningMgr.alpha);
            else
                UpdateTileUI();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == null)
        {
            Debug.Log("Func:OnTriggerExit2D parameter value is null");
            return;
        }

        oldTile = GameObject.FindWithTag("TileSensor").GetComponent<TileMgr>().FindTileCollider2D(logList[logList.Count - 1]);

        if (oldTile.gameObject.GetComponent<Tile>().type == Tile.TileType.Hole)
        {
            transform.position = startPos;
            learningMgr.tryTotal++;
            oldTile = nowTile;
        }
        else if (oldTile.gameObject.GetComponent<Tile>().type == Tile.TileType.Goal)
        {
            transform.position = startPos;
            learningMgr.tryTotal++;
            oldTile = nowTile;
        }

        // Debug.Log(collision.gameObject.GetComponent<Tile>().type.ToString());
    }

    private void Process()
    {
        if (!learningMgr.bIsWindyFrozen)
        {
            // Windy frozen mode false // Deterministic
            float exploration_random = Random.Range(0.0f, 1.0f);

            if (exploration_random <= learningMgr.exploration)
            {
                Debug.Log("Deterministic::RANDOM::Count:" + learningMgr.tryTotal);
                RandomArrow(nowTile);
            }
            else
            {
                Debug.Log("Deterministic::ARGMAX::Count:" + learningMgr.tryTotal);
                MaxValueArrow(nowTile);
            }
        }
        else
        {
            // Windy frozen mode true // Stochastic (Non-Deterministic)
            float windyFrozen_random = Random.Range(0.0f, 1.0f);

            if (windyFrozen_random <= learningMgr.windyFrozen)
            {
                Debug.Log("Stochastic::RANDOM::Count:" + learningMgr.tryTotal + "   ### SLIP!! ###" + windyFrozen_random);
                RandomArrow(nowTile, true);
            }
            else
            {
                float exploration_random = Random.Range(0.0f, 1.0f);

                if (exploration_random <= learningMgr.exploration)
                {
                    Debug.Log("Stochastic::RANDOM::Count:" + learningMgr.tryTotal);
                    RandomArrow(nowTile);
                }
                else
                {
                    Debug.Log("Stochastic::ARGMAX::Count:" + learningMgr.tryTotal);
                    MaxValueArrow(nowTile);
                }
            }
        }
    }

    private void KeyController()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            arrow = Arrow.RIGHT;
            OnMoveRight();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            arrow = Arrow.LEFT;
            OnMoveLeft();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            arrow = Arrow.UP;
            OnMoveUp();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            arrow = Arrow.DOWN;
            OnMoveDown();
        }
    }

    private void OnContectWall()
    {
        switch (logArrowList[logArrowList.Count - 1])
        {
            case Arrow.RIGHT:
                //OnMoveLeft();
                nowTile.GetComponent<Tile>().right -= 1;
                break;
            case Arrow.LEFT:
                //OnMoveRight();
                nowTile.GetComponent<Tile>().left -= 1;
                break;
            case Arrow.UP:
                //OnMoveDown();
                nowTile.GetComponent<Tile>().up -= 1;
                break;
            case Arrow.DOWN:
                //OnMoveUp();
                nowTile.GetComponent<Tile>().down -= 1;
                break;
        }

        arrow = Arrow.STAY;

        transform.position = GameObject.FindWithTag("TileSensor").GetComponent<TileMgr>().FindTileCollider2D(logList[logList.Count - 1]).transform.position;
    }

    private void UpdateTileUI(float _alpha = 1f)
    {
        switch (arrow)
        {
            case Arrow.RIGHT:
                oldTile.GetComponent<Tile>().right = ((1 - _alpha) * oldTile.GetComponent<Tile>().right) + (_alpha * Mathf.Max(nowTile.GetComponent<Tile>().right,
                    nowTile.GetComponent<Tile>().left,
                    nowTile.GetComponent<Tile>().up,
                    nowTile.GetComponent<Tile>().down) * learningMgr.discount);
                break;
            case Arrow.LEFT:
                oldTile.GetComponent<Tile>().left = ((1 - _alpha) * oldTile.GetComponent<Tile>().left) + (_alpha * Mathf.Max(nowTile.GetComponent<Tile>().right,
                    nowTile.GetComponent<Tile>().left,
                    nowTile.GetComponent<Tile>().up,
                    nowTile.GetComponent<Tile>().down) * learningMgr.discount);
                break;
            case Arrow.UP:
                oldTile.GetComponent<Tile>().up = ((1 - _alpha) * oldTile.GetComponent<Tile>().up) + (_alpha * Mathf.Max(nowTile.GetComponent<Tile>().right,
                    nowTile.GetComponent<Tile>().left,
                    nowTile.GetComponent<Tile>().up,
                    nowTile.GetComponent<Tile>().down) * learningMgr.discount);
                break;
            case Arrow.DOWN:
                oldTile.GetComponent<Tile>().down = ((1 - _alpha) * oldTile.GetComponent<Tile>().down) + (_alpha * Mathf.Max(nowTile.GetComponent<Tile>().right,
                    nowTile.GetComponent<Tile>().left,
                    nowTile.GetComponent<Tile>().up,
                    nowTile.GetComponent<Tile>().down) * learningMgr.discount);
                break;
        }

        arrow = Arrow.STAY;
    }

    private void RandomArrow(Collider2D _nowTile, bool _bIsSlip = false)
    {
        if (_nowTile == null)
            Debug.LogError("Func:RandomArrow parameter value is null");

        float max = -10f;
        float right = Random.Range(0f, 1.0f);
        float left = Random.Range(0f, 1.0f);
        float up = Random.Range(0f, 1.0f);
        float down = Random.Range(0f, 1.0f);

        if (right >= max)
        {
            max = right;
            arrow = Arrow.RIGHT;
        }

        if (left >= max)
        {
            max = left;
            arrow = Arrow.LEFT;
        }

        if (up >= max)
        {
            max = up;
            arrow = Arrow.UP;
        }

        if (down >= max)
        {
            max = down;
            arrow = Arrow.DOWN;
        }

        if (_bIsSlip)
        {
            float maxSlip = -10f;
            float rightSlip = Random.Range(0f, 1.0f);
            float leftSlip = Random.Range(0f, 1.0f);
            float upSlip = Random.Range(0f, 1.0f);
            float downSlip = Random.Range(0f, 1.0f);

            if (rightSlip >= maxSlip)
            {
                maxSlip = rightSlip;
                slipArrow = Arrow.RIGHT;
            }

            if (leftSlip >= maxSlip)
            {
                maxSlip = leftSlip;
                slipArrow = Arrow.LEFT;
            }

            if (upSlip >= maxSlip)
            {
                maxSlip = upSlip;
                slipArrow = Arrow.UP;
            }

            if (downSlip >= maxSlip)
            {
                maxSlip = downSlip;
                slipArrow = Arrow.DOWN;
            }

            switch (slipArrow)
            {
                case Arrow.RIGHT:
                    OnMoveRight();
                    break;
                case Arrow.LEFT:
                    OnMoveLeft();
                    break;
                case Arrow.UP:
                    OnMoveUp();
                    break;
                case Arrow.DOWN:
                    OnMoveDown();
                    break;
            }

            Debug.Log("Slip:WantArrow = " + arrow.ToString() + " SlipArrow = " + slipArrow.ToString());
        }
        else
        {
            switch (arrow)
            {
                case Arrow.RIGHT:
                    OnMoveRight();
                    break;
                case Arrow.LEFT:
                    OnMoveLeft();
                    break;
                case Arrow.UP:
                    OnMoveUp();
                    break;
                case Arrow.DOWN:
                    OnMoveDown();
                    break;
            }

            learningMgr.exploration /= (learningMgr.tryTotal + 1);
        }
    }

    private void MaxValueArrow(Collider2D _nowTile)
    {
        if (_nowTile == null)
            Debug.LogError("Func:MaxValueArrow parameter value is null");

        float max = -10f;
        float right = _nowTile.GetComponent<Tile>().right + (Random.Range(0f, 1f) / (learningMgr.tryTotal + 1));
        float left = _nowTile.GetComponent<Tile>().left + (Random.Range(0f, 1f) / (learningMgr.tryTotal + 1));
        float up = _nowTile.GetComponent<Tile>().up + (Random.Range(0f, 1f) / (learningMgr.tryTotal + 1));
        float down = _nowTile.GetComponent<Tile>().down + (Random.Range(0f, 1f) / (learningMgr.tryTotal + 1));

        if (right >= max)
        {
            max = right;
            arrow = Arrow.RIGHT;
        }

        if (left >= max)
        {
            max = left;
            arrow = Arrow.LEFT;
        }

        if (up >= max)
        {
            max = up;
            arrow = Arrow.UP;
        }

        if (down >= max)
        {
            max = down;
            arrow = Arrow.DOWN;
        }

        switch (arrow)
        {
            case Arrow.RIGHT:
                OnMoveRight();
                break;
            case Arrow.LEFT:
                OnMoveLeft();
                break;
            case Arrow.UP:
                OnMoveUp();
                break;
            case Arrow.DOWN:
                OnMoveDown();
                break;
        }
    }

    private void OnMoveRight()
    {
        transform.position = new Vector3(transform.position.x + 1, transform.position.y, -1);
        logArrowList.Add(Arrow.RIGHT);
    }

    private void OnMoveLeft()
    {
        transform.position = new Vector3(transform.position.x - 1, transform.position.y, -1);
        logArrowList.Add(Arrow.LEFT);
    }

    private void OnMoveUp()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y + 1, -1);
        logArrowList.Add(Arrow.UP);
    }

    private void OnMoveDown()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y - 1, -1);
        logArrowList.Add(Arrow.DOWN);
    }
}
