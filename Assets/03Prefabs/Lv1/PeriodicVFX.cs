using System.Collections;
using UnityEngine;

public class PeriodicVFX : MonoBehaviour
{
    // Inspector 창에서 제어할 변수들
    public ParticleSystem vfx; // 주기적으로 켤 VFX (파티클 시스템)
    public float activeDuration = 2f; // VFX가 켜져 있는 시간 (초)
    public float inactiveDuration = 3f; // VFX가 꺼져 있는 시간 (초)

    // 게임이 시작될 때 코루틴을 실행합니다.
    void Start()
    {
        if (vfx != null)
        {
            StartCoroutine(VfxCycle());
        }
    }

    // VFX를 켜고 끄는 것을 무한히 반복하는 코루틴
    IEnumerator VfxCycle()
    {
        while (true) // 무한 반복
        {
            // 1. VFX 켜기
            vfx.Play();

            // 2. 'activeDuration'만큼 기다리기
            yield return new WaitForSeconds(activeDuration);

            // 3. VFX 끄기
            vfx.Stop();

            // 4. 'inactiveDuration'만큼 기다리기
            yield return new WaitForSeconds(inactiveDuration);
        }
    }
}