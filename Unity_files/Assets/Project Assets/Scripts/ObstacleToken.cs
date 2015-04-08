using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Symbolise un jeton d'obstacle. Contient un GameObject, et un lien vers
/// l'obstacle qu'il représente.
/// </summary>
public class ObstacleToken
{
    public Obstacle obstacle { get; private set; } // L'obstacle représenté
    public GameObject obstacleImg { get;  set; } // Le GameObject correspondant
    public int room; // Le numéro de la salle

    /// <summary>
    /// Constructeur du jeton d'obstacle.
    /// </summary>
    /// <param name="nObstacle">L'obstacle référencé.</param>
    /// <param name="gameObject">Le gameObject contenant le jeton.</param>
    public ObstacleToken (Obstacle nObstacle, GameObject gameObject)
    {
        obstacle = nObstacle;
        obstacleImg = gameObject;
        room = gameObject.GetComponent<ObstacleScript>().room;
    }

    public void destroy(Player maker) {
        maker.obstacles.Remove(this);
        GameObject.Destroy(obstacleImg);
    }
}