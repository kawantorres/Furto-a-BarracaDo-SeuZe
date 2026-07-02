using UnityEngine;
using UnityEngine.SceneManagement;

public class AdditiveSceneLoader : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "CoffeeShopInteriorDAY";

    private void Start()
    {
        if (!SceneManager.GetSceneByName(sceneToLoad).isLoaded)
        {
            SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Additive);
        }
    }
}
