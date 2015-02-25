using UnityEngine;
using System.Collections;

/// <summary>
/// Représente une pénalité infligée à un joueur : le nombre de tours à partir duquel la pénalité a lieu, et l'effet à déclancher
/// </summary>
public class Penalty {
    public delegate void Effect(Penalty p);

	int remainingTurns { get; set; } // Nombre de tours avant effet. Vaut -1 si la réaction n'implique pas un certain nombre de tours avant effet
	Player target { get; set; } // Joueur à attaquer
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
}
