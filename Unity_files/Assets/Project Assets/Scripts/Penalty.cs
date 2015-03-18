using UnityEngine;
using System.Collections;

/// <summary>
/// Représente une pénalité infligée à un joueur : le nombre de tours à partir duquel la pénalité a lieu, et l'effet à déclencher.
/// </summary>
public class Penalty {
    public delegate void Effect(Penalty p);

	public int remainingTurns { get; private set; } // Nombre de tours avant effet. Vaut -1 si la réaction n'implique pas un certain nombre de tours avant effet
	public Player target { get; private set; } // Joueur à attaquer
	public Effect effect; // "Pointeur sur fonction" appelé au début du tour du joueur

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
	}

    /// <summary>
    /// Fait passer un tour à la pénalité. Attention, un compteur négatif (ou nul)
    /// n'est pas modifié.
    /// </summary>
    public void newTurn ()
    {
        if (remainingTurns > 0)
         remainingTurns--;
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
