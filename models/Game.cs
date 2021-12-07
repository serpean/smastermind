using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMastermind.models
{
    class Game
    {
        public static int ROUNDS = 10;
        private Result[] results;
        private int currentAttempt;
        private SecretCombination secretCombination;
        private ProposeCombination[] proposeCombinations;

        public Game()
        {
            results = new Result[ROUNDS];
            currentAttempt = 0;
            secretCombination = new SecretCombination();
            proposeCombinations = new ProposeCombination[ROUNDS];
        }

        public SecretCombination getSecretCombination()
        {
            return this.secretCombination;
        }

        public bool IsLooser()
        {
            return currentAttempt >= ROUNDS && !IsWinner();
        }

        public bool IsWinner()
        {
            return currentAttempt <= ROUNDS && currentAttempt > 0 && LastResult().match();
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
            results[currentAttempt] = new Result(this.secretCombination.getBlacks(proposeCombination), this.secretCombination.getWhites(proposeCombination));
            currentAttempt++;
        }
    }
}
