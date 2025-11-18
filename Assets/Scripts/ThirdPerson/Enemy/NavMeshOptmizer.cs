using UnityEngine;
using UnityEngine.AI;

public class NavMeshOptimizer : MonoBehaviour
{
    [Range(0.1f, 1.0f)]
    public float navMeshUpdateInterval = 0.3f;

    void Start()
    {
        // Reduz a frequência de updates do NavMesh se necessário
        NavMesh.pathfindingIterationsPerFrame = 100;

        // Log para debug
        Debug.Log($"NavMesh Otimizado - Update Interval: {navMeshUpdateInterval}");
    }
}