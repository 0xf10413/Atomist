using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Reaction {

	string reagents  { get; set; }
	string products  { get; set; }
	List<KeyValuePair<Element,int>> reagentsList  { get; set; }
	ReactionType type  { get; set; }

	public Reaction(string tReagents, string tProducts, List<KeyValuePair<Element,int>> lReagents, ReactionType nType) {
		reagents = tReagents;
		products = tProducts;
		reagentsList = lReagents;
		type = nType;
	}
}
