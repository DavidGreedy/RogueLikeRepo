using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BitSet
{
    private int bits;

    public int Bits { get { return bits; } set { bits = value; } }

    public void Set(int bit, int value)
    {
        bits |= value << bit;
    }

    public void Set(int bit, bool value)
    {
        bits |= (value ? 1 : 0) << bit;
    }

    public void Clear()
    {
        bits = 0;
    }

    public void Toggle(int bit)
    {
        bits ^= 1 << bit;
    }

    public bool Get(int bit)
    {
        return ((bits >> bit) & 1) == 1;
    }

    public bool[] ToArray()
    {
        bool[] bools = new bool[sizeof(int) * 8];

        for (int i = 0; i < bools.Length; i++)
        {
            bools[i] = Get(i);
        }

        return bools;
    }

    public override string ToString()
    {
        string s = "";
        for (int i = 0; i < sizeof(int) * 8; i++)
        {
            s += Get(i) ? 1 : 0;
        }
        return s;
    }
}