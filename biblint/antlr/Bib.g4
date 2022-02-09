grammar Bib ;

WORD : /[a-zA-Z0-9-]+/ ;
AT : '@' ;

file : bibentry* EOF ;

bibentry : AT WORD '{' field  '}' ;

fields : field (',' field)* ','? ;

field : WORD '=' WORD ;

WS : /[ \t\n]+/ -> skip ;
