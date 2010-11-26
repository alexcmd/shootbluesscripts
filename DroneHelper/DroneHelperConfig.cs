﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ShootBlues;
using Squared.Util.Bind;
using System.IO;
using Squared.Task;

namespace ShootBlues.Script {
    public partial class DroneHelperConfig : TaskUserControl, IConfigurationPanel {
        IBoundMember[] Prefs;
        DroneHelper Script;

        public DroneHelperConfig (DroneHelper script)
            : base (Program.Scheduler) {
            InitializeComponent();
            Script = script;

            Prefs = new IBoundMember[] {
                BoundMember.New(() => WhenIdle.Checked),
                BoundMember.New(() => WhenTargetLost.Checked),
                BoundMember.New(() => RecallIfShieldsBelow.Checked),
                BoundMember.New(() => RecallShieldThreshold.Value),
                BoundMember.New(() => RedeployWhenShieldsAbove.Checked),
                BoundMember.New(() => RedeployShieldThreshold.Value),
            };
        }

        private void RecallIfShieldsBelow_CheckedChanged (object sender, EventArgs e) {
            RecallShieldThreshold.Enabled = RecallIfShieldsBelow.Checked;
            ValuesChanged(sender, e);
        }

        public string GetMemberName (IBoundMember member) {
            return ((Control)member.Target).Name;
        }

        public IEnumerator<object> LoadConfiguration () {
            var rtc = new RunToCompletion<Dictionary<string, object>>(Script.GetPreferences());
            yield return rtc;

            var dict = rtc.Result;
            object value;

            foreach (var bm in Prefs)
                if (dict.TryGetValue(GetMemberName(bm), out value))
                    bm.Value = value;
        }

        public IEnumerator<object> SaveConfiguration () {
            using (var xact = Program.Database.CreateTransaction()) {
                yield return xact;

                foreach (var bm in Prefs)
                    yield return Script.SetPreference(GetMemberName(bm), bm.Value);

                yield return xact.Commit();
            }
        }

        private void ValuesChanged (object sender, EventArgs args) {
            Start(SaveConfiguration());
        }

        private void ConfigurePriorities_Click (object sender, EventArgs e) {
            Start(Program.ShowStatusWindow("Enemy Prioritizer"));
        }

        private void RedeployWhenShieldsAbove_CheckedChanged (object sender, EventArgs e) {
            RedeployShieldThreshold.Enabled = RedeployWhenShieldsAbove.Checked;
            ValuesChanged(sender, e);
        }
    }
}
