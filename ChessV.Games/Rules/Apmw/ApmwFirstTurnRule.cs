using System.Collections.Generic;

namespace ChessV.Games.Rules.Apmw
{
    public class ApmwFirstTurnRule : Rule
    {
        private CheckmateRule CheckmateRule;
        private int hashKeyIndex;

        public override void Initialize(Game game)
        {
            base.Initialize(game);
            CheckmateRule = game.FindRule(typeof(CheckmateRule), true) as CheckmateRule;
        }

        public override void GenerateSpecialMoves(MoveList list, bool capturesOnly, int ply)
        {
            // Only apply this rule in the first two turns (ply <= 3)
            if (ply > 3)
                return;

            // Get the current player's king square
            int kingSquare = -1;
            BitBoard royalPieces = Board.GetPieceTypeBitboard(Game.CurrentSide, CheckmateRule.RoyalPieceTypes[0].TypeNumber);
            if (royalPieces.BitCount > 0)
                kingSquare = royalPieces.LSB;
            else
                return;

            // If we're not in check, no need to generate counter captures
            if (!Game.IsSquareAttacked(kingSquare, Game.CurrentSide ^ 1))
                return;

            // Find all attacking pieces
            HashSet<int> attackingSquares = new HashSet<int>();
            for (int square = 0; square < Board.NumSquares; square++)
            {
                Piece piece = Board[square];
                if (piece != null && piece.Player == (Game.CurrentSide ^ 1))
                {
                    // Check if this piece is attacking the king by generating its moves
                    MoveCapability[] moves;
                    int nMoves = piece.PieceType.GetMoveCapabilities(out moves);
                    for (int nMove = 0; nMove < nMoves; nMove++)
                    {
                        MoveCapability move = moves[nMove];
                        if (!move.CanCapture)
                            continue;

                        int step = 1;
                        int nextSquare = Board.NextSquare(piece.Player, move.NDirection, piece.Square);
                        while (nextSquare >= 0 && step <= move.MaxSteps)
                        {
                            if (nextSquare == kingSquare)
                            {
                                attackingSquares.Add(piece.Square);
                                break;
                            }
                            else if (Board[nextSquare] != null)
                                break;

                            nextSquare = Board.NextSquare(piece.Player, move.NDirection, nextSquare);
                            step++;
                        }
                    }
                }
            }

            // For each of our pieces, generate special capture moves against the attacking pieces
            for (int fromSquare = 0; fromSquare < Board.NumSquares; fromSquare++)
            {
                Piece piece = Board[fromSquare];
                if (piece != null && piece.Player == Game.CurrentSide)
                {
                    foreach (int attackingSquare in attackingSquares)
                    {
                        // Add a special capture move
                        int capturedPieceValue = Board[attackingSquare].PieceType.MidgameValue;
                        list.BeginMoveAdd(MoveType.StandardCapture, fromSquare, attackingSquare);
                        list.AddCapture(fromSquare, attackingSquare);
                        // Give these moves a high priority since they prevent check
                        list.EndMoveAdd(4000 + capturedPieceValue);
                    }
                }
            }
        }

        public override void GetNotesForPieceType(PieceType type, List<string> notes)
        {
            notes.Add("first turn counter capture");
        }
    }
} 