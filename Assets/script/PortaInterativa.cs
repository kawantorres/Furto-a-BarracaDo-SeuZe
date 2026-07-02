using System.Collections;
using UnityEngine;

public class PortaInterativa : MonoBehaviour
{
    [Header("Configuração da Porta")]
    public float anguloAberta = 100f;
    public float duracaoAnimacao = 0.6f;
    public bool estaAberta = false;

    private Coroutine animacaoEmAndamento;
    private Quaternion rotacaoFechada;

    void Start()
    {
        rotacaoFechada = transform.localRotation;
    }

    public bool PodeInteragir()
    {
        return animacaoEmAndamento == null;
    }

    public void Alternar()
    {
        if (!PodeInteragir())
        {
            return;
        }

        animacaoEmAndamento = StartCoroutine(RotinaDeAbrirFechar());
    }

    private IEnumerator RotinaDeAbrirFechar()
    {
        Quaternion rotacaoInicial = transform.localRotation;
        Quaternion rotacaoFinal = estaAberta ? rotacaoFechada : rotacaoFechada * Quaternion.Euler(0f, anguloAberta, 0f);

        float tempoDecorrido = 0f;
        while (tempoDecorrido < duracaoAnimacao)
        {
            tempoDecorrido += Time.deltaTime;
            float progresso = Mathf.Clamp01(tempoDecorrido / duracaoAnimacao);
            transform.localRotation = Quaternion.Slerp(rotacaoInicial, rotacaoFinal, progresso);
            yield return null;
        }

        transform.localRotation = rotacaoFinal;
        estaAberta = !estaAberta;
        animacaoEmAndamento = null;

        Debug.Log("Porta " + (estaAberta ? "aberta" : "fechada") + ".");
    }
}
