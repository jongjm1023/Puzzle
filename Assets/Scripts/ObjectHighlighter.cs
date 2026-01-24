using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ObjectHighlighter : MonoBehaviour
{
    [Header("References")]
    [Tooltip("카메라 참조 (비어있으면 Main Camera 자동 사용)")]
    public Camera mainCamera;
    
    [Header("Highlight Settings")]
    [Tooltip("하이라이트 색상")]
    public Color highlightColor = Color.yellow;
    
    [Tooltip("하이라이트 강도")]
    public float highlightIntensity = 2f;
    
    [Tooltip("하이라이트할 레이어")]
    public LayerMask highlightLayer = -1; // 모든 레이어
    
    private Renderer lastHighlighted;
    private Material originalMaterial;
    private Material highlightMaterial;
    private TimeController timeController;
    
    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
            
        timeController = TimeController.Instance;
        
        // 하이라이트 머티리얼 생성
        highlightMaterial = new Material(Shader.Find("Standard"));
        highlightMaterial.EnableKeyword("_EMISSION");
        highlightMaterial.SetColor("_EmissionColor", highlightColor * highlightIntensity);
        
        // 코루틴 시작 (timeScale 영향 안받음)
        StartCoroutine(CheckHover());
    }
    
    IEnumerator CheckHover()
    {
        while (true)
        {
            // 시간 정지 상태에서만 작동 (TimeFreeze)
            if (StateManager.Instance.CurrentState() == State.TimeFreeze)
            {
                // 마우스 위치에서 레이캐스트
                Vector2 mousePos = Mouse.current.position.ReadValue();
                Ray ray = mainCamera.ScreenPointToRay(mousePos);
                
                // RaycastAll을 사용하여 non-convex collider도 감지
                RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, highlightLayer);
                
                if (hits.Length > 0)
                {
                    // 가장 가까운 hit 찾기
                    RaycastHit closestHit = hits[0];
                    float closestDistance = hits[0].distance;
                    
                    for (int i = 1; i < hits.Length; i++)
                    {
                        if (hits[i].distance < closestDistance)
                        {
                            closestDistance = hits[i].distance;
                            closestHit = hits[i];
                        }
                    }
                    
                    Renderer renderer = closestHit.collider.GetComponent<Renderer>();
                    
                    if (renderer != null && renderer != lastHighlighted)
                    {
                        // 이전 하이라이트 제거
                        if (lastHighlighted != null)
                            RemoveHighlight(lastHighlighted);
                        
                        // 새 하이라이트 적용
                        ApplyHighlight(renderer);
                        lastHighlighted = renderer;
                    }
                    
                    // 클릭 감지 (Input System은 timeScale 영향 안받음)
                    if (Mouse.current.leftButton.wasPressedThisFrame)
                    {
                        // 시간 재개 및 역행 시작
                        timeController.ResumeTime(closestHit.collider.gameObject);
                        
                        // 하이라이트 제거
                        if (lastHighlighted != null)
                        {
                            RemoveHighlight(lastHighlighted);
                            lastHighlighted = null;
                        }
                    }
                }
                else
                {
                    // 아무것도 안 맞으면 하이라이트 제거
                    if (lastHighlighted != null)
                    {
                        RemoveHighlight(lastHighlighted);
                        lastHighlighted = null;
                    }
                }
            }
            else
            {
                // 시간이 흐르는 중에는 하이라이트 제거
                if (lastHighlighted != null)
                {
                    RemoveHighlight(lastHighlighted);
                    lastHighlighted = null;
                }
            }
            
            yield return null; // 매 프레임 체크
        }
    }
    
    void ApplyHighlight(Renderer renderer)
    {
        if (renderer == null) return;
        
        // 원본 머티리얼 저장
        originalMaterial = renderer.sharedMaterial;
        
        if (originalMaterial == null)
        {
            Debug.LogWarning($"ObjectHighlighter: {renderer.gameObject.name}에 머티리얼이 없습니다!");
            return;
        }
        
        // 새 머티리얼 인스턴스 생성
        Material newMaterial = new Material(renderer.sharedMaterial);
        
        // 방법 1: 색상 직접 변경 (가장 확실한 방법)
        // _Color 또는 _BaseColor 또는 _MainColor 프로퍼티 찾기
        bool colorChanged = false;
        if (newMaterial.HasProperty("_Color"))
        {
            Color originalColor = newMaterial.GetColor("_Color");
            // 원본 색상과 하이라이트 색상을 블렌드
            newMaterial.SetColor("_Color", Color.Lerp(originalColor, highlightColor, 0.9f));
            colorChanged = true;
        }
        else if (newMaterial.HasProperty("_BaseColor"))
        {
            Color originalColor = newMaterial.GetColor("_BaseColor");
            newMaterial.SetColor("_BaseColor", Color.Lerp(originalColor, highlightColor, 0.9f));
            colorChanged = true;
        }
        else if (newMaterial.HasProperty("_MainColor"))
        {
            Color originalColor = newMaterial.GetColor("_MainColor");
            newMaterial.SetColor("_MainColor", Color.Lerp(originalColor, highlightColor, 0.9f));
            colorChanged = true;
        }
        
        // 방법 2: Emission도 시도 (Built-in Standard)
        if (newMaterial.HasProperty("_EmissionColor"))
        {
            newMaterial.EnableKeyword("_EMISSION");
            newMaterial.SetColor("_EmissionColor", highlightColor * highlightIntensity);
            newMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        else if (newMaterial.HasProperty("_Emission"))
        {
            newMaterial.EnableKeyword("_EMISSION");
            newMaterial.SetColor("_Emission", highlightColor * highlightIntensity);
        }
        
        // 머티리얼 적용
        renderer.material = newMaterial;
    }
    
    void RemoveHighlight(Renderer renderer)
    {
        if (renderer == null || originalMaterial == null) return;
        
        // 원본 머티리얼 복원
        renderer.material = originalMaterial;
    }
    
    void OnDestroy()
    {
        // 하이라이트 제거
        if (lastHighlighted != null)
            RemoveHighlight(lastHighlighted);
            
        // 머티리얼 정리
        if (highlightMaterial != null)
            Destroy(highlightMaterial);
    }
}
