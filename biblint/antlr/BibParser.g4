parser grammar BibParser;

options { tokenVocab=BibLexer; }

file : bibentry* EOF ;

bibentry : AT WORD LEFT_BRACE WORD COMMA fields  RIGHT_BRACE ;

fields : field (COMMA field)* COMMA? ;

field : WORD '=' value ;

value
  : WORD
  | TEXCONTENT
  ;

/* texField */
  /* : LEFT_BRACE_TEX RIGHT_BRACE_TEX */
  /* | LEFT_BRACE_TEX TEXCONTENT RIGHT_BRACE_TEX */
  /* | LEFT_BRACE_TEX TEXCONTENT texField RIGHT_BRACE_TEX */
  /* | LEFT_BRACE_TEX TEXCONTENT texField TEXCONTENT RIGHT_BRACE_TEX */
  /* ; */
