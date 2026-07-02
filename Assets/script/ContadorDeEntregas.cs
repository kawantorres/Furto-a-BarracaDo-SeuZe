using UnityEngine;

public class ContadorDeEntregas : MonoBehaviour
{
    [Header("Progresso das Tarefas")]
    public int totalDeTarefas = 1;
    public int tarefasCompletas = 0;

    [Header("Recompensa Final")]
    public GameObject bolaQuadrada;
    public Transform localDaRecompensa;
    public bool todasTarefasCompletas = false;

    public void RegistrarEntrega()
    {
        if (todasTarefasCompletas)
        {
            return;
        }

        tarefasCompletas++;
        Debug.Log("Tarefa concluída! Progresso: " + tarefasCompletas + "/" + totalDeTarefas);

        if (tarefasCompletas >= totalDeTarefas)
        {
            CompletarTodasAsTarefas();
        }
    }

    private void CompletarTodasAsTarefas()
    {
        todasTarefasCompletas = true;
        Debug.Log("Todas as tarefas foram concluídas! O jogador ganhou a bola quadrada!");

        if (bolaQuadrada != null)
        {
            bolaQuadrada.SetActive(true);

            if (localDaRecompensa != null)
            {
                bolaQuadrada.transform.SetParent(localDaRecompensa);
                bolaQuadrada.transform.localPosition = Vector3.zero;
                bolaQuadrada.transform.localRotation = Quaternion.identity;
            }
        }
    }
}
