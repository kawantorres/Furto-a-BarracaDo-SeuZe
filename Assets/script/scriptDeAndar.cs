using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class scriptDeAndar : MonoBehaviour
{
    public float velocidade = 5f;
    public float gravidade = -9.81f;
    public float alturaPulo = 1.5f;

    [Header("Configurações do Skate")]
    public float velocidadeSkate = 15f;
    public float velocidadeRotacaoSkate = 100f;
    public Transform localDoSkateNoPe; 
    public Transform modeloDoPersonagem; 
    public float offsetAlturaSkate = 0.2f; 
    public bool noSkate = false;
    private scriptDoSkate skateEquipado;
    private Quaternion rotacaoOriginalModelo;

    [Header("Configurações do Estilingue")]
    public bool possuiEstilingue = false;
    public bool estilingueEquipado = false;
    public GameObject estilingueNaMao; 
    public GameObject miraUI;          
    public float distanciaColetaEstilingue = 3f;

    private CharacterController controller;
    private Vector3 velocidadeAtual;
    private bool noChao;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        noChao = controller.isGrounded;
        if (noChao && velocidadeAtual.y < 0)
        {
            velocidadeAtual.y = -2f;
        }

        if (noSkate)
        {
            MovimentoSkate();
            
            if (Input.GetKeyDown(KeyCode.E))
            {
                DescerDoSkate();
            }
        }
        else
        {
            MovimentoNormal();

            if (Input.GetKeyDown(KeyCode.E))
            {
                // Tenta coletar o estilingue primeiro. Se coletar, não sobe no skate neste frame.
                bool coletou = TentarColetarEstilingue();
                if (!coletou)
                {
                    TentarSubirNoSkate();
                }
            }
        }

        // Tecla 1 para Equipar/Desequipar o Estilingue
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (possuiEstilingue)
            {
                AlternarEstilingue();
            }
            else
            {
                Debug.LogWarning("Você ainda não possui o estilingue no inventário!");
            }
        }

        if (Input.GetButtonDown("Jump") && noChao)
        {
            velocidadeAtual.y = Mathf.Sqrt(alturaPulo * -2f * gravidade);
        }

        velocidadeAtual.y += gravidade * Time.deltaTime;
        controller.Move(velocidadeAtual * Time.deltaTime);
    }

    void TentarSubirNoSkate()
    {
        scriptDoSkate[] skates = FindObjectsByType<scriptDoSkate>();
        scriptDoSkate skateMaisProximo = null;
        float menorDistancia = 5f;

        foreach (scriptDoSkate skate in skates)
        {
            float distancia = Vector3.Distance(transform.position, skate.transform.position);
            if (distancia <= menorDistancia)
            {
                menorDistancia = distancia;
                skateMaisProximo = skate;
            }
        }

        if (skateMaisProximo != null)
        {
            SubirNoSkate(skateMaisProximo);
        }
    }

    void MovimentoNormal()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 movimento = transform.right * x + transform.forward * z;
        controller.Move(movimento * velocidade * Time.deltaTime);
    }

    void MovimentoSkate()
    {
        float z = Input.GetAxis("Vertical");
        float x = Input.GetAxis("Horizontal");
        transform.Rotate(0, x * velocidadeRotacaoSkate * Time.deltaTime, 0);

        Vector3 movimento = transform.forward * z;
        controller.Move(movimento * velocidadeSkate * Time.deltaTime);
    }

    public void SubirNoSkate(scriptDoSkate skate)
    {
        noSkate = true;
        skateEquipado = skate;

        Collider[] skateCols = skate.GetComponentsInChildren<Collider>();
        foreach(Collider col in skateCols) 
        { 
            col.enabled = false; 
        }

        Rigidbody rb = skate.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        controller.center = new Vector3(controller.center.x, controller.center.y + offsetAlturaSkate, controller.center.z);
        if (modeloDoPersonagem != null)
        {
            rotacaoOriginalModelo = modeloDoPersonagem.localRotation;
            modeloDoPersonagem.localRotation = rotacaoOriginalModelo * Quaternion.Euler(0, 90, 0); 
        }

        skate.transform.SetParent(localDoSkateNoPe != null ? localDoSkateNoPe : transform);
        
        if (localDoSkateNoPe != null)
        {
            skate.transform.localPosition = Vector3.zero;
            skate.transform.localRotation = Quaternion.identity;
        }
        else
        {
            skate.transform.localPosition = new Vector3(0, -(controller.height / 2f) + (offsetAlturaSkate / 2f), 0);
            skate.transform.localRotation = Quaternion.identity;
        }
    }

    void DescerDoSkate()
    {
        noSkate = false;

        if (skateEquipado != null)
        {
            controller.center = new Vector3(controller.center.x, controller.center.y - offsetAlturaSkate, controller.center.z);

            if (modeloDoPersonagem != null)
            {
                modeloDoPersonagem.localRotation = rotacaoOriginalModelo;
            }

            skateEquipado.transform.SetParent(null);

            Collider[] skateCols = skateEquipado.GetComponentsInChildren<Collider>();
            foreach(Collider col in skateCols) 
            { 
                col.enabled = true; 
            }

            Rigidbody rb = skateEquipado.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;

            skateEquipado.transform.position = transform.position + transform.right * 1.5f;
            
            Vector3 rot = skateEquipado.transform.eulerAngles;
            skateEquipado.transform.eulerAngles = new Vector3(0, rot.y, 0);

            skateEquipado = null;
        }
    }

    bool TentarColetarEstilingue()
    {
        ColetavelEstilingue[] estilingues = FindObjectsByType<ColetavelEstilingue>();
        ColetavelEstilingue estilingueMaisProximo = null;
        float menorDistancia = distanciaColetaEstilingue;

        foreach (ColetavelEstilingue estilingue in estilingues)
        {
            float distancia = Vector3.Distance(transform.position, estilingue.transform.position);
            if (distancia <= menorDistancia)
            {
                menorDistancia = distancia;
                estilingueMaisProximo = estilingue;
            }
        }

        if (estilingueMaisProximo != null)
        {
            possuiEstilingue = true;
            Debug.Log("Estilingue coletado! Pressione '1' para equipar.");
            Destroy(estilingueMaisProximo.gameObject);
            return true;
        }
        return false;
    }

    void AlternarEstilingue()
    {
        estilingueEquipado = !estilingueEquipado;

        if (estilingueNaMao != null)
        {
            estilingueNaMao.SetActive(estilingueEquipado);

            // Garante que o ControladorEstilingue esteja presente no estilingue da mão ao equipar
            if (estilingueEquipado && estilingueNaMao.GetComponent<ControladorEstilingue>() == null)
            {
                estilingueNaMao.AddComponent<ControladorEstilingue>();
                Debug.Log("ControladorEstilingue adicionado automaticamente ao estilingue na mão.");
            }
        }
        else
        {
            Debug.LogWarning("Objeto 'estilingueNaMao' não configurado no Inspector de scriptDeAndar.");
        }

        if (miraUI != null)
        {
            miraUI.SetActive(estilingueEquipado);
        }
        else
        {
            Debug.LogWarning("Objeto 'miraUI' não configurado no Inspector de scriptDeAndar.");
        }
        
        Debug.Log("Estilingue " + (estilingueEquipado ? "equipado" : "desequipado") + ".");
    }
}
