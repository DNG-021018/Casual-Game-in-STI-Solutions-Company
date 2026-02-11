using UnityEngine;

namespace Bowmancer
{
    public class B_DontDestroyOnLoad : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
