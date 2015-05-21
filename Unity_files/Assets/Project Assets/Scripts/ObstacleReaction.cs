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
        foreach (ObstacleToken obstacle in maker.obstacles) {
            if ((obstacle.room == maker.room) && (obstacle.obstacle.weakness == type)) {
                obstacle.destroy(maker);
                maker.moveToNextRoom();
                break;
            }
        }
        maker.consumeForReaction(this);
    }
}
