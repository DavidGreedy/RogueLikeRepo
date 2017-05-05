using UnityEngine;
using System.Collections;

public class Modify : MonoBehaviour
{
    Vector2 rot;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 100))
            {
                print(transform.position);
                Terrain.SetBlock(hit, new BlockAir());
            }
        }
    }
}