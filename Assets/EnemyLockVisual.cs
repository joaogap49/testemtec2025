using UnityEngine;

[DisallowMultipleComponent]
public class EnemyLockVisual : MonoBehaviour
{
    [Header("Visuals (outline OR aura)")]
    [Tooltip("Material usado para 'outline' — será aplicado nas slots de material durante o lock")]
    public Material outlineMaterial;

    [Tooltip("GameObject filho com a aura/circle (opcional). Ative/desative para mostrar o lock.)")]
    public GameObject lockAura;

    // cache de renderers e materiais originais
    private Renderer[] renderers;
    private Material[][] originalMaterials;

    private void Awake()
    {
        // pega todos os Renderers (MeshRenderer e SkinnedMeshRenderer) na hierarquia do inimigo (inclui children)
        renderers = GetComponentsInChildren<Renderer>(true);

        // guarda cópias dos arrays de materiais originais (para restaurar depois)
        originalMaterials = new Material[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
        {
            // Clone das referências dos materiais originais (não alteramos sharedMaterials)
            originalMaterials[i] = renderers[i].materials;
        }

        if (lockAura != null)
            lockAura.SetActive(false);
    }

    // chama para ativar/desativar o lock visual
    public void SetLocked(bool locked)
    {
        // se escolher usar aura (recomendado para isométrico), usar isso: simples e barato
        if (lockAura != null)
        {
            lockAura.SetActive(locked);
        }

        // se houver outlineMaterial atribuído, substitui todos os materiais por esse material
        if (outlineMaterial != null)
        {
            if (locked)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    // cria um array do mesmo tamanho que o número de material slots e preenche com outlineMaterial
                    Material[] mats = new Material[renderers[i].materials.Length];
                    for (int m = 0; m < mats.Length; m++) mats[m] = outlineMaterial;
                    renderers[i].materials = mats; // atribui instance materials (bom para efeito único no inimigo)
                }
            }
            else
            {
                // restaura os materiais originais
                for (int i = 0; i < renderers.Length; i++)
                {
                    // restaura cópia salva anteriormente
                    renderers[i].materials = originalMaterials[i];
                }
            }
        }
    }

    private void OnDisable()
    {
        // garante que, se o objeto for desativado, restaura materiais e esconde aura
        if (renderers != null && originalMaterials != null)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    renderers[i].materials = originalMaterials[i];
            }
        }
        if (lockAura != null) lockAura.SetActive(false);
    }

    private void OnDestroy()
    {
        // mesma segurança na destruição
        OnDisable();
    }
}
