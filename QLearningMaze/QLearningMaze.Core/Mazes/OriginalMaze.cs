﻿namespace QLearningMaze.Core.Mazes
{
    class OriginalMaze : MazeBase, IMaze
    {
        public OriginalMaze(
            int rows,
            int columns,
            int startPosition,
            int goalPosition,
            double discountRate,
            double learningRate,
            int maxEpisodes) 
        : base (
            rows,
            columns,
            startPosition,
            goalPosition,
            discountRate,
            learningRate,
            maxEpisodes)
        {

        }

        protected override void CreateObservationSpace()
        {
            base.CreateObservationSpace();
            AddWall(0, 1);
            AddWall(1, 2);
            AddWall(4, 5);
            AddWall(6, 10);
            AddWall(10, 11);
        }
    }
}