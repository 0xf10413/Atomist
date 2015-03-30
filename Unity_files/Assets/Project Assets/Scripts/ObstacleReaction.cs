using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObstacleReaction : Reaction {

	Obstacle target { get; set; }

	public ObstacleReaction(string tReagents, string tProducts, List<KeyValuePair<Element,int>> lReagents, ReactionType nType, int cCost, int gGain) :
        base(tReagents,tProducts,lReagents,nType, cCost, gGain) {
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
