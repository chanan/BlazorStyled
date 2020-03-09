namespace BlazorStyled
{
    public interface IConfig
    {
        bool IsDevelopment { get; set; }
        bool IsDebug { get; set; }
    }
}