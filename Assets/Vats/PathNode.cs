using UnityEngine;

public class PathNode : MonoBehaviour
{
    public bool isActive = false;
    public int NodeLayer = 10;
    public bool markedForDeletion = false;

    private void Start()
    {
        Invoke(nameof(DelayedCall), 0.1f);
    }

    void OnDrawGizmos()
    {
        float radius = transform.localScale.x / 2; // Radius of the sphere
        Gizmos.color = Color.red; // Color of the sphere
        Gizmos.DrawWireSphere(transform.position, radius); // Draw wireframe sphere at object's position
    }

    void DelayedCall()
    {
        Collider[] results = new Collider[2];
        int overlapCount = Physics.OverlapSphereNonAlloc(transform.position, transform.localScale.x / 2, results, 1 << NodeLayer);

        if (overlapCount > 0)
        {
            for (int i = 0; i < results.Length; i++)
            {
                if (results[i] != null)
                {
                    if (results[i].gameObject != gameObject)
                    {
                        if (!markedForDeletion) results[i].GetComponent<PathNode>().markedForDeletion = true;
                    }
                }
            }

            for (int i = 0; i < results.Length; i++)
            {
                if (results[i] != null)
                {
                    if (results[i].gameObject != gameObject)
                    {
                        if (results[i].GetComponent<PathNode>().markedForDeletion)
                        {
                            Destroy(results[i].gameObject);
                        }
                    }
                }
            }
        }
    }
}