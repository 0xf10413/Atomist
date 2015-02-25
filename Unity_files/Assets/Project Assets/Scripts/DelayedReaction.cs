using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DelayedReaction : Reaction {

	public DelayedReaction(string tReagents, string tProducts, List<KeyValuePair<Element,int>> lReagents, ReactionType nType) : base(tReagents,tProducts,lReagents,nType) {
	}

	public void inflige(Player cible) {

	}
}
