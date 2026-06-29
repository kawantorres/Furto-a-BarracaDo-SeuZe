using UnityEngine;

public class CameraTerceiraPessoa : MonoBehaviour
{
    [Header("Alvo a ser seguido (Personagem)")]
    public Transform alvo;

    [Header("Configurações da Câmera")]
    public float distancia = 5f;
    public float alturaDaCamera = 1.5f;
    public float sensibilidadeMouseX = 3f;
    public float sensibilidadeMouseY = 3f;
    public float suavidade = 10f; 

    [Header("Limites de Visão Vertical")]
    public float anguloMinimo = -20f;
    public float anguloMaximo = 60f;

    [Header("Colisão da Câmera (Evitar entrar em paredes)")]
    public LayerMask layerColisao;
    public float distanciaMinimaColisao = 0.5f;

    private float rotacaoX;
    private float rotacaoY;
    private Vector3 posicaoAtual;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (alvo == null)
        {
            Debug.LogWarning("Falta colocar o alvo (personagem) na câmera!");
            return;
        }

        rotacaoX += Input.GetAxis("Mouse X") * sensibilidadeMouseX;
        rotacaoY -= Input.GetAxis("Mouse Y") * sensibilidadeMouseY;

        rotacaoY = Mathf.Clamp(rotacaoY, anguloMinimo, anguloMaximo);

        Quaternion rotacaoCamera = Quaternion.Euler(rotacaoY, rotacaoX, 0);

        Vector3 posicaoFoco = alvo.position + new Vector3(0, alturaDaCamera, 0);

        Vector3 direcaoParaTras = new Vector3(0, 0, -distancia);
        Vector3 posicaoDesejada = posicaoFoco + rotacaoCamera * direcaoParaTras;

        RaycastHit hit;
        if (Physics.Linecast(posicaoFoco, posicaoDesejada, out hit, layerColisao))
        {
            float distanciaParede = Vector3.Distance(posicaoFoco, hit.point);
            float distanciaCorrigida = Mathf.Clamp(distanciaParede - distanciaMinimaColisao, 0.1f, distancia);
            
            posicaoDesejada = posicaoFoco + rotacaoCamera * new Vector3(0, 0, -distanciaCorrigida);
        }

        posicaoAtual = Vector3.Lerp(posicaoAtual, posicaoDesejada, suavidade * Time.deltaTime);

        transform.position = posicaoAtual;
        transform.LookAt(posicaoFoco);
    }
}
