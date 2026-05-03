using UnityEngine;

[CreateAssetMenu(menuName = "Core/GameStateDataSO")]
public class GameStateDataSO : ScriptableObject
{
    public GameStateType type;
    [UnityEngine.Range(0f, 2f)] public float timeScale = 1f;
    public CursorLockMode cursorMode = CursorLockMode.None;
    public bool cursorVisible = true;
}
