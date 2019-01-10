﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.IntegrationTests;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.Extensions.DependencyModel;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Razor.Extensions.Version1_X.IntegrationTests
{
    public class CodeGenerationIntegrationTest : IntegrationTestBase
    {
        private static readonly RazorSourceDocument DefaultImports = MvcRazorTemplateEngine.GetDefaultImports();

        private CSharpCompilation BaseCompilation => MvcShim.BaseCompilation.WithAssemblyName("AppCode");

        [Fact(Skip="https://github.com/aspnet/AspNetCore/issues/6549")]
        public void InvalidNamespaceAtEOF_DesignTime()
        {
            var compilation = BaseCompilation;
            RunDesignTimeTest(compilation);
        }

        [Fact(Skip="https://github.com/aspnet/AspNetCore/issues/6549")]
        public void IncompleteDirectives_DesignTime()
        {
            var appCode = @"
public class MyService<TModel>
{
    public string Html { get; set; }
}
";

            var compilation = BaseCompilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(appCode));
            RunDesignTimeTest(compilation);
        }

        [Fact(Skip="https://github.com/aspnet/AspNetCore/issues/6549")]
        public void InheritsViewModel_DesignTime()
        {
            var appCode = @"
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Razor;

public class MyBasePageForViews<TModel> : RazorPage
{
    public override Task ExecuteAsync()
    {
        throw new System.NotImplementedException();
    }
}

public class MyModel
{

}
";
            var compilation = BaseCompilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(appCode));
            RunDesignTimeTest(compilation);
        }

        [Fact(Skip="https://github.com/aspnet/AspNetCore/issues/6549")]
        public void InheritsWithViewImports_DesignTime()
        {
            var appCode = @"
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Razor;

public class MyBasePageForViews<TModel> : RazorPage
{
    public override Task ExecuteAsync()
    {
        throw new System.NotImplementedException();
    }
}

public class MyModel
{

}
";

            var compilation = BaseCompilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(appCode));
            RunDesignTimeTest(compilation);
        }

        [Fact(Skip="https://github.com/aspnet/AspNetCore/issues/6549")]
        public void Basic_DesignTime()
        {
            var compilation = BaseCompilation;
            RunDesignTimeTest(compilation);
        }

        [Fact(Skip="https://github.com/aspnet/AspNetCore/issues/6549")]
        public void Sections_DesignTime()
        {
            var appCode = $@"
using Microsoft.AspNetCore.Mvc.ViewFeatures;

public class InputTestTagHelper : {typeof(TagHelper).FullName}
{{
    public ModelExpression For {{ get; set; }}
}}
";

            var compilation = BaseCompilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(appCode));

            RunDesignTimeTest(compilation);
        }

        [Fact(Skip="https://github.com/aspnet/AspNetCore/issues/6549")]
        public void _ViewImports_DesignTime()
        {
            var compilation = BaseCompilation;
            RunDesignTimeTest(compilation);
        }

        [Fact(Skip="https://github.com/aspnet/AspNetCore/issues/6549")]
        public void Inject_DesignTime()
        {
            var appCode = @"
public class MyApp
{
    public string MyProperty { get; set; }
}
";
            var compilation = BaseCompilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(appCode));
            RunDesignTimeTest(compilation);
        }

        [Fact(Skip="https://github.com/aspnet/AspNetCore/issues/6549")]
        public void InjectWithModel_DesignTime()
        {
            var appCode = @"
public class MyModel
{

}

public class MyService<TModel>
{
    public string Html { get; set; }
}

public class MyApp
{
    public string MyProperty { get; set; }
}
";
            var compilation = BaseCompilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(appCode));
            RunDesignTimeTest(compilation);
        }

        [Fact(Skip="https://github.com/aspnet/AspNetCore/issues/6549")]
        public void InjectWithSemicolon_DesignTime()
        {
            var appCode = @"
public class MyModel
{

}

public class MyService<TModel>
{
    public string Html { get; set; }
}

public class MyApp
{
    public string MyProperty { get; set; }
}
";
            var compilation = BaseCompilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(appCode));
            RunDesignTimeTest(compilation);
        }

        [Fact(Skip="https://github.com/aspnet/AspNetCore/issues/6549")]
        public void Model_DesignTime()
        {
            var compilation = BaseCompilation;
            RunDesignTimeTest(compilation);
        }

        [Fact(Skip="https://github.com/aspnet/AspNetCore/issues/6549")]
        public void MultipleModels_DesignTime()
        {
            var appCode = @"
public class ThisShouldBeGenerated
{

}";

            var compilation = BaseCompilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(appCode));
            RunDesignTimeTest(compilation);
        }
        
        [Fact(Skip="https://github.com/aspnet/AspNetCore/issues/6549")]
        public void ModelExpressionTagHelper_DesignTime()
        {
            var appCode = $@"
using Microsoft.AspNetCore.Mvc.ViewFeatures;

public class InputTestTagHelper : {typeof(TagHelper).FullName}
{{
    public ModelExpression For {{ get; set; }}
}}
";
            var compilation = BaseCompilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(appCode));

            RunDesignTimeTest(compilation);
        }

        [Fact(Skip="https://github.com/aspnet/AspNetCore/issues/6549")]
        public void ViewComponentTagHelper_DesignTime()
        {
            var appCode = $@"
public class TestViewComponent
{{
    public string Invoke(string firstName)
    {{
        return firstName;
    }}
}}

[{typeof(HtmlTargetElementAttribute).FullName}]
public class AllTagHelper : {typeof(TagHelper).FullName}
{{
    public string Bar {{ get; set; }}
}}
";

            var compilation = BaseCompilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(appCode));

            RunDesignTimeTest(compilation);
        }

        private void RunDesignTimeTest(
            CSharpCompilation baseCompilation,
            IEnumerable<string> expectedErrors = null)
        {
            Assert.Empty(baseCompilation.GetDiagnostics());
            
            // Arrange
            var engine = CreateEngine(baseCompilation);
            var projectItem = CreateProjectItem();

            // Act
            var document = engine.ProcessDesignTime(projectItem);

            // Assert
            AssertDocumentNodeMatchesBaseline(document.GetDocumentIntermediateNode());
            AssertCSharpDocumentMatchesBaseline(document.GetCSharpDocument());
            AssertSourceMappingsMatchBaseline(document);
            AssertDocumentCompiles(document, baseCompilation, expectedErrors);
        }

        private void AssertDocumentCompiles(
            RazorCodeDocument document,
            CSharpCompilation baseCompilation,
            IEnumerable<string> expectedErrors = null)
        {
            var cSharp = document.GetCSharpDocument().GeneratedCode;

            var syntaxTree = CSharpSyntaxTree.ParseText(cSharp);
            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

            var references = baseCompilation.References.Concat(new[] { baseCompilation.ToMetadataReference() });
            var compilation = CSharpCompilation.Create("CodeGenerationTestAssembly", new[] { syntaxTree }, references, options);

            var diagnostics = compilation.GetDiagnostics();

            var errors = diagnostics.Where(d => d.Severity >= DiagnosticSeverity.Warning);

            if (expectedErrors == null)
            {
                Assert.Empty(errors.Select(e => e.GetMessage()));
            }
            else
            {
                Assert.Equal(expectedErrors, errors.Select(e => e.GetMessage()));
            }
        }

        protected RazorProjectEngine CreateEngine(CSharpCompilation compilation)
        {
            var references = compilation.References.Concat(new[] { compilation.ToMetadataReference() });

            return CreateProjectEngine(b =>
            {
                RazorExtensions.Register(b);
                RazorExtensions.RegisterViewComponentTagHelpers(b);

                var existingImportFeature = b.Features.OfType<IImportProjectFeature>().Single();
                b.SetImportFeature(new NormalizedDefaultImportFeature(existingImportFeature));

                b.Features.Add(GetMetadataReferenceFeature(references));
                b.Features.Add(new CompilationTagHelperFeature());
            });
        }

        private static IRazorEngineFeature GetMetadataReferenceFeature(IEnumerable<MetadataReference> references)
        {
            return new DefaultMetadataReferenceFeature()
            {
                References = references.ToList()
            };
        }

        private class NormalizedDefaultImportFeature : RazorProjectEngineFeatureBase, IImportProjectFeature
        {
            private IImportProjectFeature _existingFeature;

            public NormalizedDefaultImportFeature(IImportProjectFeature existingFeature)
            {
                _existingFeature = existingFeature;
            }

            protected override void OnInitialized()
            {
                _existingFeature.ProjectEngine = ProjectEngine;
            }

            public IReadOnlyList<RazorProjectItem> GetImports(RazorProjectItem projectItem)
            {
                var normalizedImports = new List<RazorProjectItem>();
                var imports = _existingFeature.GetImports(projectItem);
                foreach (var import in imports)
                {
                    var text = string.Empty;
                    using (var stream = import.Read())
                    using (var reader = new StreamReader(stream))
                    {
                        text = reader.ReadToEnd().Trim();
                    }

                    // It's important that we normalize the newlines in the default imports. The default imports will
                    // be created with Environment.NewLine, but we need to normalize to `\r\n` so that the indices
                    // are the same on xplat.
                    var normalizedText = Regex.Replace(text, "(?<!\r)\n", "\r\n", RegexOptions.None, TimeSpan.FromSeconds(10));
                    var normalizedImport = new TestRazorProjectItem(import.FilePath, import.PhysicalPath, import.RelativePhysicalPath, import.BasePath)
                    {
                        Content = normalizedText
                    };

                    normalizedImports.Add(normalizedImport);
                }

                return normalizedImports;
            }
        }
    }
}
