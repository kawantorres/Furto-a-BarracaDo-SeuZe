using UnityEngine;

public interface IMunicaoEstilingue
{
    void Atirar(Transform pontoDeDisparo);
}

public class AtiraPedraEstilingue : MonoBehaviour, IMunicaoEstilingue
{
    [Header("Configurações do Projétil")]
    public float forcaDisparo = 25f; // Pedras são mais leves e rápidas
    public float elevacao = 0.05f;  // Pouco arco

    public void Atirar(Transform pontoDeDisparo)
    {
        // Pega a direção que a câmera está apontando, se não houver câmera usa o forward do ponto de disparo
        Vector3 direcao = Camera.main != null ? Camera.main.transform.forward : pontoDeDisparo.forward;
        
        GameObject pedra = Instantiate(Resources.Load("Pedra", typeof(GameObject))) as GameObject;
        if (pedra != null)
        {
            pedra.transform.position = pontoDeDisparo.position;
            Rigidbody rb = pedra.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = (direcao + Vector3.up * elevacao).normalized * forcaDisparo;
            }
        }
    }
}

/* 
// RESERVADO PARA O FUTURO: Lança Batata (AtiraBatataEstilingue)
public class AtiraBatataEstilingue : MonoBehaviour, IMunicaoEstilingue
{
    [Header("Configurações do Projétil")]
    public float forcaDisparo = 20f;

    public void Atirar(Transform pontoDeDisparo)
    {
        Vector3 direcao = Camera.main != null ? Camera.main.transform.forward : pontoDeDisparo.forward;
        GameObject batata = Instantiate(Resources.Load("Batata", typeof(GameObject))) as GameObject;
        if (batata != null)
        {
            batata.transform.position = pontoDeDisparo.position;
            Rigidbody rb = batata.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = direcao * forcaDisparo;
            }
        }
    }
}
*/

public class ControladorEstilingue : MonoBehaviour
{
    private IMunicaoEstilingue tipoMunicao;
    
    [Header("Ponto de Saída do Projétil")]
    public Transform pontoDeDisparo; // Se nulo, usa a posição do próprio estilingue

    void Start()
    {
        // Começa com o disparo de Pedra por padrão
        MudarMunicao<AtiraPedraEstilingue>();
        
        if (pontoDeDisparo == null)
        {
            pontoDeDisparo = transform;
        }
    }

    void Update()
    {
        // Atirar com Botão Esquerdo do Mouse ou Tecla Espaço
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            Atirar();
        }

        /*
        // ALTERNÂNCIA DE MUNIÇÃO (Comentado até adicionarmos o Lança Batata)
        if (Input.GetKeyDown(KeyCode.B))
        {
            MudarMunicao<AtiraPedraEstilingue>();
            Debug.Log("Munição alterada para: Pedra");
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            // MudarMunicao<AtiraBatataEstilingue>();
            // Debug.Log("Munição alterada para: Batata");
        }
        */
    }

    void MudarMunicao<T>() where T : MonoBehaviour, IMunicaoEstilingue
    {
        // Destrói a estratégia anterior
        IMunicaoEstilingue antiga = GetComponent<IMunicaoEstilingue>();
        if (antiga != null)
        {
            Destroy((MonoBehaviour)antiga);
        }

        // Adiciona a nova estratégia
        tipoMunicao = gameObject.AddComponent<T>();
    }

    void Atirar()
    {
        if (tipoMunicao != null)
        {
            tipoMunicao.Atirar(pontoDeDisparo);
        }
    }
}
