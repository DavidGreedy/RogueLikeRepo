using System.Collections;
using UnityEngine;

public class AnimatedTexture : MonoBehaviour
{
    [SerializeField]
    private int[] indices;

    public int _uvTileX = 1;
    public int _uvTileY = 1;

    public int tileX;
    public int tileY;

    public int m_framesPerSecond = 1;

    private Vector2 _size;
    private Renderer _myRenderer;

    [SerializeField]
    private bool animate;

    private int index;

    void Start()
    {
        _size = new Vector2(1.0f / _uvTileX, 1.0f / _uvTileY);
        _myRenderer = GetComponent<Renderer>();
        if (_myRenderer == null)
            enabled = false;

        StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        while (animate)
        {
            int uIndex = indices[index] % _uvTileX;
            int vIndex = indices[index] / _uvTileY;
            Vector2 offset = new Vector2(uIndex * _size.x, 1.0f - _size.y - vIndex * _size.y);
            _myRenderer.material.SetTextureOffset("_MainTex", offset);
            _myRenderer.material.SetTextureScale("_MainTex", _size);

            index = index == 4 ? 0 : index + 1;
            yield return new WaitForSeconds(1.0f / m_framesPerSecond);
        }
        print("DONW");
        yield return null;
    }
}