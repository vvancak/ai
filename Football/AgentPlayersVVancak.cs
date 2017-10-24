using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;

namespace Football
{
    internal enum GameState
    {
        Attack,
        Defense
    }

    internal static class Storage
    {
        public static int? ReceivingPlayer { get; set; }

        public static int? ReceivedFrom { get; set; }

        public static Dictionary<int, int> DefendingPlayers = new Dictionary<int, int>();

        public static List<int> FreePlayers;

        private static GameState _lastState;

        public static GameState GetGameState(Utils utils)
        {
            var ballState = utils.getBallState();
            if (ballState == Utils.BallState.enemyHasBall) return _lastState = GameState.Defense;
            if (ballState == Utils.BallState.weHaveBall) return _lastState = GameState.Attack;
            return _lastState;
        }
    }

    internal class AgentPlayerGeneral : AgentPlayerWithUtils
    {
        private void Shoot()
        {
            // Ball at the border
            if (IsTooCloseToBorder(utils.ballLocation)) PassBallToPlayer(GetBestTeammateForPass());

            // Not pass from teammate
            if (Storage.ReceivingPlayer == null)
            {
                var bestShot = GetBestShotTeammate();
                if (bestShot == myID) ShootToGoal();
                else PassBallToPlayer(bestShot);
            }

            // Received (was best shot)
            else if (Storage.ReceivingPlayer == myID) ShootToGoal();

            // Pass the ball to the best shot player
            else PassBallToPlayer(GetBestTeammateForPass());
        }

        private void Goalie()
        {
            const double idealGoalDefenseQuotient = 0.3;

            FreeMyDefendingPlayer();

            // Prevent going too far
            if (!IsInOurDefense(utils.ballLocation))
            {
                goToLocation(utils.myPenaltyPoint);
                return;
            }

            // Go between ball and goal (but away from the penalty square)
            var quotient = idealGoalDefenseQuotient;
            while (quotient <= 1)
            {
                var position = GetPointBetween(utils.ourGoalCentralPoint, utils.ballLocation, quotient);

                // Defend
                if (!IsInsidePenalties(position))
                {
                    goToLocation(position);
                    return;
                }
                quotient += 0.1;
            }
        }

        private void BallSeeker()
        {
            FreeMyDefendingPlayer();
            goToLocation(utils.ballLocation);
        }

        public override void selectAction()
        {
            // Shoot
            if (IsBallMine())
            {
                Shoot();
                return;
            }

            // Accept ball
            if (Storage.GetGameState(utils) == GameState.Attack && Storage.ReceivingPlayer == myID)
            {
                goToLocation(utils.ballLocation);
                return;
            }

            // Goalie  
            if (utils.getNearestPlayer(utils.myPenaltyPoint, Utils.target.myPlayers) == myID)
            {
                Goalie();
                return;
            }

            // Seeker
            if (utils.getNearestPlayer(utils.ballLocation, Utils.target.myPlayers) == myID)
            {
                BallSeeker();
                return;
            }

            Storage.ReceivingPlayer = null;
            Storage.ReceivedFrom = null;

            goToLocation(GetMyOptimalPosition());
        }
    }

    abstract class AgentPlayerWithUtils : AgentPlayer
    {
        // Playground for orientation
        private readonly Playground _playground = new Playground();

        // Orientation
        protected bool IsBallMine()
        {
            // Check if I am the last ball holder
            if (myID == utils.getBallHoldersID())
            {
                // Check if the last ball holder is still closest to the ball
                var closestToBall = utils.getNearestPlayer(utils.ballLocation, Utils.target.myPlayers);
                return closestToBall == myID;
            }
            return false;
        }

        protected bool IsTooCloseToBorder(PointF point)
        {
            const double tooCloseToBoarderQuotient = 0.05;

            var margin = tooCloseToBoarderQuotient*GoalToGoalDistance();
            if (point.X < margin) return true;
            if (point.X > _playground.sizeX - margin) return true;

            return false;
        }

        protected bool IsInsidePenalties(PointF point)
        {
            const int margin = 20;

            // First goal
            var x1 = _playground.firstPenalty.X - margin;
            var x2 = x1 + _playground.firstPenalty.Width + 2*margin;

            var y1 = _playground.firstPenalty.Y - margin;
            var y2 = y1 + _playground.firstPenalty.Height + 2*margin;

            if (point.X >= x1 && point.X <= x2 &&
                point.Y >= y1 && point.Y <= y2) return true;

            // Second goal
            x1 = _playground.secondPenalty.X - margin;
            x2 = x1 + _playground.secondPenalty.Width + 2*margin;

            y1 = _playground.secondPenalty.Y - margin;
            y2 = y1 + _playground.secondPenalty.Height + 2*margin;

            if (point.X >= x1 && point.X <= x2
                && point.Y >= y1 && point.Y <= y2) return true;

            return false;
        }

        protected bool IsInOurDefense(PointF position)
        {
            const double defenseLine = 0.4;

            return GetDistance(position, utils.ourGoalCentralPoint) <= (defenseLine*GoalToGoalDistance());
        }

