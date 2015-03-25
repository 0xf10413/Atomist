using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ObstacleScript : MonoBehaviour {

    public string obstacleName;
    public int salle;

    public bool initialized = false;

	// Use this for initialization
	void Start () {
        List<Obstacle> obstacles = Main.obstacles;
        if (obstacles != null) {
            Obstacle obstacle = obstacles.Find(n => n.name == obstacleName);
	        gameObject.transform.GetComponent<Image>().sprite = obstacle.obstacleResource;
        }
	}
	
	// Update is called once per frame
	void Update () {
	}
}
