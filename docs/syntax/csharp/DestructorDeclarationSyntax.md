# DestructorDeclarationSyntax

## Inheritance

* Object
  * SyntaxNode
    * CSharpSyntaxNode
      * [MemberDeclarationSyntax](MemberDeclarationSyntax.md)
        * [BaseMethodDeclarationSyntax](BaseMethodDeclarationSyntax.md)
          * DestructorDeclarationSyntax

## Syntax Properties

| Name           | Type                                                          |
| -------------- | ------------------------------------------------------------- |
| AttributeLists | SyntaxList\<[AttributeListSyntax](AttributeListSyntax.md)>    |
| Modifiers      | SyntaxTokenList                                               |
| TildeToken     | SyntaxToken                                                   |
| Identifier     | SyntaxToken                                                   |
| ParameterList  | [ParameterListSyntax](ParameterListSyntax.md)                 |
| Body           | [BlockSyntax](BlockSyntax.md)                                 |
| ExpressionBody | [ArrowExpressionClauseSyntax](ArrowExpressionClauseSyntax.md) |
| SemicolonToken | SyntaxToken                                                   |

## See Also

* [Official Documentation](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.csharp.syntax.destructordeclarationsyntax)


*\(Generated with [DotMarkdown](http://github.com/JosefPihrt/DotMarkdown)\)*