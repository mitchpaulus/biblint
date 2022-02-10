lexer grammar BibLexer;

WORD : [a-zA-Z0-9_-]+ ;
AT : '@' ;
COMMA : ',' ;
EQUALS : '=' -> pushMode(INFIELD);

LEFT_BRACE : '{' ;
RIGHT_BRACE : '}';

WS : [ \t\n]+ -> skip ;

mode INFIELD ;

FIELD_WS : [ \t\n]+ -> skip ;

// Non greedy operator important here.
TEXCONTENT : '{' (TEXCONTENT | EscapeSequence | ~[}{] )*? '}'  -> popMode;
FIELD_VALUE_WORD : [a-zA-Z0-9-]+ -> popMode ;
fragment Special : '{'  | '}' ;
fragment EscapeSequence : '\\' Special ;
