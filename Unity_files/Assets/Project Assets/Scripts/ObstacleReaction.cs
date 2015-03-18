using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObstacleReaction : Reaction {

	Obstacle target { get; set; }

	public ObstacleReaction(string tReagents, string tProducts, List<KeyValuePair<Element,int>> lReagents, ReactionType nType, int cCost, int gGain) :
        base(tReagents,tProducts,lReagents,nType, cCost, gGain) {
	}

    public override void effect() {
    }
}
