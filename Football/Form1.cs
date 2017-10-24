using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Football
{
    public partial class Form1 : Form
    {
        Simulator s;

        public Form1()
        {
            InitializeComponent();

            //---- TADY JSOU CLENOVE TYMU ----
            List<AgentPlayer> agentsInTheTeam = new List<AgentPlayer>()
            {
                new AgentPlayerGeneral(),
                new AgentPlayerGeneral(),
                new AgentPlayerGeneral(),
                new AgentPlayerGeneral(),
                new AgentPlayerGeneral()
            };

            // ---- TADY NASTAVTE JMENO VASEHO TYMU ----
            TeamAI agentTeam = new AgentTeamAI("FC Sátoraljaújhely", agentsInTheTeam);

            // ---- ZADNE DALSI VECI NENI POTREBA TADY MENIT ----

            s = new Simulator(pictureBox1, new Playground(), FirstTeamName_label, SecondTeamName_label, score_label);
            TeamAI firstTeam = agentTeam,
            //TeamAI firstTeam = new DistributedAI("Barcelona FC"),
            //    secondTeam = new AgentTeamAI("FC Sátoraljaújhely", agentsInTheTeam);

            secondTeam = new DistributedAI("Arsenal London");
            //secondTeam = new AIZoneBasedTeam("TJ Sokol Nekopnemsi");
            //secondTeam = new AIShooterTeam("TJ Sokol Nekopnemsi B");
            //secondTeam = agentTeam;

            firstTeam.setColor(Color.DarkRed);
            secondTeam.setColor(Color.Cyan);

            s.setTeams(firstTeam, secondTeam);
            s.start();
            //s.test();
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            s.goOneStep();
            s.draw();
            if (s.score.X >= 50 || s.score.Y >= 50)
                timer1.Enabled = false;
        }
    }
}