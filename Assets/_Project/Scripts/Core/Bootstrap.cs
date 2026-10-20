using UnityEngine;
using UnityEngine.SceneManagement;

namespace DarkDepths.Core
{
    // Entry point of the Boot scene: service registration lands here, then control passes to the next scene.
    public sealed class Bootstrap : MonoBehaviour
    {
        [SerializeField] private string _nextScene = string.Empty;

        private void Start()
        {
            if (!string.IsNullOrEmpty(_nextScene))
            {
                SceneManager.LoadScene(_nextScene);
            }
        }
    }
}
