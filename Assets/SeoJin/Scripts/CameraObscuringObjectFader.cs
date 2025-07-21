using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// this script needs to be attached to Main Camera Object. It finds obscuring objects and make them fade.
/// </summary>
public class CameraObscuringObjectFader : MonoBehaviour
{

    [SerializeField] private LayerMask obscuringLayers;
    [SerializeField] private float checkInterval = 0.1f; // 매 프레임 업데이트하지 않고, Interval마다 Check
    [SerializeField] private int maxObjetsToFade = 10;

    private GameObject player;
    public Camera cam;

    private List<ObjectFader> fadedObjects = new List<ObjectFader>(); // 플레이어를 가리는 Object들을 저장하는 리스트
    
    void Awake()
    {
        cam = GetComponent<Camera>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Start()
    {
        StartCoroutine(CheckForObscuringObjects());
    }

    // 게임 시작 시 끝날 때까지 돌아가는 Coroutine
    private IEnumerator CheckForObscuringObjects()
    {
        yield return new WaitForSeconds(Random.Range(0, 0.5f)); // 서로 다른 프레임에 처리되도록 처리해줌
        
        while (true)
        {
            Vector3 dir = player.transform.position - cam.transform.position;

            RaycastHit[] hits = new RaycastHit[maxObjetsToFade];
            int hitCount = Physics.RaycastNonAlloc(cam.transform.position, dir.normalized, hits, dir.magnitude, obscuringLayers);

            List<ObjectFader> hitFadersThisFrame = new List<ObjectFader>();
            
            for (int i = 0; i < hitCount; i++)
            {
                if (hits[i].collider) // null 체크 추가
                {
                    ObjectFader fader;
                    if (hits[i].collider.TryGetComponent<ObjectFader>(out fader))
                        hitFadersThisFrame.Add(fader);
                }
            }

            // LINQ Except로 전에는 Fade에 있다가 현재는 없는애들 가져오기
            var objectsToUnfade = fadedObjects.Except(hitFadersThisFrame); 
            foreach (var fader in objectsToUnfade)
            {
                fader.DoFade = false;
            }

            foreach (var fader in hitFadersThisFrame)
            {
                fader.DoFade = true;
            }
            
            fadedObjects = hitFadersThisFrame;

            yield return new WaitForSeconds(checkInterval);
        }
    }
}


