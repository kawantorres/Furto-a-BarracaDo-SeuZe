using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class scriptDeAndar : MonoBehaviour
{
    [Header("Configurações Gerais")]
    public float velocidade = 5f;
    public float velocidadeCorrida = 9f;
    public float gravidade = -9.81f;
    public float alturaPulo = 1.5f;
    public bool correndo = false;

    [Header("Configurações do Skate")]
    public float velocidadeSkate = 15f;
    public float velocidadeRotacaoSkate = 100f;
    public float alturaPuloSkate = 1.2f; // Adicionado
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
    public float distanciaInteracao = 8f;
    public float distanciaEntrega = 3f;
    public float distanciaPorta = 3f;

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
            if (Input.GetKeyDown(KeyCode.E)) DescerDoSkate();
        }
        else if (noCarrinho)
        {
            MovimentoCarrinho();
            if (Input.GetKeyDown(KeyCode.E)) DescerDoCarrinho();
        }
        else
        {
            MovimentoNormal();
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!TentarAbrirFecharPorta() && !TentarReceberEncomenda() && !TentarColetarEstilingue())
                {
                    if (!TentarSubirNoSkate()) TentarSubirNoCarrinho();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) && possuiEstilingue)
        {
            AlternarEstilingue();
        }

        // Lógica de Pulo Integrada
        if (Input.GetButtonDown("Jump") && noChao && !noCarrinho)
        {
            float altura = noSkate ? alturaPuloSkate : alturaPulo;
            velocidadeAtual.y = Mathf.Sqrt(altura * -2f * gravidade);
        }

        velocidadeAtual.y += gravidade * Time.deltaTime;
        controller.Move(velocidadeAtual * Time.deltaTime);
    }

    // --- MÉTODOS DE MOVIMENTO E INTERAÇÃO ---

    void MovimentoNormal()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        correndo = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        Vector3 movimento = transform.right * x + transform.forward * z;
        controller.Move(movimento * (correndo ? velocidadeCorrida : velocidade) * Time.deltaTime);
    }

    void MovimentoSkate()
    {
        float z = Input.GetAxis("Vertical");
        float x = Input.GetAxis("Horizontal");
        transform.Rotate(0, x * velocidadeRotacaoSkate * Time.deltaTime, 0);
        controller.Move(transform.forward * z * velocidadeSkate * Time.deltaTime);
    }

    void MovimentoCarrinho()
    {
        float z = Input.GetAxis("Vertical");
        float x = Input.GetAxis("Horizontal");
        transform.Rotate(0, x * velocidadeRotacaoCarrinho * Time.deltaTime, 0);
        
        float inclinacao = Vector3.Dot(transform.forward, Vector3.up);
        float aceleracaoGravidade = (inclinacao < -0.05f) ? -inclinacao * 15f : 0f;

        if (z > 0) velocidadeAtualCarrinhoMovel = Mathf.MoveTowards(velocidadeAtualCarrinhoMovel, velocidadeCarrinho, 10f * Time.deltaTime);
        else if (z < 0) velocidadeAtualCarrinhoMovel = Mathf.MoveTowards(velocidadeAtualCarrinhoMovel, 0f, 20f * Time.deltaTime);
        else velocidadeAtualCarrinhoMovel = Mathf.MoveTowards(velocidadeAtualCarrinhoMovel, 0f, 2f * Time.deltaTime);

        velocidadeAtualCarrinhoMovel += aceleracaoGravidade * Time.deltaTime;
        velocidadeAtualCarrinhoMovel = Mathf.Clamp(velocidadeAtualCarrinhoMovel, 0f, velocidadeCarrinho * 2f);
        controller.Move(transform.forward * velocidadeAtualCarrinhoMovel * Time.deltaTime);
    }

    // --- MÉTODOS AUXILIARES DE SUBIR/DESCER ---

    public void SubirNoSkate(scriptDoSkate skate)
    {
        noSkate = true;
        skateEquipado = skate;
        foreach (Collider col in skate.GetComponentsInChildren<Collider>()) col.enabled = false;
        if (skate.GetComponent<Rigidbody>()) skate.GetComponent<Rigidbody>().isKinematic = true;

        controller.center += new Vector3(0, offsetAlturaSkate, 0);
        if (modeloDoPersonagem)
        {
            rotacaoOriginalModelo = modeloDoPersonagem.localRotation;
            modeloDoPersonagem.localRotation *= Quaternion.Euler(0, 90, 0);
        }
        skate.transform.SetParent(localDoSkateNoPe ? localDoSkateNoPe : transform);
        skate.transform.localPosition = Vector3.zero;
        skate.transform.localRotation = Quaternion.identity;
    }

    void DescerDoSkate()
    {
        noSkate = false;
        controller.center -= new Vector3(0, offsetAlturaSkate, 0);
        if (modeloDoPersonagem) modeloDoPersonagem.localRotation = rotacaoOriginalModelo;
        
        skateEquipado.transform.SetParent(null);
        foreach (Collider col in skateEquipado.GetComponentsInChildren<Collider>()) col.enabled = true;
        if (skateEquipado.GetComponent<Rigidbody>()) skateEquipado.GetComponent<Rigidbody>().isKinematic = false;
        
        skateEquipado.transform.position = transform.position + transform.right * 1.5f;
        skateEquipado = null;
    }

    void SubirNoCarrinho(scriptDoCarrinhoDeRolima carrinho)
    {
        noCarrinho = true;
        carrinhoEquipado = carrinho;
        foreach (Collider col in carrinho.GetComponentsInChildren<Collider>()) col.enabled = false;
        if (carrinho.GetComponent<Rigidbody>()) carrinho.GetComponent<Rigidbody>().isKinematic = true;

        controller.center += new Vector3(0, offsetAlturaCarrinho, 0);
        carrinho.transform.SetParent(transform);
        carrinho.transform.localPosition = new Vector3(0, -(controller.height / 2f) + (offsetAlturaCarrinho / 2f), 0);
        carrinho.transform.localRotation = Quaternion.identity;
    }

    void DescerDoCarrinho()
    {
        noCarrinho = false;
        controller.center -= new Vector3(0, offsetAlturaCarrinho, 0);
        carrinhoEquipado.transform.SetParent(null);
        foreach (Collider col in carrinhoEquipado.GetComponentsInChildren<Collider>()) col.enabled = true;
        if (carrinhoEquipado.GetComponent<Rigidbody>()) carrinhoEquipado.GetComponent<Rigidbody>().isKinematic = false;
        
        carrinhoEquipado.transform.position = transform.position + transform.right * 1.5f;
        carrinhoEquipado = null;
    }

    // --- MÉTODOS DE LÓGICA DE INTERAÇÃO (Portas, Itens, NPC) ---
    // (Mantidos conforme seu original, garantindo funcionamento lógico)

    bool TentarAbrirFecharPorta()
    {
        PortaInterativa[] portas = FindObjectsByType<PortaInterativa>(FindObjectsSortMode.None);
        PortaInterativa alvo = null;
        float dist = distanciaPorta;
        foreach (var p in portas) if (p.PodeInteragir() && DistanciaAtePorta(p) <= dist) { dist = DistanciaAtePorta(p); alvo = p; }
        if (alvo) { alvo.Alternar(); return true; }
        return false;
    }

    float DistanciaAtePorta(PortaInterativa p)
    {
        Collider c = p.GetComponentInChildren<Collider>();
        return c ? Vector3.Distance(transform.position, c.ClosestPoint(transform.position)) : Vector3.Distance(transform.position, p.transform.position);
    }

    bool TentarReceberEncomenda()
    {
        foreach (var e in FindObjectsByType<EntregaSeuZe>(FindObjectsSortMode.None))
        {
            if (e.PodeEntregar() && Vector3.Distance(transform.position, e.transform.position) <= distanciaEntrega) { e.Entregar(transform); return true; }
        }
        return false;
    }

    bool TentarSubirNoSkate()
    {
        foreach (var s in FindObjectsByType<scriptDoSkate>(FindObjectsSortMode.None))
        {
            if (Vector3.Distance(transform.position, s.transform.position) <= distanciaInteracao) { SubirNoSkate(s); return true; }
        }
        return false;
    }

    bool TentarSubirNoCarrinho()
    {
        foreach (var c in FindObjectsByType<scriptDoCarrinhoDeRolima>(FindObjectsSortMode.None))
        {
            if (Vector3.Distance(transform.position, c.transform.position) <= distanciaInteracao) { SubirNoCarrinho(c); return true; }
        }
        return false;
    }

    bool TentarColetarEstilingue()
    {
        foreach (var e in FindObjectsByType<ColetavelEstilingue>(FindObjectsSortMode.None))
        {
            if (Vector3.Distance(transform.position, e.transform.position) <= distanciaColetaEstilingue) { possuiEstilingue = true; Destroy(e.gameObject); return true; }
        }
        return false;
    }

    void AlternarEstilingue()
    {
        estilingueEquipado = !estilingueEquipado;
        if (estilingueNaMao) {
            estilingueNaMao.SetActive(estilingueEquipado);
            if (estilingueEquipado && !estilingueNaMao.GetComponent<ControladorEstilingue>()) estilingueNaMao.AddComponent<ControladorEstilingue>();
        }
        if (miraUI) miraUI.SetActive(estilingueEquipado);
    }
}