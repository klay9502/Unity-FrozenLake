using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class oldAgent : MonoBehaviour
{
    private enum Arrow { Right, Left, Up, Down, Error };
    private Arrow arrow;
    private Collider2D nowTile;
    private Collider2D oldTile;
    private Collider2D StartTile;
    private Vector3 StartPos;
    private bool checkWall = false;
    private bool checkHole = false;
    private int state_right;
    private int state_left;
    private int state_up;
    private int state_down;

    LearningMgr learningMgr;

    void Start()
    {
        state_right = -1;
        state_left = -1;
        state_up = -1;
        state_down = -1;
        nowTile = null;
        oldTile = null;
        StartTile = null;

        learningMgr = new LearningMgr();
    }

    void Update()
    {
        DevProcess();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnProcess();
        }
    }

    void OnTriggerEnter2D (Collider2D other)
    {
        // Debug.Log(other.gameObject.name);

        if (StartTile == null)
        {
            StartTile = other;
            StartPos = gameObject.transform.position;
        }

        if (other.gameObject.name == "Wall")
        {
            checkWall = true;
            OnMove();
        }
        else if (other.gameObject.GetComponent<Tile>().type == Tile.TileType.Hole)
        {
            checkHole = true;
            OnMove();
        }
        else if (other.gameObject.GetComponent<Tile>().type == Tile.TileType.Goal)
        {
            //transform.position = StartPos;
        }
        else
        {
            nowTile = other;
            checkWall = false;
            checkHole = false;
            //state_right = other.GetComponent<Tile>().right;
            //state_left = other.GetComponent<Tile>().left;
            //state_up = other.GetComponent<Tile>().up;
            //state_down = other.GetComponent<Tile>().down;
        }

        OnTileUpdate(MaxArrow(state_right, state_left, state_up, state_down));
    }

    void OnTriggerExit2D(Collider2D old)
    {
        Debug.Log(old.gameObject.name);
        oldTile = old;
    }

    void DevProcess()
    {
        arrow = Arrow.Error;

        if (Input.GetKeyDown(KeyCode.RightArrow))
            arrow = Arrow.Right;
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            arrow = Arrow.Left;
        if (Input.GetKeyDown(KeyCode.UpArrow))
            arrow = Arrow.Up;
        if (Input.GetKeyDown(KeyCode.DownArrow))
            arrow = Arrow.Down;

        OnMove();
    }

    void OnProcess()
    {
        float exploration = Random.Range(0f, 1f);
        int WeightRight = Random.Range(0, 100);
        int WeightLeft = Random.Range(0, 100);
        int WeightUp = Random.Range(0, 100);
        int WeightDown = Random.Range(0, 100);
        arrow = Arrow.Error;

        int max = -1;

        if (exploration <= learningMgr.exploration || zeroCount(state_right, state_left, state_up, state_down))
        {
            Debug.Log("RANDOM");
            if (WeightRight > max)
            {
                max = WeightRight;
                arrow = Arrow.Right;
            }

            if (WeightLeft > max)
            {
                max = WeightLeft;
                arrow = Arrow.Left;
            }

            if (WeightUp > max)
            {
                max = WeightUp;
                arrow = Arrow.Up;
            }

            if (WeightDown > max)
            {
                arrow = Arrow.Down;
            }

            Debug.Log(exploration);
            // Debug.Log("Weight :" + WeightRight + ", " + WeightLeft + ", " + WeightUp + ", " + WeightDown);
        }
        else
        {
            Debug.Log("ARGMAX");
            if (state_right > max)
            {
                max = state_right;
                arrow = Arrow.Right;
            }

            if (state_left > max)
            {
                max = state_left;
                arrow = Arrow.Left;
            }

            if (state_up > max)
            {
                max = state_up;
                arrow = Arrow.Up;
            }

            if (state_down > max)
            {
                arrow = Arrow.Down;
            }
        }

        Debug.Log(arrow);
        OnMove();
    }

    void OnMove()
    {
        switch (arrow)
        {
            case Arrow.Right:
                transform.position = new Vector3(transform.position.x + 1, transform.position.y, -1);

                if (checkWall)
                {
                    checkWall = false;
                    transform.position = new Vector3(transform.position.x - 2, transform.position.y, -1);
                    nowTile.GetComponent<Tile>().right -= 1;
                }

                if (checkHole)
                    nowTile.GetComponent<Tile>().right -= 1;

                break;
            case Arrow.Left:
                transform.position = new Vector3(transform.position.x - 1, transform.position.y, -1);

                if (checkWall)
                {
                    checkWall = false;
                    transform.position = new Vector3(transform.position.x + 2, transform.position.y, -1);
                    nowTile.GetComponent<Tile>().left -= 1;
                }

                if (checkHole)
                    nowTile.GetComponent<Tile>().left -= 1;

                break;
            case Arrow.Up:
                transform.position = new Vector3(transform.position.x, transform.position.y + 1, -1);

                if (checkWall)
                {
                    checkWall = false;
                    transform.position = new Vector3(transform.position.x, transform.position.y - 2, -1);
                    nowTile.GetComponent<Tile>().up -= 1;
                }

                if (checkHole)
                    nowTile.GetComponent<Tile>().up -= 1;

                break;
            case Arrow.Down:
                transform.position = new Vector3(transform.position.x, transform.position.y - 1, -1);

                if (checkWall)
                {
                    checkWall = false;
                    transform.position = new Vector3(transform.position.x, transform.position.y + 2, -1);
                    nowTile.GetComponent<Tile>().down -= 1;
                }

                if (checkHole)
                    nowTile.GetComponent<Tile>().down -= 1;

                break;
            default:
                Debug.Log("ERROR :: Arrow value is ERROR!");
                break;
        }

        if (checkHole)
        {
            transform.position = StartPos;
            oldTile = null;
        }
    }

    int MaxValue(int right, int left, int up, int down)
    {
        int max = -1;

        if (right >= max)
            max = right;

        if (left >= max)
            max = left;

        if (up >= max)
            max = up;

        if (down >= max)
            max = down;

        return max;
    }

    Arrow MaxArrow(int right, int left, int up, int down)
    {
        int max = -1;
        Arrow temp = Arrow.Error;

        if (right >= max)
        {
            max = right;
            temp = Arrow.Right;
        }

        if (left >= max)
        {
            max = left;
            temp = Arrow.Left;
        }

        if (up >= max)
        {
            max = up;
            temp = Arrow.Up;
        }

        if (down >= max)
        {
            temp = Arrow.Down;
        }

        return temp;
    }

    void OnTileUpdate(Arrow arrow)
    {
        //switch (arrow)
        //{
        //    case Arrow.Right:
        //        oldTile.GetComponent<Tile>().right = MaxValue(nowTile.GetComponent<Tile>().right, nowTile.GetComponent<Tile>().left, nowTile.GetComponent<Tile>().up, nowTile.GetComponent<Tile>().down);
        //        break;
        //    case Arrow.Left:
        //        oldTile.GetComponent<Tile>().left = MaxValue(nowTile.GetComponent<Tile>().right, nowTile.GetComponent<Tile>().left, nowTile.GetComponent<Tile>().up, nowTile.GetComponent<Tile>().down);
        //        break;
        //    case Arrow.Up:
        //        oldTile.GetComponent<Tile>().up = MaxValue(nowTile.GetComponent<Tile>().right, nowTile.GetComponent<Tile>().left, nowTile.GetComponent<Tile>().up, nowTile.GetComponent<Tile>().down);
        //        break;
        //    case Arrow.Down:
        //        oldTile.GetComponent<Tile>().down = MaxValue(nowTile.GetComponent<Tile>().right, nowTile.GetComponent<Tile>().left, nowTile.GetComponent<Tile>().up, nowTile.GetComponent<Tile>().down);
        //        break;
        //}
    }

    bool zeroCount(int right, int left, int up, int down)
    {
        int count = 0;

        if (right == 0)
            count++;
        if (left == 0)
            count++;
        if (up == 0)
            count++;
        if (down == 0)
            count++;

        if (count > 1)
            return true;
        else
            return false;
    }
}
