using BlazorStyled.Stylesheets;

namespace BlazorStyled
{
    public class GlobalStyles : IGlobalStyles
    {
        private string _id;
        private IStyleSheet _styleSheet;

        internal GlobalStyles(string id, IStyleSheet styleSheet)
        {
            _id = id;
            _styleSheet = styleSheet;
        }

        public string this[string globalClassName]
        {
            get
            {
                return _styleSheet.GlobalStyle(_id, globalClassName);
            }

            set
            {
                _styleSheet.AddOrUpdateGlobalStyle(_id, globalClassName, value);
            }
        }
    }
}
