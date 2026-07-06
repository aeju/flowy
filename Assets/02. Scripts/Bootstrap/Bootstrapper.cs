using UnityEngine;

namespace Flowy.Bootstrap
{
    public class Bootstrapper : MonoBehaviour
    {
        // 역할: 유일한 진입점. Logic 객체 생성 및 주입, tick 구동
        // 빈 껍데기 -> 기능 추가될 때마다 추가

        // Logic 객체 생성 + 서로 연결(주입)
        private void Awake()
        {
            Debug.Log("Bootstrap 시작");
        }

        private void Start()
        {
            Debug.Log("Start");
        }

        // 매 프레임 Logic의 Tick 구동
        private void Update()
        {
            Debug.Log("Update");
        }
    }
}

