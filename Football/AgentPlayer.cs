using System.Drawing;

namespace Football
{
    internal abstract class AgentPlayer
    {
        protected PointF intendedVelocity, intendedBallVelocity;
        protected int myID;
        protected Utils utils;

        public AgentPlayer()
        {
            intendedBallVelocity = new PointF(0, 0);
            intendedVelocity = new PointF(0, 0);
        }

        public PointF getIntendedVelocity()
        {
            return intendedVelocity;
        }

        public PointF getIntendedBallVelocity()
        {
            return intendedBallVelocity;
        }

        public void updateInformation()
        {
            utils.update();
        }

        public void setUtils(Utils u)
        {
            utils = u;
        }

        public void setID(int ID)
        {
            myID = ID;
        }

        public abstract void selectAction();

        protected void goToLocation(PointF location)
        {
            intendedVelocity = utils.computeVelocity(utils.locations[myID], location);
        }

        protected void shootToGoal()
        {
            intendedBallVelocity = utils.computeVelocity(utils.locations[myID], utils.enemyGoalCentralPoint);
        }

        protected void passBallToPlayer(int playerID)
        {
            intendedBallVelocity = utils.computeVelocity(utils.locations[myID], utils.locations[playerID]);
        }

        protected int getNearestTeammate()
        {
            return utils.getNearestPlayer(utils.locations[myID], Utils.target.myPlayers, myID);
        }

        protected PointF getPointBetween(PointF first, PointF second)
        {
            return new PointF((first.X + second.X)/2, (first.Y + second.Y)/2);
        }
    }
}