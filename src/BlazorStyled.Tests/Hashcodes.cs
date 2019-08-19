using Microsoft.VisualStudio.TestTools.UnitTesting;
using BlazorStyled.Internal;

namespace BlazorStyled.Tests
{
    [TestClass]
    public class Hashcodes
    {
        [TestMethod]
        public void Decleration()
        {
            RuleSet ruleSet = new RuleSet();
            ruleSet.AddDeclaration(new Stylesheets.Declaration { Property = "color", Value = "red" });
            Hash hash = new Hash();
            var hashcode = hash.GetHashCode(ruleSet);
            Assert.AreEqual("dqma-qdh", hashcode);
        }
    }
}