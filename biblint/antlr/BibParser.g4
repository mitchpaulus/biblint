parser grammar BibParser;

options { tokenVocab=BibLexer; }

file : bibentry* EOF ;

bibentry : AT WORD LEFT_BRACE WORD COMMA fields  RIGHT_BRACE ;

fields : field (COMMA field)* COMMA? ;

field : WORD '=' value ;

value
  : FIELD_VALUE_WORD
  | TEXCONTENT
  ;
