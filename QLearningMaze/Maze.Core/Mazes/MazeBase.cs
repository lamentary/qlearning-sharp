﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace QLearningMaze.Core.Mazes
{
    public abstract partial class MazeBase : IMaze
    {
        private Random _random = new Random();
        protected int _goalValue = 50;
        protected double _movementValue = -0.5;
        protected double _start_decay = 1;
        protected double _end_decay;
        protected double _epsilon_decay_value;
        protected bool _mazeInitialized = false;
        protected int _numberOfActions = 5;
        private int _backtrackPunishment = 40;
        protected List<AdditionalReward> _additionalRewards = new List<AdditionalReward>();

        public MazeBase(
            int rows,
            int columns,
            int startPosition,
            int goalPosition,
            double discountRate,
            double learningRate,
            int maxEpochs)
        {
            this.Rows = rows;
            this.Columns = columns;
            this.StartPosition = startPosition;
            this.GoalPosition = goalPosition;
            this.DiscountRate = discountRate;
            this.LearningRate = learningRate;
            this.MaxEpochs = maxEpochs;
        }

        public int NumberOfStates 
        { 
            get
            {
                return Rows * Columns;
            }
        }
        public int Rows { get; set; } = 4;
        public int Columns { get; set; } = 4;
        public int GoalPosition { get; set; } = 0;
        public int StartPosition { get; set; } = 0;
        /// <summary>
        /// Decimal value between 0 and 1 that determines how much long term reward is weighted vs immediate.  Higher values reflect more regard to long term rewards
        /// </summary>
        public double DiscountRate { get; set; } = 0.5;
        /// <summary>
        /// Determines to what extent newly acquired information overrides old information. A factor of 0 makes the agent learn nothing (exclusively exploiting prior knowledge),
        /// while a factor of 1 makes the agent consider only the most recent information (ignoring prior knowledge to explore possibilities).
        /// </summary>
        public double LearningRate { get; set; } = 0.5;
        public int MaxEpochs { get; set; } = 1000;

        public int[][] MazeStates { get; set; }
        public double[][] Rewards { get; set; }
        public double[][] Quality { get; set; }
        public List<MazeObstruction> Obstructions { get; set; } = new List<MazeObstruction>();
        public double TotalRewards { get; set; }


        /// <summary>
        /// Creates the maze matrix.  Anything with a value of 1 indicates the ability of free movement between 2 spaces (needs to be assigned bi-directionally).  A value of 0 (zero)
        /// indicates a blocked path (also bi-directional)
        /// </summary>
        protected virtual void CreateMazeStates()
        {
            OnMazeCreatingEventHandler();

            Console.WriteLine("Creating Maze States (Observation Space)");

            int[][] mazeNextStates = new int[NumberOfStates][];

            for (int i = 0; i < NumberOfStates; ++i)
            {
                mazeNextStates[i] = new int[NumberOfStates];

                for (int j = 0; j < NumberOfStates; ++j)
                {
                    var count = Obstructions.Where(x => (x.BetweenSpace == i && x.AndSpace == j) ||
                            (x.AndSpace == i && x.BetweenSpace == j)).Count();

                    if (count != 0) continue;

                    if (i != GoalPosition &&
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

            OnMazeCreatedEventhHandler();
            
            MazeStates = mazeNextStates;
            _mazeInitialized = true;
        }

        protected virtual void CreateRewards()
        {
            Console.WriteLine("Creating Reward States");
            double[][] reward = new double[NumberOfStates][];

            for (int i = 0; i < NumberOfStates; ++i)
            {
                reward[i] = new double[NumberOfStates];

                for (int j = 0; j < NumberOfStates; ++j)
                {
                    if (MazeStates[i][j] > 0)
                    {
                        if (j == GoalPosition)
                        {
                            reward[i][j] = _goalValue;
                        }                        
                        else
                        {
                            var customReward = _additionalRewards.Where(r => r.Position == j).FirstOrDefault();

                            if (customReward != null)
                            {
                                reward[i][j] = customReward.Value;
                            }
                            else
                            {
                                reward[i][j] = _movementValue;
                            }
                        }
                    }
                }
            }

            Rewards = reward;
            PrintRewards();
            OnRewardsCreated();
        }

        public virtual void AddCustomReward(int position, double reward)
        {
            var exists = _additionalRewards.Where(x => x.Position == position).FirstOrDefault();
            
            if (exists != null)
            {
                if (exists.Value == reward)
                    return;

                exists.Value = reward;
            }
            else
            {
                _additionalRewards.Add(new AdditionalReward {  Position = position, Value = reward });
            }

            for (int i = 0; i < MazeStates.Length; ++i)
            {
                if (MazeStates[i][position] > 0)
                {
                    Rewards[i][position] = reward;
                }
            }
        }

        public virtual void RemoveCustomReward(int position)
        {
            var exists = _additionalRewards.Where(x => x.Position == position).FirstOrDefault();

            if (exists == null) return;

            _additionalRewards.Remove(exists);

            for (int i = 0; i < MazeStates.Length; ++i)
            {
                if (MazeStates[i][position] > 0)
                {
                    Rewards[i][position] = _movementValue;
                }

            }
        }

        public IEnumerable<AdditionalReward> GetAdditionalRewards()
        {
            return this._additionalRewards;
        }

        public void RemoveReward(int position)
        {

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
            OnQualityCreated();
        }

        public virtual void AddWall(int betweenSpace, int andSpace)
        {
            if (!_mazeInitialized)
                CreateMazeStates();

            MazeStates[betweenSpace][andSpace] = 0;
            MazeStates[andSpace][betweenSpace] = 0;


            var maze = GetObstructionFromList(betweenSpace, andSpace);

            if (maze == null)
            {
                var obstruction = new MazeObstruction
                {
                    BetweenSpace = betweenSpace,
                    AndSpace = andSpace
                };

                Obstructions.Add(obstruction);
                OnObstructionAdded(obstruction);
            }                

            CreateRewards();
        }

        public virtual void RemoveWall(int betweenSpace, int andSpace)
        {
            if (!_mazeInitialized)
                CreateMazeStates();

            MazeStates[betweenSpace][andSpace] = 1;
            MazeStates[andSpace][betweenSpace] = 1;
            Rewards[betweenSpace][andSpace] = _movementValue;
            Rewards[andSpace][betweenSpace] = _movementValue;

            var wall = GetObstructionFromList(betweenSpace, andSpace);

            if (wall != null)
            {
                Obstructions.Remove(wall);
                OnObstructionRemoved(wall);
            }
        }

        private MazeObstruction GetObstructionFromList(int betweenSpace, int andSpace)
        {
            return Obstructions.Where(x => (x.BetweenSpace == betweenSpace && x.AndSpace == andSpace) || (x.AndSpace == betweenSpace && x.BetweenSpace == andSpace)).FirstOrDefault();
        }

        protected virtual List<int> GetPossibleNextStates(int currentState, int[][] mazeNextStates)
        {
            List<int> result = new List<int>();

            for (int j = 0; j < mazeNextStates.Length; ++j)
            {
                if (mazeNextStates[currentState][j] > 0)
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

            if (possibleNextStates.Count > 0)
                return possibleNextStates[index];
            else
                return -1;
        }

        public virtual void Train()
        {            
            int moves;
            bool isBackTrack = false;
            CreateMazeStates();
            CreateRewards();
            CreateQuality();
            
            double epsilon = 1;
            _end_decay = (int)MaxEpochs / 2;
            _epsilon_decay_value = epsilon / (_end_decay - _start_decay);

            Console.WriteLine("Please wait while I learn the maze");
            
            OnTrainingStatusChanged(true);
            
            for (int epoch = 0; epoch < MaxEpochs; ++epoch)
            {
                moves = 0;
                TotalRewards = 0;
                Console.Write($"Runnging through epoch {(epoch + 1).ToString("#,##0")} of {MaxEpochs.ToString("#,##0")}\r");

                int currState = _random.Next(0, Rewards.Length);
                //int currState = StartPosition;
                bool done = false;

                while (!done)
                {
                    moves++;

                    int nextState;
                    nextState = GetRandomNextState(currState, MazeStates);
                    isBackTrack = false;
                    double randRand = _random.NextDouble();

                    if (randRand > epsilon)
                    {
                        int preferredNext = -1;
                        double max = 0;

                        for (int i = 0; i < this.Quality[currState].Length; ++i)
                        {
                            if (this.Quality[currState][i] > max)
                            {
                                max = this.Quality[currState][i];
                                preferredNext = i;
                            }
                        }

                        if (preferredNext >= 0)
                        {
                            nextState = preferredNext;
                        }
                    }


                    if (nextState < 0)
                    {
                        currState = _random.Next(0, Rewards.Length);
                        continue;
                    }

                    List<int> possNextNextStates = GetPossibleNextStates(nextState, MazeStates);
                    double maxQ = double.MinValue;
                    
                    for (int j = 0; j < possNextNextStates.Count; ++j)
                    {
                        int futureNextState = possNextNextStates[j];  // short alias

                        double futureQuality = Quality[nextState][futureNextState];
                        double immediateRewards = Rewards[currState][nextState];
                        double futureRewards = Rewards[nextState][futureNextState];
                        if (futureQuality > maxQ) 
                        {
                            maxQ = futureQuality;
                            Quality[nextState][currState] -= 1;
                        }

                        if (currState == futureNextState)
                            isBackTrack = true;
                    }


                    double oldtempQuality = Quality[currState][nextState];
                    
                    TotalRewards = Rewards[currState][nextState] + (isBackTrack ? (GetAdditionalRewards().Max(x => x.Value) + 5)-_backtrackPunishment : 0);

                    var qLearn = Quality[currState][nextState];
                    var temporalDiff = TotalRewards + (DiscountRate * maxQ) - Quality[currState][nextState];
                    var newQ = qLearn + (LearningRate * temporalDiff);

                    Quality[currState][nextState] = ((1 - LearningRate) * Quality[currState][nextState]) + (LearningRate * (TotalRewards + (DiscountRate * maxQ)));
                    OnTrainingAgentStateChanging(nextState, currState, oldtempQuality, Quality[currState][nextState]);
                    currState = nextState;

                    if (currState == GoalPosition ||
                        moves > 10000)
                    {
                        done = true;
                    }
                }

                if (_end_decay >= epoch &&
                    epoch >= _start_decay)
                {
                    epsilon -= _epsilon_decay_value;
                }

                OnTrainingEpochCompleted(new TrainingEpochCompletedEventArgs(epoch + 1, MaxEpochs, moves, TotalRewards, currState == GoalPosition));
            }

            OnTrainingStatusChanged(false);
            Console.WriteLine();
            Console.WriteLine("I'm done learning");
        }


        public virtual void RunMaze()
        {
            int curr = StartPosition; 
            int next; 
            int moves = 0;
            int previousPosition = -1;
            TotalRewards = 0;
            Console.Write(curr + "->");

            while (curr != GoalPosition)
            {
                if (Quality == null)
                    Train();                
                
                next = ArgMax(Quality[curr]);

                if (MazeStates[curr][next] < 1)
                {
                    string message = "I guess I didn't learn very  well, I just tried an illegal move.  Check the learning rate and discount rate and try again";
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine(message);
                    throw new InvalidOperationException(message);
                }

                TotalRewards += Rewards[curr][next];
                moves++;
                Console.Write(next + "->");
                if (next == curr ||
                    next == previousPosition)
                {
                    string message = "I'm a greedy idiot, and am backtracking to get more rewards.  Try increasing the Discount Rate to stop me from doing this.";
                    throw new InvalidOperationException(message);
                }

                previousPosition = curr;
                curr = next;
                OnAgentStateChanged(curr, TotalRewards, moves);
            }

            Console.WriteLine("done");
        }

        protected virtual int ArgMax(double[] vector)
        {
            double maxVal = double.MinValue; 
            int idx = 0;

            for (int i = 0; i < vector.Length; ++i)
            {
                if (vector[i] != 0 && vector[i] > maxVal)
                {
                    maxVal = vector[i]; 
                    idx = i;
                }
            }

            return idx;
        }

        public virtual void PrintRewards()
        {
            int ns = Rewards.Length;
            Console.WriteLine($"Rewards [0] [1] . . [{NumberOfStates - 1}]");

            for (int i = 0; i < ns; ++i)
            {
                for (int j = 0; j < ns; ++j)
                {
                    Console.Write(Rewards[i][j].ToString("F2") + " ");
                }
                Console.WriteLine();
            }

        }
        public virtual void PrintQuality()
        {
            int ns = Quality.Length;
            Console.WriteLine($"Quality [0] [1] . . [{NumberOfStates - 1}]");
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