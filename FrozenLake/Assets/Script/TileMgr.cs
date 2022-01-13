using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMgr : MonoBehaviour
{
    public Collider2D FindTileCollider2D(int tileID)
    {
        GameObject temp = GameObject.Find("TileGround_" + tileID);
        return temp.GetComponent<Collider2D>();
    }
}
