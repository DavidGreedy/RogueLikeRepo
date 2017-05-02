using UnityEngine;

[System.Serializable]
public class SpriteSheet
{
    public Sprite[] sprites;

    public Sprite this[int i]
    {
        get
        {
            return sprites[i];
        }
        set
        {
            sprites[i] = value;
        }
    }
}

public static class SpritePicker
{
    public static int[] conv =
    {
        20, 68, 92, 112, 28, 124, 116, 80, 21, 84, 87, 221, 127, 255, 241, 17, 29, 117, 85, 95, 247, 215, 209,
        1, 23, 213, 81, 31, 253, 125, 113, 16, 5, 69, 93, 119, 223, 255, 245, 65, 0, 4, 71, 193, 7, 199, 197, 64
    };

    public static int GetSpriteNumber(bool[] localTiles)
    {
        int spriteNum = 0;
        for (int i = 0; i < 8; i++)
        {
            if (localTiles[i] && (i % 2 == 0 || (localTiles[i - 1] && localTiles[i + 1 > 6 ? 0 : i + 1])))
            {
                spriteNum += (int)Mathf.Pow(2, i);
            }
        }
        return System.Array.IndexOf(conv, spriteNum);
        //return conv[spriteNum];

    }
    public static int GetSpriteNumber(int spriteNum)
    {
        return System.Array.IndexOf(conv, spriteNum);
        //return conv[spriteNum];
    }
}