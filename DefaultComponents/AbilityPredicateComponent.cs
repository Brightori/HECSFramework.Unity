namespace Components
{
    public partial class AbilityPredicateComponent 
    {
        partial void InitBP()
        {
            AbilityPredicates.Init();
            TargetPredicates.Init();
            AbilityOwnerPredicates.Init();
        }
    }
}