        protected bool IsInPlayground(PointF position)
        {
            return position.X > 0
                   && position.X < _playground.sizeX
                   && position.Y > 0
                   && position.Y < _playground.sizeY;
        }

        protected PointF GetMyOptimalPosition()
        {
            return utils.locations[GetMyDefendingPlayer()];
        }

        protected PointF GetPointBetween(PointF beginning, PointF end, double quotient)
        {
            var newX = (1 - quotient)*beginning.X + quotient*end.X;
            var newY = (1 - quotient)*beginning.Y + quotient*end.Y;
            return new PointF((int) newX, (int) newY);
        }

        // Add storage operations to base player methods
        protected void PassBallToPlayer(int player)
        {
            Storage.ReceivingPlayer = player;
            Storage.ReceivedFrom = myID;

            passBallToPlayer(player);
        }

        protected void ShootToGoal()
        {
            Storage.ReceivingPlayer = null;
            Storage.ReceivedFrom = null;
            shootToGoal();
        }

        // Teammates
        protected int GetBestShotTeammate()
        {
            return utils.myPlayersIDs
                .Where(p => !IsInOurDefense(utils.locations[p]) && p != Storage.ReceivedFrom)
                .Select(player => {
                    var lineToGoal = GetLineRating(utils.locations[player], utils.enemyGoalCentralPoint,
                        utils.enemyGoalCentralPoint);
                    var lineToPlayer = GetLineRating(utils.locations[myID], utils.locations[player],
                        utils.locations[myID]);
                    return new KeyValuePair<int, int>(player, lineToGoal + (lineToPlayer/2));
                })
                .OrderByDescending(kvp => kvp.Value)
                .FirstOrDefault().Key;
        }

        protected int GetBestTeammateForPass()
        {
            return utils.myPlayersIDs
                .Where(p => p != myID)
                .Select(player => {
                    var lineToPlayer = GetLineRating(utils.locations[myID], utils.locations[player],
                        utils.enemyGoalCentralPoint);
                    return new KeyValuePair<int, int>(player, lineToPlayer);
                })
                .OrderByDescending(kvp => kvp.Value)
                .FirstOrDefault().Key;
        }

        // Opposite
        protected void FreeMyDefendingPlayer()
        {
            if (!Storage.DefendingPlayers.ContainsKey(myID)) return;

            var myPlayer = Storage.DefendingPlayers[myID];
            Storage.FreePlayers.Add(myPlayer);
            Storage.DefendingPlayers.Remove(myID);
        }

        protected int GetMyDefendingPlayer()
        {
            int myPlayer;
            if (Storage.DefendingPlayers.TryGetValue(myID, out myPlayer))
            {
                // Outside the defense => full chance of player swap
                if (!IsInOurDefense(utils.locations[myPlayer]))
                {
                    SwapMyDefendingPlayer();
                }
            }
            else
            {
                // Push all players to the queue at the beginning
                if (Storage.FreePlayers == null)
                {
                    Storage.FreePlayers = new List<int>(5);
                    foreach (var player in utils.opositePlayersIDs)
                    {
                        Storage.FreePlayers.Add(player);
                    }
                }

                myPlayer = GetMostDangerousEnemy();
                Storage.DefendingPlayers.Add(myID, myPlayer);
            }
            return myPlayer;
        }

        // == PRIVATE HELPERS ==
        private int GetDistance(PointF beginning, PointF end)
        {
            return (int) Math.Sqrt(utils.getDistanceSquared(beginning, end));
        }

        private int GoalToGoalDistance()
        {
            return GetDistance(utils.ourGoalCentralPoint, utils.enemyGoalCentralPoint);
        }

        private int GetLineRating(PointF start, PointF end, PointF target)
        {
            // Check if there is an enemy on the line
            Func<double, int> blockingEnemyDistance = quotient => {
                var point = GetPointBetween(start, end, quotient);
                var nearestOppositePlayer = utils.getNearestPlayer(point, Utils.target.opositePlayers);
                return GetDistance(point, utils.locations[nearestOppositePlayer]);
            };

            // Check the line length and how good the end point is
            int nearestEnemy = int.MaxValue;

            // Get enemy closest to the line points
            foreach (var quotient in new[] {0.2, 0.4, 0.5, 0.6, 0.8})
            {
                var bed = blockingEnemyDistance(quotient);
                if (bed < nearestEnemy) nearestEnemy = bed;
            }
            var distanceQuotient = (GetDistance(end, target) + GetDistance(start, end))/GoalToGoalDistance();
            return (int) nearestEnemy/(1 + distanceQuotient);
        }

        private int GetMostDangerousEnemy()
        {
            var player = Storage.FreePlayers
                .OrderBy(p => GetDistance(utils.locations[p], utils.ourGoalCentralPoint))
                .First();

            Storage.FreePlayers
                .Remove(player);

            return player;
        }

        private void SwapMyDefendingPlayer()
        {
            var myPlayer = Storage.DefendingPlayers[myID];
            Storage.FreePlayers.Add(myPlayer);
            myPlayer = GetMostDangerousEnemy();
            Storage.DefendingPlayers[myID] = myPlayer;
        }
    }
}