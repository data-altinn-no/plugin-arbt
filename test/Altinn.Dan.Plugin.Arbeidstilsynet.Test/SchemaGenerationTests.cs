using Altinn.Dan.Plugin.Arbeidstilsynet.Models;
using Dan.Common.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Altinn.Dan.Plugin.Arbeidstilsynet.Test
{
    [TestClass]
    public class SchemaGenerationTests
    {
        [TestMethod]
        public void Bemanning_SchemaUsesLowerCamelCasePropertyNames()
        {
            var schema = EvidenceValue.SchemaFromObject<Bemanning>(Formatting.Indented);

            StringAssert.Contains(schema, "\"organisasjonsnummer\"");
            StringAssert.Contains(schema, "\"godkjenningsstatus\"");

            Assert.IsFalse(schema.Contains("\"Organisasjonsnummer\""));
            Assert.IsFalse(schema.Contains("\"Godkjenningsstatus\""));
        }

        [TestMethod]
        public void Renhold_SchemaUsesLowerCamelCasePropertyNames()
        {
            var schema = EvidenceValue.SchemaFromObject<Renhold>(Formatting.Indented);

            StringAssert.Contains(schema, "\"organisasjonsnummer\"");
            StringAssert.Contains(schema, "\"status\"");
            StringAssert.Contains(schema, "\"statusEndret\"");

            Assert.IsFalse(schema.Contains("\"Organisasjonsnummer\""));
            Assert.IsFalse(schema.Contains("\"Status\""));
            Assert.IsFalse(schema.Contains("\"StatusEndret\""));
        }

        [TestMethod]
        public void BilpleieregisterResult_RegisterstatusIsDeclaredAsStringInSchema()
        {
            var schema = EvidenceValue.SchemaFromObject<BilpleieregisterResult>(Formatting.Indented);
            var parsed = JObject.Parse(schema);
            var registerstatusType = parsed["properties"]?["registerstatus"]?["type"]?.ToString() ?? string.Empty;

            StringAssert.Contains(registerstatusType, "string");
            Assert.IsFalse(registerstatusType.Contains("integer"));
        }
    }
}
