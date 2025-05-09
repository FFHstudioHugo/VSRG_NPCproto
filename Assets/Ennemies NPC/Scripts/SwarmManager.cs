using System.Collections.Generic;
using UnityEngine;

public class SwarmManager : MonoBehaviour
{
    public Transform player;

    public int maxConcurrentAttackers = 2;

    public List<SwarmInsect> insects;
    private HashSet<SwarmInsect> currentAttackers = new HashSet<SwarmInsect>();

    public void Register(SwarmInsect insect)
    {
        insects.Add(insect);
        insect.AssignManager(this, insects.Count);
    }

    public bool TryRequestAttack(SwarmInsect insect)
    {
        if (currentAttackers.Count < maxConcurrentAttackers)
        {
            currentAttackers.Add(insect);
            return true;
        }
        return false;
    }

    public void NotifyAttackEnded(SwarmInsect insect)
    {
        currentAttackers.Remove(insect);
    }
}
