using UnityEngine;

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
        } else {
            Destroy(gameObject);
        }
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
