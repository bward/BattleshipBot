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
        private Random rng;
        private List<IGridSquare> firingStack;
        private HashSet<IGridSquare> firedSquares;

        public DasBot()
        {
            firedSquares = new HashSet<IGridSquare>();
            rng = new Random();
            firingStack = new List<IGridSquare>();
            for (var i = 0; i < 10; i++)
            for (var j = 1; j < 11; j += 2)
                firingStack.Add(new GridSquare(Rows[i], j + i % 2));

            firingStack = firingStack.OrderBy(item => rng.Next()).ToList();
            
        }

        public IEnumerable<IShipPosition> GetShipPositions()
        { 
            return InsertRandomShips(new List<int> {5, 4, 3, 3, 2}, new List<IShipPosition>());
        }

        public IGridSquare SelectTarget()
        {
            var target = firingStack.Last();
            firingStack.RemoveAt(firingStack.Count - 1);
            return target;
        }

        public void HandleShotResult(IGridSquare square, bool wasHit)
        {
            firedSquares.Add(square);
            if (wasHit)
            {
                var newTargets = NeighbourhoodGridSquares(new ShipPosition(square, square), 1)
                    .Where(neighbour => (neighbour.Column + Rows.IndexOf(neighbour.Row)) % 2 !=
                                        (square.Column + Rows.IndexOf(square.Row)) % 2)
                    .Where(neighbour => !firedSquares.Contains(neighbour));
                foreach (var target in newTargets)
                {
                    firingStack.RemoveAll(s => s.Equals(target));
                    firingStack.Add(target);
                }
            }

        }

        public void HandleOpponentsShot(IGridSquare square)
        {
            // Ignore what our opponent does
        }

        public string Name => "Das Bot";

        private IEnumerable<IShipPosition> InsertRandomShips(IReadOnlyCollection<int> lengths, IEnumerable<IShipPosition> shipPositions)
        {
            if (shipPositions == null)
                return null;

            var positionList = shipPositions.ToList();

            if (lengths.Count > 1)
            {
                var output = InsertRandomShips(lengths.Take(1).ToList(), InsertRandomShips(lengths.Skip(1).ToList(), positionList));
                if (output == null)
                    return InsertRandomShips(lengths, positionList);
                return output;
            }

            var length = lengths.First();
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

    }
}