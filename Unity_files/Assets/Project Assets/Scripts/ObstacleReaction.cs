using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObstacleReaction : Reaction {

	Obstacle target { get; set; }

	public ObstacleReaction(string tReagents, string tProducts, List<KeyValuePair<Element,int>> lReagents, ReactionType nType, int cCost, int gGain, string nInfo) :
        base(tReagents,tProducts,lReagents,nType, cCost, gGain, "Détruit les obstacles de type \""+ weakness(nType).name +"\"", nInfo) {
	}
    /// <summary>
    /// Retourne la faiblesse de l'obstacle
    /// </summary>
    /// <returns></returns>
    private static Obstacle weakness(ReactionType type) {
        foreach (Obstacle obstacle in Main.obstacles) {
            if (obstacle.weakness == type)
                return obstacle;
        }
        return null;
    }

    public override void effect(Player maker) {
            /* On se limite aux obstacles dans la même pièce */
            List<ObstacleToken> potential_obstacles = maker.obstacles.FindAll (o => o.room == maker.room);
            /* Cette liste est de taille 0, 1 ou 2 */
            
            /* Si elle est nulle, il ne reste plus d'obstacle */
            if (potential_obstacles.Count == 0)
                return;

            /* Si elle vaut 1, il n'y a qu'un obstacle */
            if (potential_obstacles.Count == 1) 
                if (potential_obstacles[0].obstacle.weakness == type) {
                    potential_obstacles[0].destroy (maker);
                    maker.moveToNextRoom ();
                }
            
            /* Sinon, il y en a deux, et il faut savoir de quel côté aller */
            if (potential_obstacles.Count == 2) 
                if (potential_obstacles[0].obstacle.weakness == type) {
                    potential_obstacles[0].destroy (maker);
                    maker.moveToNextRoom (1);
                }
                else if (potential_obstacles[1].obstacle.weakness == type) {
                    potential_obstacles[1].destroy (maker);
                    maker.moveToNextRoom (2);
                }
        maker.consumeForReaction(this);
    }
}
