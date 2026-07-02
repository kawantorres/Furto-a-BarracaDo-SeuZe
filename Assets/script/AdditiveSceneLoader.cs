using UnityEngine;
using UnityEngine.SceneManagement;

public class AdditiveSceneLoader : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "CoffeeShopInteriorDAY";

    private void Start()
    {
        
    if (!SceneManager.GetSceneByName("nome_da_cena").isLoaded) {
        SceneManager.LoadScene("nome_da_cena", LoadSceneMode.Additive);
    }
    }
}
