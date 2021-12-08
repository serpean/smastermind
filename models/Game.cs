using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMastermind.models
{
    class Game
    {
        public static int ROUNDS = 10;
        private readonly Result[] results;
        private int currentAttempt;
        private readonly SecretCombination secretCombination;
        private readonly ProposeCombination[] proposeCombinations;

        public Game()
        {
            results = new Result[ROUNDS];
            currentAttempt = 0;
            secretCombination = new SecretCombination();
            proposeCombinations = new ProposeCombination[ROUNDS];
        }

        public SecretCombination GetSecretCombination()
        {
            return this.secretCombination;
        }

        public bool IsLooser()
        {
            return currentAttempt >= ROUNDS && !IsWinner();
        }

        public bool IsWinner()
        {
            return currentAttempt <= ROUNDS && currentAttempt > 0 && LastResult().Match();
        }

        private Result LastResult()
        {
            return results[currentAttempt - 1];
        }

        public int GetCurrentAttempt()
        {
            return this.currentAttempt;
        }

        public Result GetResultForIndex(int index)
        {
            return this.results[index];
        }

        public ProposeCombination GetProposeCombinationForIndex(int index)
        {
            return this.proposeCombinations[index];
        }

        public void AddProposeCombination(ProposeCombination proposeCombination)
        {
            proposeCombinations[currentAttempt] = proposeCombination;
            results[currentAttempt] = new Result(this.secretCombination.GetBlacks(proposeCombination), this.secretCombination.GetWhites(proposeCombination));
            currentAttempt++;
        }
    }
}
