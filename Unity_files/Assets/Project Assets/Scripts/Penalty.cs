using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Représente une pénalité infligée à un joueur : le nombre de tours à partir duquel la pénalité a lieu, et l'effet à déclencher.
/// </summary>
public class Penalty {
    public delegate bool Effect(Penalty p);
    public delegate void Delete();
    
    private Effect effect; // Effet à chaque tour
    private Delete delete; // Effet au moment où la pénalité est supprimée

    private bool acted; // Rustine pour la réactionb uranium qui n'agit plus instantanément
    private bool removed = false;

    /// <summary>
    /// Constructeur de la pénalité
    /// </summary>
    /// <param name="onEffect">La fonction appelée au début du tour du joueur ciblé. Doit retourner true si la pénalité doit être supprimée, false sinon</param>
    /// <param name="onDelete">La fonction appelée lorsque la pénalité est supprimée.</param>
	public Penalty(Penalty.Effect onEffect, Penalty.Delete onDelete) {
        effect = onEffect;
        delete = onDelete;
        acted = false;
	}
    
    /// <summary>
    /// Appelle la fonction "effet à chaque tour" de la pénalité.
    /// Si la pénailté doit être supprimée à ce tour là, supprime la pénalité et retourne true
    /// Retourne false sinon
    /// </summary>
    public bool setOff() {
        if (acted)
            return false;
        if (effect(this))
            acted = true;
        return removed;
    }

    /// <summary>
    /// Fonction à appeler lorsque la pénalité est supprimée
    /// </summary>
    public void Remove() {
        delete();
        removed = true;
    }

    /// <summary>
    /// Fonction à appeler à la fin du tour d'un joueur
    /// </summary>
    public void reset() {
        acted = false;
    }
}
