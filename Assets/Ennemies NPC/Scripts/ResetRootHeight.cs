using UnityEngine;

public class ResetRootHeight : StateMachineBehaviour
{   
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //animator.rootPosition = new Vector3(animator.rootPosition.x, 0, animator.rootPosition.z);
    }
}
