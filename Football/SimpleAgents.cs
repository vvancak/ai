using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Football
{
    class AgentSimpleDefender : AgentPlayer
    {
        public override void selectAction()
        {
            goToLocation(getPointBetween(utils.ballLocation, utils.ourGoalCentralPoint));
            passBallToPlayer(getNearestTeammate());
        }
    }

    class AgentSimpleBallSeeker : AgentPlayer
    {
        public override void selectAction()
        {
            goToLocation(utils.ballLocation);
            passBallToPlayer(getNearestTeammate());
        }
    }

    class AgentSimpleOffender : AgentPlayer
    {
        public override void selectAction()
        {
            goToLocation(utils.enemyPenaltyPoint);
            shootToGoal();
        }
    }
}