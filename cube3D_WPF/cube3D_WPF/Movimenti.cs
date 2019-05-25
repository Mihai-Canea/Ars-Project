using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace cube3D_WPF
{
    public enum Move
    {
        F, R, B, L, U, D,
        M, //In mezzo tra L & R
        E, //In mezzo tra U & D
        S, //In mezzo tra F & B
        None,
    }

    public enum Scorrimenti
    {
        None,
        Hor, //horizontal
        Ver, //vertical
    }

    // copiato
    public enum RotationDirection
    {
        CounterClockWise = -1,
        None,
        ClockWise,
    }

    public struct SwipedFace
    {
        public Faccie face;
        public Scorrimenti direction;
        public int layer;

        public SwipedFace(Faccie f, Scorrimenti direction, int layer)
        {
            this.face = f;
            this.direction = direction;
            this.layer = layer;
        }
    }

    class Movimenti
    {
        private HashSet<string> touchedFaces;
        public HashSet<string> TouchedFaces
        {
            get
            {
                return this.touchedFaces;
            }

            set
            {
                this.touchedFaces = value;
                swipedFaces.Clear();
                parse();
            }
        }

        public List<SwipedFace> swipedFaces = new List<SwipedFace>();

        public Movimenti() { }

        public Movimenti(HashSet<string> touchedFaces)
        {
            this.touchedFaces = touchedFaces;
        }

        private void parse()
        {
            foreach (string tf in touchedFaces)
            {
                Faccie face = (Faccie)Enum.Parse(typeof(Faccie), tf[0].ToString());
                Scorrimenti dir = (Scorrimenti)Enum.Parse(typeof(Scorrimenti), tf[1].ToString());
                int layer = Convert.ToInt32(tf[2].ToString());

                swipedFaces.Add(new SwipedFace(face, dir, layer));
            }
        }

        public KeyValuePair<Move, RotationDirection> getMove()
        {
            KeyValuePair<Move, RotationDirection> retval = new KeyValuePair<Move, RotationDirection>(Move.None, RotationDirection.None);

            if (swipedFaces.Count < 3)
            {
                return retval;
            }

            Faccie f = getDominantFace();

            if (f == Faccie.None)
            {
                return retval;
            }

            filterMoves(f);
            Scorrimenti dir = getSingleDirection();

            if (dir == Scorrimenti.None)
            {
                return retval;
            }

            SwipedFace swipedFace = getSingleSwipedFace(dir);

            //Debug.Print("face: {0}{1}{2}", swipedFace.face, swipedFace.direction, swipedFace.layer);

            Move m = Move.None;

            switch (swipedFace.face)
            {
                case Faccie.F:
                case Faccie.B:
                    switch (swipedFace.direction)
                    {
                        case Scorrimenti.Hor:
                            switch (swipedFace.layer)
                            {
                                case 0:
                                    m = Move.U;
                                    break;
                                case 1:
                                    m = Move.E;
                                    break;
                                case 2:
                                    m = Move.D;
                                    break;
                            }
                            break;
                        case Scorrimenti.Ver:
                            switch (swipedFace.layer)
                            {
                                case 0:
                                    m = Move.L;
                                    break;
                                case 1:
                                    m = Move.M;
                                    break;
                                case 2:
                                    m = Move.R;
                                    break;
                            }
                            break;
                    }
                    break;
                case Faccie.R:
                case Faccie.L:
                    switch (swipedFace.direction)
                    {
                        case Scorrimenti.Hor:
                            switch (swipedFace.layer)
                            {
                                case 0:
                                    m = Move.D;
                                    break;
                                case 1:
                                    m = Move.E;
                                    break;
                                case 2:
                                    m = Move.U;
                                    break;
                            }
                            break;
                        case Scorrimenti.Ver:
                            switch (swipedFace.layer)
                            {
                                case 0:
                                    m = Move.B;
                                    break;
                                case 1:
                                    m = Move.S;
                                    break;
                                case 2:
                                    m = Move.F;
                                    break;
                            }
                            break;
                    }
                    break;
                case Faccie.U:
                case Faccie.D:
                    switch (swipedFace.direction)
                    {
                        case Scorrimenti.Hor:
                            switch (swipedFace.layer)
                            {
                                case 0:
                                    m = Move.B;
                                    break;
                                case 1:
                                    m = Move.S;
                                    break;
                                case 2:
                                    m = Move.F;
                                    break;
                            }
                            break;
                        case Scorrimenti.Ver:
                            switch (swipedFace.layer)
                            {
                                case 0:
                                    m = Move.L;
                                    break;
                                case 1:
                                    m = Move.M;
                                    break;
                                case 2:
                                    m = Move.R;
                                    break;
                            }
                            break;
                    }
                    break;
            }

            retval = new KeyValuePair<Move, RotationDirection>(m, getRotationDirection(swipedFace));
            Debug.Print("Move: " + retval.ToString());

            return retval;
        }

        private SwipedFace getSingleSwipedFace(Scorrimenti dir)
        {
            return swipedFaces.Where(x => x.direction == dir).First();
        }

        private Scorrimenti getSingleDirection()
        {
            Dictionary<Scorrimenti, int> directionCount = new Dictionary<Scorrimenti, int>() {
                {Scorrimenti.Hor, 0},
                {Scorrimenti.Ver, 0},
            };

            foreach (var s in swipedFaces)
            {
                directionCount[s.direction]++;
            }

            try
            {
                return directionCount.Where(count => count.Value == 1).First().Key;
            }
            catch (InvalidOperationException)
            {
                return Scorrimenti.None;
            }
        }

        private RotationDirection getRotationDirection(SwipedFace f)
        {
            Dictionary<Faccie, Dictionary<Scorrimenti, Dictionary<int, RotationDirection>>> dirs =
                new Dictionary<Faccie, Dictionary<Scorrimenti, Dictionary<int, RotationDirection>>> {
                {Faccie.F, new Dictionary<Scorrimenti, Dictionary<int, RotationDirection>>{
                    {Scorrimenti.Ver, new Dictionary<int, RotationDirection>{
                        {0, RotationDirection.ClockWise},
                        {1, RotationDirection.ClockWise},
                        {2, RotationDirection.CounterClockWise}
                    }},
                    {Scorrimenti.Hor, new Dictionary<int, RotationDirection>{
                        {0, RotationDirection.CounterClockWise},
                        {1, RotationDirection.ClockWise},
                        {2, RotationDirection.ClockWise}
                    }}
                }},
                {Faccie.R, new Dictionary<Scorrimenti, Dictionary<int, RotationDirection>>{
                    {Scorrimenti.Ver, new Dictionary<int, RotationDirection>{
                        {0, RotationDirection.ClockWise},
                        {1, RotationDirection.CounterClockWise},
                        {2, RotationDirection.CounterClockWise}
                    }},
                    {Scorrimenti.Hor, new Dictionary<int, RotationDirection>{
                        {0, RotationDirection.CounterClockWise},
                        {1, RotationDirection.CounterClockWise},
                        {2, RotationDirection.ClockWise}
                    }}
                }},
                {Faccie.B, new Dictionary<Scorrimenti, Dictionary<int, RotationDirection>>{
                    {Scorrimenti.Ver, new Dictionary<int, RotationDirection>{
                        {0, RotationDirection.CounterClockWise},
                        {1, RotationDirection.CounterClockWise},
                        {2, RotationDirection.ClockWise}
                    }},
                    {Scorrimenti.Hor, new Dictionary<int, RotationDirection>{
                        {0, RotationDirection.ClockWise},
                        {1, RotationDirection.CounterClockWise},
                        {2, RotationDirection.CounterClockWise}
                    }}
                }},
                {Faccie.L, new Dictionary<Scorrimenti, Dictionary<int, RotationDirection>>{
                    {Scorrimenti.Ver, new Dictionary<int, RotationDirection>{
                        {0, RotationDirection.CounterClockWise},
                        {1, RotationDirection.ClockWise},
                        {2, RotationDirection.ClockWise}
                    }},
                    {Scorrimenti.Hor, new Dictionary<int, RotationDirection>{
                        {0, RotationDirection.ClockWise},
                        {1, RotationDirection.ClockWise},
                        {2, RotationDirection.CounterClockWise}
                    }}
                }},
                {Faccie.U, new Dictionary<Scorrimenti, Dictionary<int, RotationDirection>>{
                    {Scorrimenti.Ver, new Dictionary<int, RotationDirection>{
                        {0, RotationDirection.ClockWise},
                        {1, RotationDirection.ClockWise},
                        {2, RotationDirection.CounterClockWise}
                    }},
                    {Scorrimenti.Hor, new Dictionary<int, RotationDirection>{
                        {0, RotationDirection.CounterClockWise},
                        {1, RotationDirection.ClockWise},
                        {2, RotationDirection.ClockWise}
                    }}
                }},
                {Faccie.D, new Dictionary<Scorrimenti, Dictionary<int, RotationDirection>>{
                    {Scorrimenti.Ver, new Dictionary<int, RotationDirection>{
                        {0, RotationDirection.CounterClockWise},
                        {1, RotationDirection.CounterClockWise},
                        {2, RotationDirection.ClockWise}
                    }},
                    {Scorrimenti.Hor, new Dictionary<int, RotationDirection>{
                        {0, RotationDirection.ClockWise},
                        {1, RotationDirection.CounterClockWise},
                        {2, RotationDirection.CounterClockWise}
                    }}
                }},
            };


            return (RotationDirection)(Convert.ToInt32(dirs[f.face][f.direction][f.layer]) * getLayerOrder(f));
        }

        private int getLayerOrder(SwipedFace ignore)
        {
            List<SwipedFace> orderedFaces = swipedFaces;
            orderedFaces.Remove(ignore);

            for (int i = 1; i < orderedFaces.Count; i++)
            {
                if (orderedFaces[i].layer < orderedFaces[i - 1].layer)
                {
                    return -1;
                }
            }

            return 1;
        }

        public Faccie getDominantFace()
        {
            Dictionary<Faccie, int> faceCount = new Dictionary<Faccie, int>();
            int count;
            foreach (var f in swipedFaces)
            {
                count = 0;
                faceCount.TryGetValue(f.face, out count);
                faceCount[f.face] = ++count;
            }

            Faccie dominantFace = new Faccie();
            int max = Int32.MinValue;
            foreach (var i in faceCount)
            {
                if (i.Value > max)
                {
                    max = i.Value;
                    dominantFace = i.Key;
                }
            }

            foreach (var i in faceCount)
            {
                if (i.Value == max && i.Key != dominantFace)
                {
                    return Faccie.None;
                }
            }

            return dominantFace;
        }

        private void filterMoves(Faccie f)
        {
            List<SwipedFace> filteredSwipedFaces = new List<SwipedFace>();

            foreach (var i in swipedFaces)
            {
                if (i.face == f)
                {
                    filteredSwipedFaces.Add(i);
                }
            }

            this.swipedFaces = filteredSwipedFaces;
        }
    }

}
