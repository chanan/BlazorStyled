namespace BlazorStyled.Stylesheets
{
    public class Declaration
    {
        public string Property { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return $"{Property}: {Value};";
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
