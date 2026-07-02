using UnityEngine;

public class Perseguidor : MonoBehaviour
{
    [Header("Perseguição")]
    public Transform alvo;
    public float velocidadePerseguicao = 6f;
    public float distanciaDesistencia = 30f;
    public float velocidadeRotacao = 10f;
    public bool perseguindo = false;

    [Header("Toque no Jogador")]
    public float distanciaDeToque = 1f;

    private VidaJogador vidaDoAlvo;

    void Start()
    {
        if (alvo == null)
        {
            GameObject jogador = GameObject.FindGameObjectWithTag("Player");
            if (jogador != null)
            {
                alvo = jogador.transform;
            }
        }

        if (alvo != null)
        {
            vidaDoAlvo = alvo.GetComponentInParent<VidaJogador>();
        }
    }

    public void ComecarPerseguicao()
    {
        perseguindo = true;
    }

    void Update()
    {
        if (!perseguindo || alvo == null)
        {
            return;
        }

        float distancia = Vector3.Distance(transform.position, alvo.position);
        if (distancia >= distanciaDesistencia)
        {
            perseguindo = false;
            return;
        }

        Vector3 direcao = alvo.position - transform.position;
        direcao.y = 0f;

        if (direcao.sqrMagnitude > 0.001f)
        {
            Quaternion rotacaoAlvo = Quaternion.LookRotation(direcao);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacaoAlvo, velocidadeRotacao * Time.deltaTime);
        }

        transform.position += direcao.normalized * velocidadePerseguicao * Time.deltaTime;

        if (distancia <= distanciaDeToque && vidaDoAlvo != null)
        {
            vidaDoAlvo.PerderVida();
        }
    }
}
