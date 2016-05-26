namespace GrislyGrotto.Models
{
    public class PredicateValidator
    {
        public bool Valid { get; private set; }

        public PredicateValidator()
        {
            Valid = true;
        }

        public void Validate(bool bPredicate)
        {
            if (Valid && !bPredicate)
                Valid = false;
        }
    }
}
