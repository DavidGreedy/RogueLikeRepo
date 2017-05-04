using UnityEngine;
using System.Collections;

public class BlockGrass : Block
{
    public override Tile TexturePosition(DIRECTION direction)
    {
        Tile tile = new Tile();
        switch (direction)
        {
            case DIRECTION.D_UP:
            tile.x = 2;
            tile.y = 0;
            return tile;
            case DIRECTION.D_DOWN:
            tile.x = 1;
            tile.y = 0;
            return tile;
        }
        tile.x = 3;
        tile.y = 0;
        return tile;
    }
}