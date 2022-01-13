using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public enum TileType { Field, Hole, Start, Goal };
    public TileType type;
    public int tileID;
    public float right;
    public float left;
    public float up;
    public float down;

    public GameObject UI_Text;

    // Start is called before the first frame update
    void Start()
    {
        switch (type)
        {
            case TileType.Hole:
                right = -1;
                left = -1;
                up = -1;
                down = -1;
                break;
            case TileType.Goal:
                right = 1;
                left = 1;
                up = 1;
                down = 1;
                break;
            default:
                right = 0;
                left = 0;
                up = 0;
                down = 0;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        UI_Text.transform.Find("right").GetComponent<Text>().text = right.ToString();
        UI_Text.transform.Find("left").GetComponent<Text>().text = left.ToString();
        UI_Text.transform.Find("up").GetComponent<Text>().text = up.ToString();
        UI_Text.transform.Find("down").GetComponent<Text>().text = down.ToString();
    }
}
