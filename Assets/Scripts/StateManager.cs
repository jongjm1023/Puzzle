using UnityEngine;
using UnityEngine.SceneManagement;

public enum State
{
    Normal,
    TimeFreeze,
    TimeRewind,
    MapFlip,
}

public class StateManager : MonoBehaviour
{
    public static StateManager Instance;
    private State currentState = State.Normal;
    void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // 씬 로드 이벤트 구독
            SceneManager.sceneLoaded += OnSceneLoaded;
        } else {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 리로드 시 상태를 Normal로 초기화
        currentState = State.Normal;
    }

    public State CurrentState() => currentState;
        
    public bool CanUseAbility()
    {
        return currentState == State.Normal;
    }
    public void SetState(State newState)
    {
        currentState = newState;
    }
}
