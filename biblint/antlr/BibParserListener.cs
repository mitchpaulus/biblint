//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.8
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from BibParser.g4 by ANTLR 4.8

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using Antlr4.Runtime.Misc;
using IParseTreeListener = Antlr4.Runtime.Tree.IParseTreeListener;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete listener for a parse tree produced by
/// <see cref="BibParser"/>.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.8")]
[System.CLSCompliant(false)]
public interface IBibParserListener : IParseTreeListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="BibParser.file"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFile([NotNull] BibParser.FileContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="BibParser.file"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFile([NotNull] BibParser.FileContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="BibParser.bibentry"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBibentry([NotNull] BibParser.BibentryContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="BibParser.bibentry"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBibentry([NotNull] BibParser.BibentryContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="BibParser.fields"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFields([NotNull] BibParser.FieldsContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="BibParser.fields"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFields([NotNull] BibParser.FieldsContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="BibParser.field"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterField([NotNull] BibParser.FieldContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="BibParser.field"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitField([NotNull] BibParser.FieldContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="BibParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterValue([NotNull] BibParser.ValueContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="BibParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitValue([NotNull] BibParser.ValueContext context);
}
