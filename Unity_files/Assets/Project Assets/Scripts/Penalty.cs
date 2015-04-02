using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Représente une pénalité infligée à un joueur : le nombre de tours à partir duquel la pénalité a lieu, et l'effet à déclencher.
/// </summary>
public class Penalty {
    public delegate void Effect(Penalty p);

	public int remainingTurns { get; private set; } // Nombre de tours avant effet. Vaut -1 si la réaction n'implique pas un certain nombre de tours avant effet
	public Player target { get; private set; } // Joueur à attaquer
	private Effect effect; // "Pointeur sur fonction" appelé au début du tour du joueur

    
    private GameObject penaltyToken;

    /// <summary>
    /// Constructeur de la pénalité
    /// </summary>
    /// <param name="turns">Nombre de tours avant effet</param>
    /// <param name="nTarget">Le joueur ciblé par la pénalité</param>
    /// <param name="nEffect">La fonction appelée au début du tour du joueur ciblé. Prend en argument la pénalité.</param>
	public Penalty(int turns, Player nTarget, Penalty.Effect nEffect) {
		remainingTurns = turns;
		target = nTarget;
		effect = nEffect;

        if (turns > 0) {
            penaltyToken = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/PenaltyToken"));
            penaltyToken.transform.Find("RemainingTurns").GetComponent<Text>().text = remainingTurns.ToString();
            penaltyToken.transform.SetParent(target.playerScreen.transform.Find("BoardGame/PenaltyTokensContainer"+ target.room));
        }
	}

    /// <summary>
    /// Fait passer un tour à la pénalité. Attention, un compteur négatif (ou nul)
    /// n'est pas modifié.
    /// </summary>
    public void newTurn ()
    {
        if (remainingTurns > 0) {
            remainingTurns--;
            penaltyToken.transform.Find("RemainingTurns").GetComponent<Text>().text = remainingTurns.ToString();
        }
    }

    public void setOff() {
        Remove();
        effect(this);
    }

    public void Remove() {
        GameObject.Destroy(penaltyToken);
    }

    /// <summary>
    /// Teste si la pénalité est active (compteur négatif ou nul).
    /// </summary>
    /// <returns>Vrai ssi la pénalité est active.</returns>
    public bool isActive ()
    {
        return remainingTurns <= 0;
    }
}
