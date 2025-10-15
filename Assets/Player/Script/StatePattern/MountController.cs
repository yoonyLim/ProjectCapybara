// MountController.cs
using UnityEngine;

public class MountController : MonoBehaviour
{
    [Header("탑승 설정")]
    [Tooltip("동물이 앉을 플레이어 위의 빈 게임 오브젝트입니다.")]
    [SerializeField] private Transform mountPoint;

    [Tooltip("탑승 가능한 동물을 감지할 반경입니다.")]
    [SerializeField] private float interactionRadius = 2.0f;

    [Tooltip("탑승 가능한 동물이 속한 레이어입니다.")]
    [SerializeField] private LayerMask ridableLayer;

    private Ridable currentRider;

    void Update()
    {
        // 'R' 키 입력을 확인하여 탑승 또는 하차를 실행합니다.
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (currentRider == null)
            {
                TryMount();
            }
            else
            {
                Dismount();
            }
        }

        if (currentRider != null && Input.GetKeyDown(KeyCode.T))
        {
            // 탑승한 동물의 특수 능력 메서드 호출
            // (나타나 있는 시간 2초, 나타나는 시간 0.5초, 사라지는 시간 1.5초)
            currentRider.UseSpecialAbility(1f, 0.3f, 1.5f);
        }
    }

    private void TryMount()
    {
        // 지정된 레이어에서 주변 동물을 찾습니다.
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, interactionRadius, ridableLayer);

        if (nearbyColliders.Length > 0)
        {
            // 가장 가까운 동물로부터 RidableAnimal 컴포넌트를 가져옵니다.
            Ridable animal = nearbyColliders[0].GetComponent<Ridable>();
            if (animal != null)
            {
                currentRider = animal;
                currentRider.Mount(mountPoint);
            }
        }
    }

    private void Dismount()
    {
        if (currentRider != null)
        {
            currentRider.Dismount();
            currentRider = null;
        }
    }

    #region 애니메이션 동기화 메서드
    // 이 메서드들은 PlayerController에 의해 호출됩니다.

    public void OnPlayerJump()
    {
        currentRider?.TriggerJump();
    }

    public void OnPlayerGlide(bool isGliding)
    {
        currentRider?.SetFlying(isGliding);
    }
    #endregion

    // 에디터에서 상호작용 반경을 시각적으로 표시합니다.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}