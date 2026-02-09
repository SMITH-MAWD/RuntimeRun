using UnityEngine;
using Cinemachine;

public class camwide : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float wideFieldOfView = 80f;
    [SerializeField] private float normalFieldOfView = 60f;
    [SerializeField] private float transitionSpeed = 5f;
    [SerializeField] private string playerTag = "Player";
    
    private bool isWide = false;
    private float targetFOV;

    private void Start()
    {
        if (virtualCamera == null)
        {
            virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        }
        
        targetFOV = normalFieldOfView;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isWide = true;
            targetFOV = wideFieldOfView;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isWide = false;
            targetFOV = normalFieldOfView;
        }
    }

    private void Update()
    {
        if (virtualCamera != null)
        {
            // Smoothly transition to target FOV
            virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(
                virtualCamera.m_Lens.FieldOfView, 
                targetFOV, 
                Time.deltaTime * transitionSpeed
            );
        }
    }
}
