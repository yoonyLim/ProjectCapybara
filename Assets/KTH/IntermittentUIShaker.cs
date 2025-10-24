using UnityEngine;
using DG.Tweening; // DOTween 네임스페이스 추가

public class IntermittentUIShaker : MonoBehaviour
{
    [Header("흔들림 설정")]
    public float shakeDuration = 0.5f; // 한 번 흔들리는 데 걸리는 시간
    public float shakeStrength = 10f;  // 흔들림의 강도 (위치 값)
    public int vibrato = 10;         // 흔들림의 빈도 (얼마나 자잘하게 떨리는지)

    [Header("간격 설정")]
    public float pauseInterval = 2.0f; // 흔들림 사이의 대기 시간

    private RectTransform rectTransform;

    void Start()
    {
        // 이 스크립트가 붙어있는 게임오브젝트의 RectTransform을 가져옴
        rectTransform = GetComponent<RectTransform>();

        // DOTween 시퀀스 생성
        Sequence shakeSequence = DOTween.Sequence();

        // 1. 지정된 시간(shakeDuration) 동안 위치(AnchorPos)를 흔듦
        shakeSequence.Append(rectTransform.DOShakeAnchorPos(
            shakeDuration,
            shakeStrength,
            vibrato
        ));

        // 2. 흔들림이 끝난 후, 지정된 시간(pauseInterval) 동안 대기
        shakeSequence.AppendInterval(pauseInterval);

        // 3. 시퀀스 전체를 무한 반복 (-1은 무한을 의미)
        shakeSequence.SetLoops(-1);
    }
}