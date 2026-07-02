using System.Collections;
using UnityEngine;

public class VidaJogador : MonoBehaviour
{
    [Header("Configuração de Vida")]
    public int vidaMaxima = 3;
    public int vidaAtual;
    public float duracaoInvulneravel = 1f;

    private bool invulneravel = false;

    void Start()
    {
        vidaAtual = vidaMaxima;
    }

    public void PerderVida(int quantidade = 1)
    {
        if (invulneravel || vidaAtual <= 0)
        {
            return;
        }

        vidaAtual = Mathf.Max(vidaAtual - quantidade, 0);
        Debug.Log("Jogador perdeu vida! Vida atual: " + vidaAtual + "/" + vidaMaxima);

        if (vidaAtual <= 0)
        {
            Debug.Log("O jogador ficou sem vida!");
        }
        else
        {
            StartCoroutine(InvulnerabilidadeTemporaria());
        }
    }

    private IEnumerator InvulnerabilidadeTemporaria()
    {
        invulneravel = true;
        yield return new WaitForSeconds(duracaoInvulneravel);
        invulneravel = false;
    }
}
