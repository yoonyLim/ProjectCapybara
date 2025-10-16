// RidableAnimal.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Ridable : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;
    private Collider col;
    private bool isMounted = false;

    //박쥐는 투명화 능력 있으니 따로 변수로 표시
    public bool isBat = false;
    public string colorPropertyName = "_BaseColor";
    //private Renderer modelRenderer; // 동물의 렌더러 (SkinnedMeshRenderer 또는 MeshRenderer)
    private List<Material> allLODMaterials = new List<Material>();
    private Renderer[] allRenderers;

    private Material animalMaterial;  // 동물의 재질 인스턴스
    private Coroutine fadeCoroutine;  // 현재 실행 중인 페이드 코루틴을 저장
    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();


        allRenderers = GetComponentsInChildren<Renderer>();

        if (allRenderers.Length > 0)
        {
            foreach (Renderer lodRenderer in allRenderers)
            {
                allLODMaterials.AddRange(lodRenderer.materials);
            }
        }
        //modelRenderer = GetComponentInChildren<Renderer>();
        //if (modelRenderer != null)
        //{
        //    // 공유 재질(Shared Material)이 아닌 인스턴스 재질을 사용해야
        //    // 이 오브젝트에만 변경사항이 적용됩니다.
        //    animalMaterial = modelRenderer.material;
        //}

        // 만약 특수 능력 동물이라면, 게임 시작 시 완전히 투명하게 만듭니다.

    }

    // 탑승
    public void Mount(Transform mountPoint)
    {
        if (isMounted) return;

        if (isBat)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            // 1.5초에 걸쳐 완전히 투명하게(알파값 0) 만듭니다. 시간은 조절 가능합니다.
            fadeCoroutine = StartCoroutine(FadeTo(0.2f, 1.5f));
        }

        isMounted = true;

        // 물리적 충돌을 방지하기 위해 비활성화
        rb.isKinematic = true;
        col.enabled = false;

        // 동물을 플레이어의 탑승 위치에 자식으로 설정
        transform.SetParent(mountPoint);

        // 탑승 위치에 정확히 앉도록 로컬 위치와 회전 초기화
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    // 내림
    public void Dismount()
    {
        if (!isMounted) return;

        isMounted = false;

        if (isBat)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            SetMaterialAlpha(1f); // 알파값을 1로 만들어 완전히 보이게 함
            SetShadowMode(true);
        }

        // 부모/자식 관계 해제
        transform.SetParent(null);

        // 물리 기능 다시 활성화
        col.enabled = true;

        // 선택 사항: 즉시 다시 탑승하는 것을 방지하기 위해 살짝 뒤로 이동
        transform.position += transform.forward * -1.0f;
    }

    /// <summary>
    /// 점프 애니메이션을 실행합니다. 'Jump'라는 이름의 트리거 파라미터가 있다고 가정합니다.
    /// </summary>
    public void TriggerJump()
    {
        if (isMounted)
        {
            animator.SetTrigger("Jump");
            StartCoroutine(HopAnimation());
        }
    }

    /// <summary>
    /// 비행 애니메이션 상태를 설정합니다. 'isFly'라는 이름의 bool 파라미터가 있다고 가정합니다.
    /// </summary>
    public void SetFlying(bool isFlying)
    {
        if (isMounted)
        {
            animator.SetBool("isFly", isFlying);
        }
    }

    // 탑승한 동물도 같이 점프
    private IEnumerator HopAnimation()
    {
        float jumpHeight = 0.6f; // 동물이 살짝 뛰어오를 높이
        float jumpDuration = 1.5f; // 뛰어오르는 데 걸리는 시간
        Vector3 originalPos = Vector3.zero; // 탑승 위치의 기본값은 (0,0,0)
        Vector3 targetPos = new Vector3(0, jumpHeight, 0);

        // 올라가는 움직임
        for (float t = 0; t < 1; t += Time.deltaTime / (jumpDuration / 2))
        {
            transform.localPosition = Vector3.Lerp(originalPos, targetPos, t);
            yield return null;
        }

        // 내려오는 움직임
        for (float t = 0; t < 1; t += Time.deltaTime / (jumpDuration / 2))
        {
            transform.localPosition = Vector3.Lerp(targetPos, originalPos, t);
            yield return null;
        }

        transform.localPosition = originalPos; // 정확한 위치로 보정
    }


    /// <summary>
    /// 특수 능력을 발동시키는 외부 호출용 메서드
    /// </summary>
    /// <param name="visibleDuration">능력이 지속될 시간(나타나 있는 시간)</param>
    /// <param name="fadeInTime">나타나는 데 걸리는 시간</param>
    /// <param name="fadeOutTime">사라지는 데 걸리는 시간</param>
    public void UseSpecialAbility(float visibleDuration, float fadeInTime, float fadeOutTime)
    {
        // 특수 능력 동물이 아니면 아무것도 하지 않음
        if (!isBat) return;

        // 이전에 실행 중이던 페이드 코루틴이 있다면 중지
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        // 새로운 페이드 시퀀스 시작
        fadeCoroutine = StartCoroutine(FadeSequence(visibleDuration, fadeInTime, fadeOutTime));
    }

    /// <summary>
    /// 나타나기 -> 유지 -> 사라지기 순서로 페이드 효과를 제어하는 코루틴
    /// </summary>
    private IEnumerator FadeSequence(float visibleDuration, float fadeInTime, float fadeOutTime)
    {
        // Fade In (서서히 나타나기)
        yield return StartCoroutine(FadeTo(1f, fadeInTime));

        // 능력 지속 시간만큼 대기
        yield return new WaitForSeconds(visibleDuration);

        // Fade Out (서서히 사라지기)
        yield return StartCoroutine(FadeTo(0.2f, fadeOutTime));
    }

    /// <summary>
    /// 지정된 시간 동안 목표 알파 값으로 색상을 변경하는 코루틴
    /// </summary>
    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        // material.color 대신 GetColor를 사용합니다.
        if (allLODMaterials.Count == 0) yield break;

        if (targetAlpha > 0.01f)
        {
            SetShadowMode(true);
        }

        Color startColor = allLODMaterials[0].GetColor(colorPropertyName);
        float startAlpha = startColor.a;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            SetMaterialAlpha(newAlpha); // 수정된 SetMaterialAlpha를 호출
            yield return null;
        }

        SetMaterialAlpha(targetAlpha);

        if (targetAlpha < 0.01f)
        {
            //SetShadowMode(false);
        }
    }

    /// <summary>
    /// 재질의 알파 값을 안전하게 설정하는 헬퍼 메서드
    /// </summary>
    private void SetMaterialAlpha(float alpha)
    {
        if (allLODMaterials.Count == 0) return;

        foreach (Material mat in allLODMaterials)
        {
            Color color = mat.GetColor(colorPropertyName);
            mat.SetColor(colorPropertyName, new Color(color.r, color.g, color.b, alpha));
        }
    }

    private void SetShadowMode(bool enabled)
    {
        // enabled가 true이면 On, false이면 Off
        ShadowCastingMode mode = enabled ? ShadowCastingMode.On : ShadowCastingMode.Off;

        if (allRenderers != null && allRenderers.Length > 0)
        {
            foreach (Renderer renderer in allRenderers)
            {
                renderer.shadowCastingMode = mode;
            }
        }
    }
}