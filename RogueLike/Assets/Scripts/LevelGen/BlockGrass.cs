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
            tile.x = 6;
            tile.y = 12;
            return tile;
            case DIRECTION.D_DOWN:
            tile.x = 6;
            tile.y = 12;
            return tile;
        }
        tile.x = 6;
        tile.y = 12;
        return tile;
    }
}