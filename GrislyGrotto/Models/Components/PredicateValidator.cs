namespace GrislyGrotto.Models.Components
{
    public class PredicateValidator
    {
        public bool Valid { get; private set; }

        public PredicateValidator()
        {
            Valid = true;
        }

        public void Validate(bool predicate)
        {
            if (Valid && !predicate)
                Valid = false;
        }
    }
}
