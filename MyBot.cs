using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Battleships.Player.Interface;

namespace BattleshipBot
{
    public class DasBot : IBattleshipsBot
    {
        private const string Rows = "ABCDEFGHIJ";
        private IGridSquare lastTarget;
        private Random rng;

        public IEnumerable<IShipPosition> GetShipPositions()
        {
            lastTarget = null; // Forget all our history when we start a new game
            rng = new Random();
            while (true)
            {
                var shipPositions = InsertRandomShip(5, new List<IShipPosition>());
                shipPositions = InsertRandomShip(4, shipPositions);
                shipPositions = InsertRandomShip(3, shipPositions);
                shipPositions = InsertRandomShip(3, shipPositions);
                shipPositions = InsertRandomShip(2, shipPositions);
                if (shipPositions == null)
                    continue;
                return shipPositions;
            }
        }

        public IGridSquare SelectTarget()
        {
            var nextTarget = GetNextTarget();
            lastTarget = nextTarget;
            return nextTarget;
        }

        public void HandleShotResult(IGridSquare square, bool wasHit)
        {
            // Ignore whether we're successful
        }

        public void HandleOpponentsShot(IGridSquare square)
        {
            // Ignore what our opponent does
        }

        public string Name => "Das Bot";

        private IEnumerable<IShipPosition> InsertRandomShip(int length, IEnumerable<IShipPosition> shipPositions)
        {
            if (shipPositions == null)
                return null;
            var positionList = shipPositions.ToList();
            for (var i = 0; i < 20; i++)
            {
                var horizontal = rng.Next(0, 2);
                var row = Rows[rng.Next(10 - length * (1 - horizontal))];
                var column = rng.Next(1, 11 - length * horizontal);
                var newPosition = GetShipPosition(row, column, (char)(row + (length - 1) * (1 - horizontal)), column + (length - 1) * horizontal);

                if (positionList.All(position => !NeighbourhoodGridSquares(position, 0)
                    .Intersect(NeighbourhoodGridSquares(newPosition, 1)).Any()))
                {
                    positionList.Add(newPosition);
                    return positionList;
                }

               
            }
            return null;
        }

        private IEnumerable<IGridSquare> NeighbourhoodGridSquares(IShipPosition position, int radius)
        {
            var positionList = new List<IGridSquare>();

            var startColumn = Math.Max(position.StartingSquare.Column - radius, 1);
            var endColumn = Math.Min(position.EndingSquare.Column + radius, 10);
            var startRow = Math.Max(Rows.IndexOf(position.StartingSquare.Row) - radius, 0);
            var endRow = Math.Min(Rows.IndexOf(position.EndingSquare.Row) + radius, 9);

            for (var i = startRow; i < endRow + 1; i++)
            for (var j = startColumn; j < endColumn + 1; j++)
                positionList.Add(new GridSquare(Rows[i], j));

            return positionList;
        }

        private static ShipPosition GetShipPosition(char startRow, int startColumn, char endRow, int endColumn)
        {
            return new ShipPosition(new GridSquare(startRow, startColumn), new GridSquare(endRow, endColumn));
        }

        private IGridSquare GetNextTarget()
        {
            if (lastTarget == null)
                return new GridSquare('A', 1);

            var row = lastTarget.Row;
            var col = lastTarget.Column + 1;
            if (lastTarget.Column != 10)
                return new GridSquare(row, col);

            row = (char) (row + 1);
            if (row > 'J')
                row = 'A';
            col = 1;
            return new GridSquare(row, col);
        }
    }
}