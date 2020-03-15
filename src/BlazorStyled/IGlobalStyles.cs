namespace BlazorStyled
{
    public interface IGlobalStyles
    {
        string this[string globalClassName] { get; set; }
    }
}