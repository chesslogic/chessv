//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.13.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from ChessVCLexer.g4 by ANTLR 4.13.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using System;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.13.1")]
[System.CLSCompliant(false)]
public partial class ChessVCLexer : Lexer {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		LINE_COMMENT=1, COMMENT=2, WHITESPACE=3, NULL=4, TRUE=5, FALSE=6, INT_T=7, 
		INTRANGE_T=8, BOOL_T=9, STRING_T=10, CHOICE_T=11, PIECETYPE_T=12, GAME_T=13, 
		IF=14, ELSE=15, RETURN=16, VAR=17, MIRROR_SYMMETRY=18, ROTATIONAL_SYMMETRY=19, 
		NO_SYMMETRY=20, ATTRIBUTE=21, IDENTIFIER=22, CHAR=23, STRING=24, INTEGER=25, 
		SYMMETRY=26;
	public static string[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"LINE_COMMENT", "COMMENT", "WHITESPACE", "NULL", "TRUE", "FALSE", "INT_T", 
		"INTRANGE_T", "BOOL_T", "STRING_T", "CHOICE_T", "PIECETYPE_T", "GAME_T", 
		"IF", "ELSE", "RETURN", "VAR", "MIRROR_SYMMETRY", "ROTATIONAL_SYMMETRY", 
		"NO_SYMMETRY", "STR_ESC", "ATTRIBUTE", "IDENTIFIER", "ID_ESC", "CHAR", 
		"STRING", "DIGIT", "INTEGER", "SYMMETRY"
	};


	public ChessVCLexer(ICharStream input)
	: this(input, Console.Out, Console.Error) { }

