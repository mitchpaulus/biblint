lexer grammar BibLexer;

WORD : [a-zA-Z0-9-]+ ;
AT : '@' ;
COMMA : ',' ;
EQUALS : '=' -> pushMode(INFIELD);

LEFT_BRACE : '{' ;
RIGHT_BRACE : '}';

WS : [ \t\n]+ -> skip ;

/* mode INENTRY ; */

/* LEFT_BRACE_TEX : '{' -> pushMode(INFIELD); */
/* RIGHT_BRACE_TEX : '}' -> popMode; */
/* ENTRY_WORD : /[a-zA-Z0-9-]+/ ; */
/* ENTRY_COMMA : ',' ; */
/* [> EQUALS : '=' ; <] */
/* ENTRYWS : /[ \t\n]+/ -> skip ; */

mode INFIELD ;

FIELD_WS : [ \t\n]+ -> skip ;
TEXCONTENT : '{' (TEXCONTENT | EscapeSequence | ~[{}] )* '}' -> popMode;
fragment EscapeSequence : '\\{' | '\\}' ;
