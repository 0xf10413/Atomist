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

    /// <summary>
    /// Constructeur du jeton d'obstacle.
    /// </summary>
    /// <param name="nObstacle">L'obstacle référencé.</param>
    public ObstacleToken (Obstacle nObstacle, GameObject parent)
    {
        obstacle = nObstacle;
        /* Initialisation du GameObject */
        obstacleImg = new GameObject ();
        obstacleImg.name = "Token " + nObstacle.name;
        obstacleImg.AddComponent<Image> ();
        obstacleImg.GetComponent<Image> ().sprite = nObstacle.obstacleResource;
        obstacleImg.transform.SetParent (parent.transform);
        obstacleImg.transform.localPosition = new Vector2 ();
       // obstacleImg.AddComponent<RectTransform> ();
    }
}