using UnityEngine;

public class CrateBehaviour : SolidMonoBehaviour
{
    [SerializeField]
    private GameObject m_objectToSpawn;

    void Start()
    {
        OnHit += Destroy;
    }

    void Destroy(RaycastHit hit)
    {
        m_objectToSpawn.SetActive(true);
        Destroy(gameObject);
    }
}