using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NNUI1_sem._1_Matejka
{
    public class Maze
    {
        private const int VisualMultiplicator = 10;
        private const int TurnPunishment = 2;
        private const int FullTurnPunishment = 3;
        private const int ForwardPunishment = 5;

        static readonly Color WallColor = Color.FromArgb(0,0,0);
        static readonly Color PathColor = Color.FromArgb(255,255,255);

        public bool FoundSolutin { get; set; }
        public Action<Maze> UpdateUi { get; set; }

        public Bitmap BitmapImage { get; set; } = new(32, 32);
        public Bitmap Visualization { get; set; }
        public int Width => BitmapImage.Width;
        public int Height => BitmapImage.Height;

        private Point StartPoint { get; } = new();
        private Point EndPoint { get; } = new();
        public State Agent { get; set; } = new();

        public List<Point> DiscoveredPlaces { get; } = new();
        public List<State> Successors { get; } = new();

        public async Task<Bitmap> BuildVisualBitmap()
        {
            
            Visualization = new Bitmap(BitmapImage.Width * VisualMultiplicator, BitmapImage.Height * VisualMultiplicator);
            for (int i = 0; i < BitmapImage.Width; i++)
            {
                for (int j = 0; j < BitmapImage.Height; j++)
                {
                    await FillSquareInBitmap(BitmapImage.GetPixel(i, j), new Point() { X = i, Y = j });
                }
            }
            return Visualization;
        }

        public async Task FindSolutionAsync()
        {
            await BuildVisualBitmap();
            await SetStartPoint(StartPoint.X, StartPoint.Y);
            await SetEndPoint(EndPoint.X, EndPoint.Y);
            Successors.Clear();

            Agent.Point = StartPoint;
            Successors.Add(Agent);
            DiscoveredPlaces.Clear();
            FoundSolutin = false;
            while (Successors.Count > 0)
            {
                var successor = await FindNextSuccessor();
                await FillSquareInBitmap(Color.Blue, successor.Point);
                Agent = successor;
                await Task.Delay(10);
                UpdateUi?.Invoke(this);
                if (successor.Point.Equals(EndPoint))
                {
                    FoundSolutin = true;
                    State pathState = Agent;
                    while (pathState is not null)
                    {
                        await FillSquareInBitmap(Color.GreenYellow, pathState.Point);

                        pathState = pathState.PreviousState;
                    }
                    UpdateUi?.Invoke(this);
                    break;
                }

                await FindSuccessors();
            }
        }

        public async Task SetStartPoint(int x, int y)
        {
            Point prev = new Point()
            {
                X = StartPoint.X,
                Y = StartPoint.Y
            };
            StartPoint.Y = y;
            StartPoint.X = x;
            Agent.Point.X = x;
            Agent.Point.Y = y;
            await UpdateStartPoint(prev);
        }

        private async Task UpdateStartPoint(Point previous)
        {
            await FillSquareInBitmap(BitmapImage.GetPixel(previous.X, previous.Y), previous);
            await FillSquareInBitmap(Color.FromArgb(0, 255, 0), StartPoint);
        }

        public async Task SetEndPoint(int x, int y)
        {
            Point prev = new Point()
            {
                X = EndPoint.X,
                Y = EndPoint.Y
            };
            EndPoint.X = x;
            EndPoint.Y = y;
            await UpdateEndPoint(prev);
        }

        private async Task UpdateEndPoint(Point previous)
        {
            await FillSquareInBitmap(BitmapImage.GetPixel(previous.X, previous.Y), previous);
            await FillSquareInBitmap(Color.FromArgb(255, 0, 0), EndPoint);
        }

        private async Task FillSquareInBitmap(Color color, Point point)
        {
            for (int i = 0; i < VisualMultiplicator; i++)
            {
                for (int j = 0; j < VisualMultiplicator; j++)
                {
                    Visualization.SetPixel(point.X * VisualMultiplicator + i, point.Y * VisualMultiplicator + j, color);
                }
            }
        }
        private async Task FindSuccessors()
        {

            /*Left check*/
            if (
                Agent.Point.X > 0
                && BitmapImage.GetPixel(Agent.Point.X - 1, Agent.Point.Y).Equals(PathColor)
                && !DiscoveredPlaces.Contains(new Point(Agent.Point.X - 1, Agent.Point.Y))
                )
            {
                Successors.Add(new State(){
	                Point = new Point() { X = Agent.Point.X - 1, Y = Agent.Point.Y },
                    Direction = Direction.West,
                    Cost = ForwardPunishment + Agent.Cost + Agent.Direction switch
                    {
                        Direction.North => TurnPunishment,
                        Direction.South => TurnPunishment,
                        Direction.East => FullTurnPunishment,
                        Direction.West => 0,
                    },
                    PreviousState = Agent
	                });
            }
            /*Right check*/
            if (
                Agent.Point.X < Width - 1
                && BitmapImage.GetPixel(Agent.Point.X + 1, Agent.Point.Y).Equals(PathColor)
                && !DiscoveredPlaces.Contains(new Point(Agent.Point.X + 1, Agent.Point.Y))
            )
			{ 
	            Successors.Add(new State()
                {
	                Point = new Point() { X = Agent.Point.X + 1, Y = Agent.Point.Y },
	                Direction = Direction.East,
					Cost = ForwardPunishment + Agent.Cost + Agent.Direction switch
					{
						Direction.North => TurnPunishment,
						Direction.South => TurnPunishment,
						Direction.East => 0,
						Direction.West => FullTurnPunishment,
					},
                    PreviousState = Agent
                });
			}
            /*Top check*/
            if (
                Agent.Point.Y > 0
                && BitmapImage.GetPixel(Agent.Point.X, Agent.Point.Y - 1).Equals(PathColor)
                && !DiscoveredPlaces.Contains(new Point(Agent.Point.X, Agent.Point.Y - 1))
            )
			{
                Successors.Add(new State()
                {
	                Point = new Point() { X = Agent.Point.X, Y = Agent.Point.Y - 1 },
	                Direction = Direction.North,
					Cost = ForwardPunishment + Agent.Cost + Agent.Direction switch
					{
						Direction.North => 0,
						Direction.South => FullTurnPunishment,
						Direction.East => TurnPunishment,
						Direction.West => TurnPunishment,
					},
                    PreviousState = Agent
                });
			}
            /*Bottom check*/
            if (
                Agent.Point.Y < Height - 1
                && BitmapImage.GetPixel(Agent.Point.X, Agent.Point.Y + 1).Equals(PathColor)
                && !DiscoveredPlaces.Contains(new Point(Agent.Point.X, Agent.Point.Y + 1))
            )
			{
                Successors.Add(new State()
                {
	                Point = new Point() { X = Agent.Point.X, Y = Agent.Point.Y + 1 },
	                Direction = Direction.South,
					Cost = ForwardPunishment + Agent.Cost + Agent.Direction switch
					{
						Direction.North => FullTurnPunishment,
						Direction.South => 0,
						Direction.East => TurnPunishment,
						Direction.West => TurnPunishment,
					},
                    PreviousState = Agent
                });
			}
        }
        

        public async Task<State> FindNextSuccessor()
        {
	        State bestSuccessor = null;
	        int minDistance = 0;
	        foreach (var successor in Successors)
	        {
		        int distance = successor.Cost + ForwardPunishment * GetManhattanDistance(Agent.Point, successor.Point);
		        if (minDistance == 0 || distance < minDistance)
		        {
			        minDistance = distance;
			        bestSuccessor = successor;
		        }
	        }

            if (bestSuccessor is not null)
            {
                DiscoveredPlaces.Add(bestSuccessor.Point);
                Successors.Remove(bestSuccessor);
            }

            return bestSuccessor;
        }
        
        public int GetManhattanDistance(Point a, Point b)
        {
	        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y); 
        }

        public class State
        {
            public Direction Direction { get; set; } = Direction.North;
            public Point Point { get; set; } = new();
            public int Cost { get; set; } = 0;

            public State PreviousState { get; set; }

        }
        public class Point
        {
            public Point() { }
            public Point(int x, int y) {
                X = x;
                Y = y;
            }
            public int X { get; set; } = 0;
            public int Y { get; set; } = 0;

            public override bool Equals(object? obj)
            {
                return obj is Point point &&
                       X == point.X &&
                       Y == point.Y;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(X, Y);
            }
        }
        public enum Direction
        {
            North,
            South,
            East,
            West
        }
    }
}
