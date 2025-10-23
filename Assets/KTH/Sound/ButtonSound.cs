using UnityEngine;
using UnityEngine.EventSystems; // UI 이벤트를 감지하기 위해 꼭 필요합니다.
using UnityEngine.UI;

/// <summary>
/// UI 버튼에 붙여서 네비게이션(포커스) 및 선택(클릭) 시 사운드를 재생하는 스크립트입니다.
/// Button 컴포넌트가 있는 게임 오브젝트에 추가해야 합니다.
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonSound : MonoBehaviour, ISelectHandler, ISubmitHandler
{
    [Header("Sound Names")]
    [Tooltip("컨트롤러나 키보드로 이 버튼에 포커스가 이동했을 때 재생할 사운드 이름")]
    [SerializeField] private string navigateSoundName = "ui_move"; // 기본 이동 사운드 이름

    [Tooltip("이 버튼을 클릭(선택)했을 때 재생할 사운드 이름")]
    [SerializeField] private string submitSoundName = "ui_select"; // 기본 선택 사운드 이름

    /// <summary>
    /// 이 UI 요소가 EventSystem에 의해 '선택'될 때(포커스될 때) 호출됩니다.
    /// </summary>
    public void OnSelect(BaseEventData eventData)
    {
        // navigateSoundName이 비어있지 않다면 SoundManager를 통해 사운드를 재생합니다.
        if (!string.IsNullOrEmpty(navigateSoundName) && SoundManager.instance != null)
        {
            SoundManager.instance.PlaySFX(navigateSoundName);
        }
    }

    /// <summary>
    /// 이 UI 요소가 '제출'(클릭)될 때 호출됩니다.
    /// </summary>
    public void OnSubmit(BaseEventData eventData)
    {
        // submitSoundName이 비어있지 않다면 SoundManager를 통해 사운드를 재생합니다.
        if (!string.IsNullOrEmpty(submitSoundName) && SoundManager.instance != null)
        {
            SoundManager.instance.PlaySFX(submitSoundName);
        }
    }
}
