using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Symbolise un jeton d'obstacle. Contient un GameObject, et un lien vers
/// l'obstacle qu'il repr�sente.
/// </summary>
public class ObstacleToken
{
    public Obstacle obstacle { get; private set; } // L'obstacle repr�sent�
    public GameObject obstacleImg { get;  set; } // Le GameObject correspondant
    public int salle; // Le num�ro de la salle

    /// <summary>
    /// Constructeur du jeton d'obstacle.
    /// </summary>
    /// <param name="nObstacle">L'obstacle r�f�renc�.</param>
    /// <param name="gameObject">Le gameObject contenant le jeton.</param>
    public ObstacleToken (Obstacle nObstacle, GameObject gameObject)
    {
        obstacle = nObstacle;
        obstacleImg = gameObject;
        salle = gameObject.GetComponent<ObstacleScript>().salle;
        /* Initialisation du GameObject */
        // D�sol� Florent, ton code sert � rien en fait ^^
        /*obstacleImg = new GameObject ();
        obstacleImg.name = "Token " + nObstacle.name;
        obstacleImg.AddComponent<Image> ();
        obstacleImg.GetComponent<Image> ().sprite = nObstacle.obstacleResource;
        obstacleImg.transform.SetParent (parent.transform);
        obstacleImg.transform.localPosition = new Vector2 ();*/
       // obstacleImg.AddComponent<RectTransform> ();
    }

    public void destroy() {
        GameObject.Destroy(obstacleImg);
    }
}