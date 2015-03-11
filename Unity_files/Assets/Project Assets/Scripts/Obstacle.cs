using UnityEngine;
using System.Collections;

/// <summary>
/// Symbolise un type d'obstacle. Contient les informations liées :
/// nom, ressource graphique.
/// </summary>
public class Obstacle
{
    public string name { get; private set; } // Le nom de l'obstacle (e.g. "Feu")
    public Sprite obstacleResource { get; private set; } // Ressource graphique
    public ReactionType weakness { get; private set; } // Le type de réaction permettant de le détruire

    /// <summary>
    /// Constructeur de l'obstacle.
    /// </summary>
    /// <param name="nName">Le nom de l'obstacle.</param>
    /// <param name="fileName">Le nom du fichier image (sans extension).</param>
    public Obstacle (string nName, string fileName)
    {
        name = nName;
        obstacleResource = Resources.Load<Sprite> ("Images/Obstacles/" + fileName);
    }
}
