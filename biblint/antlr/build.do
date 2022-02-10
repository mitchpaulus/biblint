#!/bin/sh
set -e

java -jar "$ANTLR_JAR" BibLexer.g4
java -jar "$ANTLR_JAR" BibParser.g4

java -jar "$ANTLR_JAR" -Dlanguage=CSharp BibLexer.g4
java -jar "$ANTLR_JAR" -Dlanguage=CSharp BibParser.g4

javac *.java
