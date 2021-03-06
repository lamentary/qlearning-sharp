﻿namespace QLearningMaze.Core.Agent
{
    using Mazes;

    using QLearning.Core.Agent;

    public class MazeAgent : TDAgent<MazeBase>, ITDAgent<MazeBase>
    {
        public MazeAgent() { }

        public MazeAgent(
            double learningRate,
            double discountFactor,
            int numberOfTrainingEpisodes,
            int maximumAllowedMoves = 1000,
            int maximumAllowedBacktracks = -1)
            : base (
                  new MazeBase(1, 1, 0, 0, 200),
                  learningRate,
                  discountFactor,
                  numberOfTrainingEpisodes,
                  maximumAllowedMoves,
                  maximumAllowedBacktracks)
        {

        }

        private int _startPosition;

        public int StartPosition
        {
            get { return _startPosition; }
            set 
            { 
                _startPosition = value;

                if (Environment != null)
                {
                    Environment.SetInitialState(value);
                }
            }
        }

    }
}
