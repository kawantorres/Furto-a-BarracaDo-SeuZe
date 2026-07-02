using System.Collections;
using UnityEngine;

public class EntregaSeuZe : MonoBehaviour
{
    [Header("Configuração da Entrega")]
    public GameObject caixaEncomenda;
    public float duracaoEntrega = 1f;
    public float alturaEntregaNaMao = 1.2f;
    public Vector3 offsetNaMao = new Vector3(0f, 0.05f, 0.1f);
    public bool jaEntregou = false;

    private Coroutine entregaEmAndamento;

    public bool PodeEntregar()
    {
        return !jaEntregou && caixaEncomenda != null && entregaEmAndamento == null;
    }

    public void Entregar(Transform alvo)
    {
        if (!PodeEntregar())
        {
            return;
        }

        entregaEmAndamento = StartCoroutine(RotinaDeEntrega(alvo));
    }

    private IEnumerator RotinaDeEntrega(Transform alvo)
    {
        Vector3 posicaoInicial = caixaEncomenda.transform.position;
        Quaternion rotacaoInicial = caixaEncomenda.transform.rotation;

        Collider colisorDaCaixa = caixaEncomenda.GetComponent<Collider>();
        if (colisorDaCaixa != null)
        {
            colisorDaCaixa.enabled = false;
        }

        Transform maoDoAlvo = EncontrarMao(alvo);
        Transform pontoDeApoio = maoDoAlvo != null ? maoDoAlvo : alvo;
        Vector3 offsetLocal = maoDoAlvo != null ? offsetNaMao : Vector3.forward * 0.5f + Vector3.up * alturaEntregaNaMao;

        float tempoDecorrido = 0f;
        while (tempoDecorrido < duracaoEntrega)
        {
            tempoDecorrido += Time.deltaTime;
            float progresso = Mathf.Clamp01(tempoDecorrido / duracaoEntrega);

            Vector3 posicaoAlvo = pontoDeApoio.TransformPoint(offsetLocal);
            caixaEncomenda.transform.position = Vector3.Lerp(posicaoInicial, posicaoAlvo, progresso);
            caixaEncomenda.transform.rotation = Quaternion.Slerp(rotacaoInicial, pontoDeApoio.rotation, progresso);

            yield return null;
        }

        caixaEncomenda.transform.SetParent(pontoDeApoio);
        caixaEncomenda.transform.localPosition = offsetLocal;
        caixaEncomenda.transform.localRotation = Quaternion.identity;

        jaEntregou = true;
        entregaEmAndamento = null;

        Debug.Log("Seu Zé entregou a encomenda para " + alvo.name + "!");
    }

    private Transform EncontrarMao(Transform alvo)
    {
        Transform maoDireita = null;
        Transform maoEsquerda = null;

        var pilha = new System.Collections.Generic.Stack<Transform>();
        pilha.Push(alvo);
        while (pilha.Count > 0)
        {
            Transform atual = pilha.Pop();
            string nome = atual.name.ToLowerInvariant();

            if (maoDireita == null && nome.Contains("righthand") && !nome.Contains("dummy"))
            {
                maoDireita = atual;
            }
            if (maoEsquerda == null && nome.Contains("lefthand") && !nome.Contains("dummy"))
            {
                maoEsquerda = atual;
            }

            foreach (Transform filho in atual)
            {
                pilha.Push(filho);
            }
        }

        return maoDireita != null ? maoDireita : maoEsquerda;
    }
}
