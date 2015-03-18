using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class PoisonReaction : DelayedReaction
{
    public int nbOfTurns { get; private set; }
    public PoisonReaction (string tReagents, string tProducts, List<KeyValuePair<Element, int>> lReagents, ReactionType nType, int cCost, int gGain, int nnbOfTurns)
        : base (tReagents, tProducts, lReagents, nType, cCost, gGain)
    {
        nbOfTurns = nnbOfTurns;
    }

    public override void inflict(Player target)
    {
        target.penalties.Add (new Penalty (nbOfTurns, target, p => { Main.infoDialog ("Tour annulé !", delegate { p.target.EndTurn (); }); }));
    }
}