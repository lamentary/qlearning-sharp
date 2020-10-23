﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace QLearningTutorial
{
    public abstract class MazeBase : IMaze
    {
        private Random _random = new Random();
        protected bool _mazeInitialized = false;
        protected bool _rewardsInitialized = false;
        
        public MazeBase(
            int rows,
            int columns,
            int startPosition,
            int goalPosition,
            double gamma,
            double learnRate,
            int maxEpochs)
        {
            this.Rows = rows;
            this.Columns = columns;
            this.StartPosition = startPosition;
            this.GoalPosition = goalPosition;
            this.DiscountRate = gamma;
            this.LearningRate = learnRate;
            this.MaxEpochs = maxEpochs;
        }

        public int NumberOfStates 
        { 
            get
            {
                return Rows * Columns;
            }
        }
        public int Rows { get; set; } = 3;
        public int Columns { get; set; } = 4;
        public int GoalPosition { get; set; }
        public int StartPosition { get; set; }
        /// <summary>
        /// Decimal value between 0 and 1 that determines how much long term reward is weighted vs immediate.  Higher values reflect more regard to long term rewards
        /// </summary>
        public double DiscountRate { get; set; }
        /// <summary>
        /// Determines to what extent newly acquired information overrides old information. A factor of 0 makes the agent learn nothing (exclusively exploiting prior knowledge),
        /// while a factor of 1 makes the agent consider only the most recent information (ignoring prior knowledge to explore possibilities).
        /// </summary>
        public double LearningRate { get; set; }
        public int MaxEpochs { get; set; }

        public int[][] MazeStates { get; set; }
        public double[][] Rewards { get; set; }
        public double[][] Quality { get; set; }

        /// <summary>
        /// Creates the maze matrix.  Anything with a value of 1 indicates the ability of free movement between 2 spaces (needs to be assigned bi-directionally).  A value of 0 (zero)
        /// indicates a blocked path (also bi-directional)
        /// </summary>
        protected virtual void CreateMazeStates()
        {
            if (_mazeInitialized)
                return;

            Console.WriteLine("Creating Maze States (Observation Space)");

            int[][] mazeNextStates = new int[NumberOfStates][];

            for (int i = 0; i < NumberOfStates; ++i)
            {
                mazeNextStates[i] = new int[NumberOfStates];

                for (int j = 0; j < NumberOfStates; ++j)
                {
                    if (i != GoalPosition && // Don't map a way out of the goal position, we're already there
                        ((i + 1 == j && j % Columns != 0) || // i and j are sequential, and j is not on the next row
                        (i - 1 == j && i % Columns != 0) || // i and j are sequential, and i is not on the previous row
                        i + Columns == j || // j is directly below i
                        i - Columns == j)) // j is directly above i)
                    {
                        mazeNextStates[i][j] = 1;
                    }
                    else if (i == GoalPosition && j == GoalPosition)
                    {
                        mazeNextStates[i][j] = 1;
                    }
                }
            }
            
            MazeStates = mazeNextStates;
            _mazeInitialized = true;
        }

        protected virtual void CreateRewards()
        {
            if (_rewardsInitialized)
                return;

            Console.WriteLine("Creating Reward States");
            double[][] reward = new double[NumberOfStates][];

            for (int i = 0; i < NumberOfStates; ++i)
            {
                reward[i] = new double[NumberOfStates];

                for (int j = 0; j < NumberOfStates; ++j)
                {
                    if (MazeStates[i][j] == 1)
                    {
                        if (j == GoalPosition)
                        {
                            reward[i][j] = 10.0;
                        }
                        else
                        {
                            reward[i][j] = -0.1;
                        }
                    }
                }
            }

            Rewards = reward;
            _rewardsInitialized = true;
        }

        /// <summary>
        /// Creates the Q-Table matrix
        /// </summary>
        protected virtual void CreateQuality()
        {
            Console.WriteLine("Creating Quality of States");

            double[][] quality = new double[NumberOfStates][];

            for (int i = 0; i < NumberOfStates; ++i)
            {
                quality[i] = new double[NumberOfStates];
            }
            
            Quality = quality;
        }

        public virtual void AddWall(int betweenSpace, int andSpace)
        {
            if (!_mazeInitialized)
                CreateMazeStates();

            if (!_rewardsInitialized)
                CreateRewards();

            MazeStates[betweenSpace][andSpace] = 0;
            MazeStates[andSpace][betweenSpace] = 0;
            Rewards[betweenSpace][andSpace] = 0;
            Rewards[andSpace][betweenSpace] = 0;
        }

        public virtual void RemoveWall(int betweenSpace, int andSpace)
        {
            MazeStates[betweenSpace][andSpace] = 1;
            MazeStates[andSpace][betweenSpace] = 1;
            Rewards[betweenSpace][andSpace] = -0.1;
            Rewards[andSpace][betweenSpace] = -0.1;
        }

        protected virtual List<int> GetPossibleNextStates(int currentState, int[][] mazeNextStates)
        {
            List<int> result = new List<int>();

            for (int j = 0; j < mazeNextStates.Length; ++j)
            {
                if (mazeNextStates[currentState][j] == 1)
                {
                    result.Add(j);
                }
            }

            return result;
        }

        protected virtual int GetRandomNextState(int currentState, int[][] mazeNextStates)
        {
            List<int> possibleNextStates = GetPossibleNextStates(currentState, mazeNextStates);

            int count = possibleNextStates.Count;
            int index = _random.Next(0, count);

            return possibleNextStates[index];
        }

        public virtual void Train()
        {
            CreateMazeStates();
            CreateRewards();
            CreateQuality();

            for (int epoch = 0; epoch < MaxEpochs; ++epoch)
            {
                int currState = _random.Next(0, Rewards.Length);

                while (true)
                {
                    int nextState = GetRandomNextState(currState, MazeStates);
                    List<int> possNextNextStates = GetPossibleNextStates(nextState, MazeStates);
                    double maxQ = double.MinValue;

                    for (int j = 0; j < possNextNextStates.Count; ++j)
                    {
                        int nnumberOfStates = possNextNextStates[j];  // short alias
                        double quality = Quality[nextState][nnumberOfStates];
                        if (quality > maxQ) maxQ = quality;
                    }

                    Quality[currState][nextState] = ((1 - LearningRate) * Quality[currState][nextState]) + (LearningRate * (Rewards[currState][nextState] + (DiscountRate * maxQ)));
                    currState = nextState;

                    if (currState == GoalPosition) break;
                }
            }
        }

        public virtual void RunMaze()
        {
            int curr = StartPosition; int next;

            Console.Write(curr + "->");

            while (curr != GoalPosition)
            {
                next = ArgMax(Quality[curr]);
                Console.Write(next + "->");
                curr = next;
            }

            Console.WriteLine("done");
        }

        protected virtual int ArgMax(double[] vector)
        {
            double maxVal = vector[0]; int idx = 0;

            for (int i = 0; i < vector.Length; ++i)
            {
                if (vector[i] > maxVal)
                {
                    maxVal = vector[i]; idx = i;
                }
            }

            return idx;
        }

        public virtual void PrintQuality()
        {
            int ns = Quality.Length;
            Console.WriteLine($"[0] [1] . . [{NumberOfStates - 1}]");
            for (int i = 0; i < ns; ++i)
            {
                for (int j = 0; j < ns; ++j)
                {
                    Console.Write(Quality[i][j].ToString("F2") + " ");
                }
                Console.WriteLine();
            }
        }
    }
}
