using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    // public static LoadingManager instance;
    
    private static readonly int LoadingStart = Animator.StringToHash("LoadingStart");
    private static readonly int LoadingEnd = Animator.StringToHash("LoadingEnd");
    
    [SerializeField] Animator anim;
    // [SerializeField] private Slider loadingBar;

    private void Start()
    {
        /*if (loadingBar != null)
            loadingBar.value = 100f;*/
    }

    public void LoadScene(int sceneId)
    {
        StartCoroutine(LoadSceneAsync(sceneId));
    }

    /*public void LoadLastScene()
    {
        Debug.Log("Loading last scene: " + GameManager.Instance.lastSceneIndex);
        StartCoroutine(LoadSceneAsync(GameManager.Instance.lastSceneIndex));
    }*/

    IEnumerator LoadSceneAsync(int sceneId)
    {
        // GameManager.Instance.lastSceneIndex = SceneManager.GetActiveScene().buildIndex;
        
        // loadingBar.value = 0f;
        
        anim.SetTrigger(LoadingStart);
        yield return new WaitForSeconds(1);
        AsyncOperation loadingOperation = SceneManager.LoadSceneAsync(sceneId);

        if (loadingOperation != null)
        {
            loadingOperation.allowSceneActivation = false;

            do
            {
                // loadingBar.value = loadingOperation.progress * 100;
                yield return new WaitForSeconds(0.1f);
            } while (loadingOperation.progress < 0.9f);

            // loadingBar.value = 100f;
            yield return new WaitForSeconds(1);
                
            loadingOperation.allowSceneActivation = true;
            
            // if player exists in the scene, add Noah to the game manager
        }

        anim.SetTrigger(LoadingEnd);
    }
}