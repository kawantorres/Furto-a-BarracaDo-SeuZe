using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class scriptDeAndar : MonoBehaviour
{
    public float velocidade = 5f;
    public float velocidadeCorrida = 9f;
    public float gravidade = -9.81f;
    public float alturaPulo = 1.5f;
    public bool correndo = false;

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

    [Header("Configurações de Interação")]
    public float distanciaInteracao = 8f; // Distância para subir no skate ou carrinho
    public float distanciaEntrega = 3f; // Distância para receber encomenda do Seu Zé
    public float distanciaPorta = 3f; // Distância para abrir/fechar portas

    [Header("Configurações do Carrinho de Rolimã")]
    public float velocidadeCarrinho = 20f;
    public float velocidadeRotacaoCarrinho = 80f;
    public float offsetAlturaCarrinho = 0.1f;
    public bool noCarrinho = false;
    private scriptDoCarrinhoDeRolima carrinhoEquipado;
    private float velocidadeAtualCarrinhoMovel = 0f;

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
        else if (noCarrinho)
        {
            MovimentoCarrinho();

            if (Input.GetKeyDown(KeyCode.E))
            {
                DescerDoCarrinho();
            }
        }
        else
        {
            MovimentoNormal();

            if (Input.GetKeyDown(KeyCode.E))
            {
                // Tenta abrir/fechar uma porta primeiro.
                bool mexeuNaPorta = TentarAbrirFecharPorta();
                if (!mexeuNaPorta)
                {
                    // Tenta receber a encomenda do Seu Zé.
                    bool recebeuEncomenda = TentarReceberEncomenda();
                    if (!recebeuEncomenda)
                    {
                        // Tenta coletar o estilingue. Se coletar, não sobe no skate/carrinho neste frame.
                        bool coletou = TentarColetarEstilingue();
                        if (!coletou)
                        {
                            // Tenta subir no skate. Se não conseguir, tenta subir no carrinho de rolimã.
                            bool subiuNoSkate = TentarSubirNoSkate();
                            if (!subiuNoSkate)
                            {
                                TentarSubirNoCarrinho();
                            }
                        }
                    }
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

        // Pulo só é permitido se não estiver no skate nem no carrinho
        if (Input.GetButtonDown("Jump") && noChao && !noSkate && !noCarrinho)
        {
            velocidadeAtual.y = Mathf.Sqrt(alturaPulo * -2f * gravidade);
        }

        velocidadeAtual.y += gravidade * Time.deltaTime;
        controller.Move(velocidadeAtual * Time.deltaTime);
    }

    bool TentarAbrirFecharPorta()
    {
        PortaInterativa[] portas = FindObjectsByType<PortaInterativa>(FindObjectsSortMode.None);
        PortaInterativa portaMaisProxima = null;
        float menorDistancia = distanciaPorta;

        foreach (PortaInterativa porta in portas)
        {
            if (!porta.PodeInteragir())
            {
                continue;
            }

            float distancia = DistanciaAtePorta(porta);
            if (distancia <= menorDistancia)
            {
                menorDistancia = distancia;
                portaMaisProxima = porta;
            }
        }

        if (portaMaisProxima != null)
        {
            portaMaisProxima.Alternar();
            return true;
        }
        return false;
    }

    // Usa o ponto mais próximo do colisor da porta (não o pivô da dobradiça),
    // já que a dobradiça fica na borda da porta e não no centro.
    float DistanciaAtePorta(PortaInterativa porta)
    {
        Collider colisorDaPorta = porta.GetComponentInChildren<Collider>();
        if (colisorDaPorta == null)
        {
            return Vector3.Distance(transform.position, porta.transform.position);
        }

        Vector3 pontoMaisProximo = colisorDaPorta.ClosestPoint(transform.position);
        return Vector3.Distance(transform.position, pontoMaisProximo);
    }

    bool TentarReceberEncomenda()
    {
        EntregaSeuZe[] entregadores = FindObjectsByType<EntregaSeuZe>(FindObjectsSortMode.None);
        EntregaSeuZe entregadorMaisProximo = null;
        float menorDistancia = distanciaEntrega;

        foreach (EntregaSeuZe entregador in entregadores)
        {
            if (!entregador.PodeEntregar())
            {
                continue;
            }

            float distancia = Vector3.Distance(transform.position, entregador.transform.position);
            if (distancia <= menorDistancia)
            {
                menorDistancia = distancia;
                entregadorMaisProximo = entregador;
            }
        }

        if (entregadorMaisProximo != null)
        {
            entregadorMaisProximo.Entregar(transform);
            return true;
        }
        return false;
    }

    bool TentarSubirNoSkate()
    {
        scriptDoSkate[] skates = FindObjectsByType<scriptDoSkate>();
        scriptDoSkate skateMaisProximo = null;
        float menorDistancia = distanciaInteracao;

        Debug.Log("Encontrados " + skates.Length + " skates na cena.");
        foreach (scriptDoSkate skate in skates)
        {
            float distancia = Vector3.Distance(transform.position, skate.transform.position);
            Debug.Log("Distância até o skate '" + skate.name + "': " + distancia + "m. (Limite: " + menorDistancia + "m)");
            if (distancia <= menorDistancia)
            {
                menorDistancia = distancia;
                skateMaisProximo = skate;
            }
        }

        if (skateMaisProximo != null)
        {
            SubirNoSkate(skateMaisProximo);
            return true;
        }
        return false;
    }

    bool TentarSubirNoCarrinho()
    {
        scriptDoCarrinhoDeRolima[] carrinhos = FindObjectsByType<scriptDoCarrinhoDeRolima>();
        scriptDoCarrinhoDeRolima carrinhoMaisProximo = null;
        float menorDistancia = distanciaInteracao;

        Debug.Log("Encontrados " + carrinhos.Length + " carrinhos de rolimã na cena.");
        foreach (scriptDoCarrinhoDeRolima carrinho in carrinhos)
        {
            float distancia = Vector3.Distance(transform.position, carrinho.transform.position);
            Debug.Log("Distância até o carrinho '" + carrinho.name + "': " + distancia + "m. (Limite: " + menorDistancia + "m)");
            if (distancia <= menorDistancia)
            {
                menorDistancia = distancia;
                carrinhoMaisProximo = carrinho;
            }
        }

        if (carrinhoMaisProximo != null)
        {
            SubirNoCarrinho(carrinhoMaisProximo);
            return true;
        }
        return false;
    }

    void SubirNoCarrinho(scriptDoCarrinhoDeRolima carrinho)
    {
        noCarrinho = true;
        carrinhoEquipado = carrinho;
        velocidadeAtualCarrinhoMovel = 0f;

        Collider[] carrinhosCols = carrinho.GetComponentsInChildren<Collider>();
        foreach (Collider col in carrinhosCols)
        {
            col.enabled = false;
        }

        Rigidbody rb = carrinho.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        controller.center = new Vector3(controller.center.x, controller.center.y + offsetAlturaCarrinho, controller.center.z);
        if (modeloDoPersonagem != null)
        {
            rotacaoOriginalModelo = modeloDoPersonagem.localRotation;
            // No rolimã o personagem fica de frente, então não precisa rotacionar 90 graus como no skate
        }

        carrinho.transform.SetParent(transform);
        carrinho.transform.localPosition = new Vector3(0, -(controller.height / 2f) + (offsetAlturaCarrinho / 2f), 0);
        carrinho.transform.localRotation = Quaternion.identity;
    }

    void DescerDoCarrinho()
    {
        noCarrinho = false;

        if (carrinhoEquipado != null)
        {
            controller.center = new Vector3(controller.center.x, controller.center.y - offsetAlturaCarrinho, controller.center.z);

            if (modeloDoPersonagem != null)
            {
                modeloDoPersonagem.localRotation = rotacaoOriginalModelo;
            }

            carrinhoEquipado.transform.SetParent(null);

            Collider[] carrinhoCols = carrinhoEquipado.GetComponentsInChildren<Collider>();
            foreach (Collider col in carrinhoCols)
            {
                col.enabled = true;
            }

            Rigidbody rb = carrinhoEquipado.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;

            carrinhoEquipado.transform.position = transform.position + transform.right * 1.5f;

            Vector3 rot = carrinhoEquipado.transform.eulerAngles;
            carrinhoEquipado.transform.eulerAngles = new Vector3(0, rot.y, 0);

            carrinhoEquipado = null;
        }
    }

    void MovimentoCarrinho()
    {
        float z = Input.GetAxis("Vertical"); // W/S
        float x = Input.GetAxis("Horizontal"); // A/D

        // Rotaciona o carrinho (e o jogador)
        transform.Rotate(0, x * velocidadeRotacaoCarrinho * Time.deltaTime, 0);

        // Detecta inclinação para baixo para acelerar com a gravidade
        float inclinacao = Vector3.Dot(transform.forward, Vector3.up); // Negativo se descida, positivo se subida
        float aceleracaoGravidade = 0f;

        if (inclinacao < -0.05f) // Descendo
        {
            aceleracaoGravidade = -inclinacao * 15f; // Quanto mais inclinado, mais acelera
        }

        // Aceleração manual (W) ou freio (S)
        if (z > 0)
        {
            velocidadeAtualCarrinhoMovel = Mathf.MoveTowards(velocidadeAtualCarrinhoMovel, velocidadeCarrinho, 10f * Time.deltaTime);
        }
        else if (z < 0)
        {
            velocidadeAtualCarrinhoMovel = Mathf.MoveTowards(velocidadeAtualCarrinhoMovel, 0f, 20f * Time.deltaTime); // Freio forte
        }
        else
        {
            velocidadeAtualCarrinhoMovel = Mathf.MoveTowards(velocidadeAtualCarrinhoMovel, 0f, 2f * Time.deltaTime); // Desaceleração gradual
        }

        // Adiciona gravidade na descida
        velocidadeAtualCarrinhoMovel += aceleracaoGravidade * Time.deltaTime;

        // Limita a velocidade máxima
        velocidadeAtualCarrinhoMovel = Mathf.Clamp(velocidadeAtualCarrinhoMovel, 0f, velocidadeCarrinho * 2f);

        Vector3 movimento = transform.forward * velocidadeAtualCarrinhoMovel;
        controller.Move(movimento * Time.deltaTime);
    }

    void MovimentoNormal()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        correndo = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float velocidadeAplicada = correndo ? velocidadeCorrida : velocidade;

        Vector3 movimento = transform.right * x + transform.forward * z;
        controller.Move(movimento * velocidadeAplicada * Time.deltaTime);
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