	public ChessVCLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
	: base(input, output, errorOutput)
	{
		Interpreter = new LexerATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	private static readonly string[] _LiteralNames = {
		null, null, null, null, "'null'", "'true'", "'false'", "'Integer'", "'IntRange'", 
		"'Bool'", "'String'", "'Choice'", "'PieceType'", "'Game'", "'if'", "'else'", 
		"'return'", "'var'", "'MirrorSymmetry'", "'RotationalSymmetry'", "'NoSymmetry'"
	};
	private static readonly string[] _SymbolicNames = {
		null, "LINE_COMMENT", "COMMENT", "WHITESPACE", "NULL", "TRUE", "FALSE", 
		"INT_T", "INTRANGE_T", "BOOL_T", "STRING_T", "CHOICE_T", "PIECETYPE_T", 
		"GAME_T", "IF", "ELSE", "RETURN", "VAR", "MIRROR_SYMMETRY", "ROTATIONAL_SYMMETRY", 
		"NO_SYMMETRY", "ATTRIBUTE", "IDENTIFIER", "CHAR", "STRING", "INTEGER", 
		"SYMMETRY"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "ChessVCLexer.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ChannelNames { get { return channelNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override int[] SerializedAtn { get { return _serializedATN; } }

	static ChessVCLexer() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}
	private static int[] _serializedATN = {
		4,0,26,291,6,-1,2,0,7,0,2,1,7,1,2,2,7,2,2,3,7,3,2,4,7,4,2,5,7,5,2,6,7,
		6,2,7,7,7,2,8,7,8,2,9,7,9,2,10,7,10,2,11,7,11,2,12,7,12,2,13,7,13,2,14,
		7,14,2,15,7,15,2,16,7,16,2,17,7,17,2,18,7,18,2,19,7,19,2,20,7,20,2,21,
		7,21,2,22,7,22,2,23,7,23,2,24,7,24,2,25,7,25,2,26,7,26,2,27,7,27,2,28,
		7,28,1,0,1,0,1,0,1,0,5,0,64,8,0,10,0,12,0,67,9,0,1,0,3,0,70,8,0,1,0,1,
		0,1,0,1,0,1,1,1,1,1,1,1,1,5,1,80,8,1,10,1,12,1,83,9,1,1,1,1,1,1,1,1,1,
		1,1,1,2,4,2,91,8,2,11,2,12,2,92,1,2,1,2,1,3,1,3,1,3,1,3,1,3,1,4,1,4,1,
		4,1,4,1,4,1,5,1,5,1,5,1,5,1,5,1,5,1,6,1,6,1,6,1,6,1,6,1,6,1,6,1,6,1,7,
		1,7,1,7,1,7,1,7,1,7,1,7,1,7,1,7,1,8,1,8,1,8,1,8,1,8,1,9,1,9,1,9,1,9,1,
		9,1,9,1,9,1,10,1,10,1,10,1,10,1,10,1,10,1,10,1,11,1,11,1,11,1,11,1,11,
		1,11,1,11,1,11,1,11,1,11,1,12,1,12,1,12,1,12,1,12,1,13,1,13,1,13,1,14,
		1,14,1,14,1,14,1,14,1,15,1,15,1,15,1,15,1,15,1,15,1,15,1,16,1,16,1,16,
		1,16,1,17,1,17,1,17,1,17,1,17,1,17,1,17,1,17,1,17,1,17,1,17,1,17,1,17,
		1,17,1,17,1,18,1,18,1,18,1,18,1,18,1,18,1,18,1,18,1,18,1,18,1,18,1,18,
		1,18,1,18,1,18,1,18,1,18,1,18,1,18,1,19,1,19,1,19,1,19,1,19,1,19,1,19,
		1,19,1,19,1,19,1,19,1,20,1,20,1,20,1,20,3,20,232,8,20,1,21,1,21,1,21,5,
		21,237,8,21,10,21,12,21,240,9,21,1,22,1,22,5,22,244,8,22,10,22,12,22,247,
		9,22,1,22,1,22,1,22,5,22,252,8,22,10,22,12,22,255,9,22,1,22,3,22,258,8,
		22,1,23,1,23,1,23,1,23,3,23,264,8,23,1,24,1,24,1,24,1,24,1,25,1,25,1,25,
		5,25,273,8,25,10,25,12,25,276,9,25,1,25,1,25,1,26,1,26,1,27,4,27,283,8,
		27,11,27,12,27,284,1,28,1,28,1,28,3,28,290,8,28,4,65,81,253,274,0,29,1,
		1,3,2,5,3,7,4,9,5,11,6,13,7,15,8,17,9,19,10,21,11,23,12,25,13,27,14,29,
		15,31,16,33,17,35,18,37,19,39,20,41,0,43,21,45,22,47,0,49,23,51,24,53,
		0,55,25,57,26,1,0,4,3,0,9,10,13,13,32,32,3,0,65,90,95,95,97,122,4,0,48,
		57,65,90,95,95,97,122,1,0,48,57,303,0,1,1,0,0,0,0,3,1,0,0,0,0,5,1,0,0,
		0,0,7,1,0,0,0,0,9,1,0,0,0,0,11,1,0,0,0,0,13,1,0,0,0,0,15,1,0,0,0,0,17,
		1,0,0,0,0,19,1,0,0,0,0,21,1,0,0,0,0,23,1,0,0,0,0,25,1,0,0,0,0,27,1,0,0,
		0,0,29,1,0,0,0,0,31,1,0,0,0,0,33,1,0,0,0,0,35,1,0,0,0,0,37,1,0,0,0,0,39,
		1,0,0,0,0,43,1,0,0,0,0,45,1,0,0,0,0,49,1,0,0,0,0,51,1,0,0,0,0,55,1,0,0,
		0,0,57,1,0,0,0,1,59,1,0,0,0,3,75,1,0,0,0,5,90,1,0,0,0,7,96,1,0,0,0,9,101,
		1,0,0,0,11,106,1,0,0,0,13,112,1,0,0,0,15,120,1,0,0,0,17,129,1,0,0,0,19,
		134,1,0,0,0,21,141,1,0,0,0,23,148,1,0,0,0,25,158,1,0,0,0,27,163,1,0,0,
		0,29,166,1,0,0,0,31,171,1,0,0,0,33,178,1,0,0,0,35,182,1,0,0,0,37,197,1,
		0,0,0,39,216,1,0,0,0,41,231,1,0,0,0,43,233,1,0,0,0,45,257,1,0,0,0,47,263,
		1,0,0,0,49,265,1,0,0,0,51,269,1,0,0,0,53,279,1,0,0,0,55,282,1,0,0,0,57,
		289,1,0,0,0,59,60,5,47,0,0,60,61,5,47,0,0,61,65,1,0,0,0,62,64,9,0,0,0,
		63,62,1,0,0,0,64,67,1,0,0,0,65,66,1,0,0,0,65,63,1,0,0,0,66,69,1,0,0,0,
		67,65,1,0,0,0,68,70,5,13,0,0,69,68,1,0,0,0,69,70,1,0,0,0,70,71,1,0,0,0,
		71,72,5,10,0,0,72,73,1,0,0,0,73,74,6,0,0,0,74,2,1,0,0,0,75,76,5,47,0,0,
		76,77,5,42,0,0,77,81,1,0,0,0,78,80,9,0,0,0,79,78,1,0,0,0,80,83,1,0,0,0,
		81,82,1,0,0,0,81,79,1,0,0,0,82,84,1,0,0,0,83,81,1,0,0,0,84,85,5,42,0,0,
		85,86,5,47,0,0,86,87,1,0,0,0,87,88,6,1,0,0,88,4,1,0,0,0,89,91,7,0,0,0,
		90,89,1,0,0,0,91,92,1,0,0,0,92,90,1,0,0,0,92,93,1,0,0,0,93,94,1,0,0,0,
		94,95,6,2,1,0,95,6,1,0,0,0,96,97,5,110,0,0,97,98,5,117,0,0,98,99,5,108,
		0,0,99,100,5,108,0,0,100,8,1,0,0,0,101,102,5,116,0,0,102,103,5,114,0,0,
		103,104,5,117,0,0,104,105,5,101,0,0,105,10,1,0,0,0,106,107,5,102,0,0,107,
		108,5,97,0,0,108,109,5,108,0,0,109,110,5,115,0,0,110,111,5,101,0,0,111,
		12,1,0,0,0,112,113,5,73,0,0,113,114,5,110,0,0,114,115,5,116,0,0,115,116,
		5,101,0,0,116,117,5,103,0,0,117,118,5,101,0,0,118,119,5,114,0,0,119,14,
		1,0,0,0,120,121,5,73,0,0,121,122,5,110,0,0,122,123,5,116,0,0,123,124,5,
		82,0,0,124,125,5,97,0,0,125,126,5,110,0,0,126,127,5,103,0,0,127,128,5,
		101,0,0,128,16,1,0,0,0,129,130,5,66,0,0,130,131,5,111,0,0,131,132,5,111,
		0,0,132,133,5,108,0,0,133,18,1,0,0,0,134,135,5,83,0,0,135,136,5,116,0,
		0,136,137,5,114,0,0,137,138,5,105,0,0,138,139,5,110,0,0,139,140,5,103,
		0,0,140,20,1,0,0,0,141,142,5,67,0,0,142,143,5,104,0,0,143,144,5,111,0,
		0,144,145,5,105,0,0,145,146,5,99,0,0,146,147,5,101,0,0,147,22,1,0,0,0,
		148,149,5,80,0,0,149,150,5,105,0,0,150,151,5,101,0,0,151,152,5,99,0,0,
		152,153,5,101,0,0,153,154,5,84,0,0,154,155,5,121,0,0,155,156,5,112,0,0,
		156,157,5,101,0,0,157,24,1,0,0,0,158,159,5,71,0,0,159,160,5,97,0,0,160,
		161,5,109,0,0,161,162,5,101,0,0,162,26,1,0,0,0,163,164,5,105,0,0,164,165,
		5,102,0,0,165,28,1,0,0,0,166,167,5,101,0,0,167,168,5,108,0,0,168,169,5,
		115,0,0,169,170,5,101,0,0,170,30,1,0,0,0,171,172,5,114,0,0,172,173,5,101,
		0,0,173,174,5,116,0,0,174,175,5,117,0,0,175,176,5,114,0,0,176,177,5,110,
		0,0,177,32,1,0,0,0,178,179,5,118,0,0,179,180,5,97,0,0,180,181,5,114,0,
		0,181,34,1,0,0,0,182,183,5,77,0,0,183,184,5,105,0,0,184,185,5,114,0,0,
		185,186,5,114,0,0,186,187,5,111,0,0,187,188,5,114,0,0,188,189,5,83,0,0,
		189,190,5,121,0,0,190,191,5,109,0,0,191,192,5,109,0,0,192,193,5,101,0,
		0,193,194,5,116,0,0,194,195,5,114,0,0,195,196,5,121,0,0,196,36,1,0,0,0,
		197,198,5,82,0,0,198,199,5,111,0,0,199,200,5,116,0,0,200,201,5,97,0,0,
		201,202,5,116,0,0,202,203,5,105,0,0,203,204,5,111,0,0,204,205,5,110,0,
		0,205,206,5,97,0,0,206,207,5,108,0,0,207,208,5,83,0,0,208,209,5,121,0,
		0,209,210,5,109,0,0,210,211,5,109,0,0,211,212,5,101,0,0,212,213,5,116,
		0,0,213,214,5,114,0,0,214,215,5,121,0,0,215,38,1,0,0,0,216,217,5,78,0,
		0,217,218,5,111,0,0,218,219,5,83,0,0,219,220,5,121,0,0,220,221,5,109,0,
		0,221,222,5,109,0,0,222,223,5,101,0,0,223,224,5,116,0,0,224,225,5,114,
		0,0,225,226,5,121,0,0,226,40,1,0,0,0,227,228,5,92,0,0,228,232,5,34,0,0,
		229,230,5,92,0,0,230,232,5,92,0,0,231,227,1,0,0,0,231,229,1,0,0,0,232,
		42,1,0,0,0,233,234,5,64,0,0,234,238,7,1,0,0,235,237,7,2,0,0,236,235,1,
		0,0,0,237,240,1,0,0,0,238,236,1,0,0,0,238,239,1,0,0,0,239,44,1,0,0,0,240,
		238,1,0,0,0,241,245,7,1,0,0,242,244,7,2,0,0,243,242,1,0,0,0,244,247,1,
		0,0,0,245,243,1,0,0,0,245,246,1,0,0,0,246,258,1,0,0,0,247,245,1,0,0,0,
		248,253,5,39,0,0,249,252,3,47,23,0,250,252,9,0,0,0,251,249,1,0,0,0,251,
		250,1,0,0,0,252,255,1,0,0,0,253,254,1,0,0,0,253,251,1,0,0,0,254,256,1,
		0,0,0,255,253,1,0,0,0,256,258,5,39,0,0,257,241,1,0,0,0,257,248,1,0,0,0,
		258,46,1,0,0,0,259,260,5,92,0,0,260,264,5,39,0,0,261,262,5,92,0,0,262,
		264,5,92,0,0,263,259,1,0,0,0,263,261,1,0,0,0,264,48,1,0,0,0,265,266,5,
		96,0,0,266,267,9,0,0,0,267,268,5,96,0,0,268,50,1,0,0,0,269,274,5,34,0,
		0,270,273,3,41,20,0,271,273,9,0,0,0,272,270,1,0,0,0,272,271,1,0,0,0,273,
		276,1,0,0,0,274,275,1,0,0,0,274,272,1,0,0,0,275,277,1,0,0,0,276,274,1,
		0,0,0,277,278,5,34,0,0,278,52,1,0,0,0,279,280,7,3,0,0,280,54,1,0,0,0,281,
		283,3,53,26,0,282,281,1,0,0,0,283,284,1,0,0,0,284,282,1,0,0,0,284,285,
		1,0,0,0,285,56,1,0,0,0,286,290,3,35,17,0,287,290,3,37,18,0,288,290,3,39,
		19,0,289,286,1,0,0,0,289,287,1,0,0,0,289,288,1,0,0,0,290,58,1,0,0,0,16,
		0,65,69,81,92,231,238,245,251,253,257,263,272,274,284,289,2,0,1,0,6,0,
		0
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
