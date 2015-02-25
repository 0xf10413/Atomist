using UnityEngine;
using System.Collections;

public class Penalty {
    public delegate void Effect(Penalty p);

	int remainingTurns { get; set; }
	Player target { get; set; }
	public Effect effect;

	public Penalty(int turns, Player nTarget, Penalty.Effect nEffect) {
		remainingTurns = turns;
		target = nTarget;
		effect = nEffect;
	}
}
