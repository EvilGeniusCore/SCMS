using SCMS.Services.Template;

namespace SCMS.Tests.Services
{
    public class TemplateParserTests
    {
        private readonly TemplateParser _parser = new();

        [Fact]
        public void Parse_SimpleVariable_ReplacesToken()
        {
            var template = "Hello {{Name}}!";
            var context = new Dictionary<string, object> { ["Name"] = "World" };

            var result = _parser.Parse(template, context);

            Assert.Equal("Hello World!", result);
        }

        [Fact]
        public void Parse_MissingVariable_ReplacesWithEmpty()
        {
            var template = "Hello {{Missing}}!";
            var context = new Dictionary<string, object>();

            var result = _parser.Parse(template, context);

            Assert.Equal("Hello !", result);
        }

        [Fact]
        public void Parse_EachBlock_IteratesCollection()
        {
            var template = "{{#each Items}}[{{Name}}]{{/each}}";
            var context = new Dictionary<string, object>
            {
                ["Items"] = new List<object>
                {
                    new Dictionary<string, object> { ["Name"] = "A" },
                    new Dictionary<string, object> { ["Name"] = "B" },
                    new Dictionary<string, object> { ["Name"] = "C" }
                }
            };

            var result = _parser.Parse(template, context);

            Assert.Equal("[A][B][C]", result);
        }

        [Fact]
        public void Parse_EachBlock_EmptyCollection_ProducesNothing()
        {
            var template = "before{{#each Items}}[{{Name}}]{{/each}}after";
            var context = new Dictionary<string, object>
            {
                ["Items"] = new List<object>()
            };

            var result = _parser.Parse(template, context);

            Assert.Equal("beforeafter", result);
        }

        [Fact]
        public void Parse_IfBlock_TrueCondition_RendersContent()
        {
            var template = "{{#if Show}}visible{{/if}}";
            var context = new Dictionary<string, object> { ["Show"] = true };

            var result = _parser.Parse(template, context);

            Assert.Equal("visible", result);
        }

        [Fact]
        public void Parse_IfBlock_FalseCondition_RendersNothing()
        {
            var template = "{{#if Show}}visible{{/if}}";
            var context = new Dictionary<string, object> { ["Show"] = false };

            var result = _parser.Parse(template, context);

            Assert.Equal("", result);
        }

        [Fact]
        public void Parse_IfElseBlock_FalseCondition_RendersElse()
        {
            var template = "{{#if Show}}yes{{else}}no{{/if}}";
            var context = new Dictionary<string, object> { ["Show"] = false };

            var result = _parser.Parse(template, context);

            Assert.Equal("no", result);
        }

        [Fact]
        public void Parse_IfBlock_NullValue_IsFalse()
        {
            var template = "{{#if Missing}}yes{{else}}no{{/if}}";
            var context = new Dictionary<string, object>();

            var result = _parser.Parse(template, context);

            Assert.Equal("no", result);
        }

        [Fact]
        public void Parse_IfBlock_NonEmptyString_IsTrue()
        {
            var template = "{{#if Name}}has name{{/if}}";
            var context = new Dictionary<string, object> { ["Name"] = "Test" };

            var result = _parser.Parse(template, context);

            Assert.Equal("has name", result);
        }

        [Fact]
        public void Parse_IfBlock_EmptyString_IsFalse()
        {
            var template = "{{#if Name}}has name{{else}}no name{{/if}}";
            var context = new Dictionary<string, object> { ["Name"] = "" };

            var result = _parser.Parse(template, context);

            Assert.Equal("no name", result);
        }

        [Fact]
        public void Parse_IfBlock_NonEmptyList_IsTrue()
        {
            var template = "{{#if Items}}has items{{/if}}";
            var context = new Dictionary<string, object>
            {
                ["Items"] = new List<object> { new Dictionary<string, object> { ["X"] = "1" } }
            };

            var result = _parser.Parse(template, context);

            Assert.Equal("has items", result);
        }

        [Fact]
        public void Parse_IfBlock_EmptyList_IsFalse()
        {
            var template = "{{#if Items}}has items{{else}}empty{{/if}}";
            var context = new Dictionary<string, object>
            {
                ["Items"] = new List<object>()
            };

            var result = _parser.Parse(template, context);

            Assert.Equal("empty", result);
        }

        [Fact]
        public void Parse_NestedEach_ResolvesInnerContext()
        {
            var template = "{{#each Items}}{{#each Children}}{{Val}}{{/each}}{{/each}}";
            var context = new Dictionary<string, object>
            {
                ["Items"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["Children"] = new List<object>
                        {
                            new Dictionary<string, object> { ["Val"] = "X" },
                            new Dictionary<string, object> { ["Val"] = "Y" }
                        }
                    }
                }
            };

            var result = _parser.Parse(template, context);

            Assert.Equal("XY", result);
        }

        [Fact]
        public void Parse_HtmlComments_AreStripped()
        {
            var template = "before<!-- comment -->after";
            var context = new Dictionary<string, object>();

            var result = _parser.Parse(template, context);

            Assert.Equal("beforeafter", result);
        }

        [Fact]
        public void Parse_UnmatchedBlock_ThrowsException()
        {
            var template = "{{#each Items}}no closing";
            var context = new Dictionary<string, object>
            {
                ["Items"] = new List<object> { new Dictionary<string, object>() }
            };

            Assert.Throws<Exception>(() => _parser.Parse(template, context));
        }

        [Fact]
        public void Parse_IntZero_IsFalseInIf()
        {
            var template = "{{#if Count}}yes{{else}}no{{/if}}";
            var context = new Dictionary<string, object> { ["Count"] = 0 };

            var result = _parser.Parse(template, context);

            Assert.Equal("no", result);
        }

        [Fact]
        public void Parse_IntNonZero_IsTrueInIf()
        {
            var template = "{{#if Count}}yes{{else}}no{{/if}}";
            var context = new Dictionary<string, object> { ["Count"] = 5 };

            var result = _parser.Parse(template, context);

            Assert.Equal("yes", result);
        }
    }
}
