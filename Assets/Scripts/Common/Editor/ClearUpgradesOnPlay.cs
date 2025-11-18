using UnityEditor;

[InitializeOnLoad]
static class ClearUpgradesOnPlay
{
    static ClearUpgradesOnPlay()
    {
        EditorApplication.playModeStateChanged += state => {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                // Chama o utilitário já existente em Upgrades
                Upgrades.ResetAllLevels();
            }
        };
    }
}