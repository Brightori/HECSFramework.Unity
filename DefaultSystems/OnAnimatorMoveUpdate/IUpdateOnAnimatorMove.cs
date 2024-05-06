using HECSFramework.Core;
using Systems;

/// <summary>
/// read documentation for understanding how use this functionality 
/// </summary>
[Documentation(Doc.HECS, Doc.OnAnimatorMoveUpdate, "u should declare this interface on actors systems, and have inject system on actor " + nameof(InjectAnimatorMoveUpdateSystem))]
public interface IUpdateOnAnimatorMove
{
    public void UpdateOnAnimatorMove();
}