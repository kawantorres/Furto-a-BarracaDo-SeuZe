using UnityEngine;

public class GatilhoPerseguicao : MonoBehaviour
{
    public Perseguidor[] perseguidores;
    public string mensagemAtivacao = "O jogador entrou na área, os perseguidores foram atrás dele!";
    private bool jaAtivado = false;

    void OnTriggerEnter(Collider other)
    {
        if (jaAtivado || !other.CompareTag("Player"))
        {
            return;
        }

        jaAtivado = true;

        foreach (Perseguidor perseguidor in perseguidores)
        {
            if (perseguidor != null)
            {
                perseguidor.ComecarPerseguicao();
            }
        }

        Debug.Log(mensagemAtivacao);
    }
}
