using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WinTimer = System.Windows.Forms.Timer;


namespace EmojiSlotsGUI
{
    public class Form1 : Form
    {
        // Config
        private readonly int StartBalance = 100;
        private readonly int MinBet = 5;
        private readonly string[] Emojis = { "🍒", "🍋", "🍇", "⭐", "💎", "🐱" };

        // State
        private int balance;
        private int bet = 10;
        private readonly Random rng = new Random();

        // UI
        private readonly Label lblBalance = new Label();
        private readonly Label reel1 = new Label();
        private readonly Label reel2 = new Label();
        private readonly Label reel3 = new Label();
        private readonly Button btnSpin = new Button();
        private readonly Button btnBetDown = new Button();
        private readonly Button btnBetUp = new Button();
        private readonly Label lblBet = new Label();
        private readonly Label lblMsg = new Label();
        private readonly WinTimer spinTimer = new WinTimer();


        // Spin animation counters
        private int ticks;
        private int maxTicks;

        public Form1()
        {
            Text = "Emoji Slot Machine (C#)";
            ClientSize = new Size(420, 300); // a bit taller window
            StartPosition = FormStartPosition.CenterScreen;

            int hudOffset = 20; // extra vertical space for the HUD

            // Balance label
            lblBalance.SetBounds(15, 10, 380, 45); // increase height from 30 → 45
            lblBalance.Font = new Font("Segoe UI Semibold", 11f);
            Controls.Add(lblBalance);

            // Reels
            var reelFont = new Font("Segoe UI Emoji", 48f);
            foreach (var r in new[] { reel1, reel2, reel3 })
            {
                r.TextAlign = ContentAlignment.MiddleCenter;
                r.Font = reelFont;
            }
            reel1.SetBounds(20, 50 + hudOffset, 110, 80);
            reel2.SetBounds(155, 50 + hudOffset, 110, 80);
            reel3.SetBounds(290, 50 + hudOffset, 110, 80);
            Controls.Add(reel1); Controls.Add(reel2); Controls.Add(reel3);

            // Bet controls
            lblBet.SetBounds(15, 145 + hudOffset, 150, 30);
            lblBet.Font = new Font("Segoe UI", 10f);
            Controls.Add(lblBet);

            btnBetDown.Text = "−";
            btnBetDown.SetBounds(200, 145 + hudOffset, 35, 25);
            btnBetDown.Click += (_, __) => { bet = Math.Max(MinBet, bet - MinBet); UpdateHUD(); };
            Controls.Add(btnBetDown);

            btnBetUp.Text = "+";
            btnBetUp.SetBounds(240, 145 + hudOffset, 35, 25);
            btnBetUp.Click += (_, __) => { bet = Math.Min(balance, bet + MinBet); UpdateHUD(); };
            Controls.Add(btnBetUp);

            // Spin button
            btnSpin.Text = "Spin";
            btnSpin.SetBounds(300, 140 + hudOffset, 100, 35);
            btnSpin.Font = new Font("Segoe UI Semibold", 11f);
            btnSpin.Click += (_, __) => StartSpin();
            Controls.Add(btnSpin);

            // Message label
            lblMsg.SetBounds(15, 185 + hudOffset, 385, 60);
            lblMsg.Font = new Font("Segoe UI", 10f);
            Controls.Add(lblMsg);

            // Timer
            spinTimer.Interval = 60; // ms
            spinTimer.Tick += (_, __) => SpinTick();

            // Init
            balance = StartBalance;
            RandomizeReels();
            UpdateHUD();
        }

        private void UpdateHUD()
        {
            lblBalance.Text = $"Balance: {balance}\nPayouts: 3-kind x10, 2-kind x3, any 💎 x2, 3x💎 x20";
            lblBet.Text = $"Bet: {bet}   (min {MinBet})";
            btnSpin.Enabled = (balance >= MinBet && bet >= MinBet && bet <= balance);
        }

        private void RandomizeReels()
        {
            reel1.Text = Emojis[rng.Next(Emojis.Length)];
            reel2.Text = Emojis[rng.Next(Emojis.Length)];
            reel3.Text = Emojis[rng.Next(Emojis.Length)];
        }

        private void StartSpin()
        {
            if (bet < MinBet || bet > balance)
            {
                lblMsg.Text = $"Bet must be between {MinBet} and {balance}.";
                return;
            }

            balance -= bet;
            UpdateHUD();
            lblMsg.Text = "Spinning...";

            ticks = 0;
            maxTicks = rng.Next(18, 28);

            btnSpin.Enabled = false;
            btnBetDown.Enabled = false;
            btnBetUp.Enabled = false;

            spinTimer.Start();
        }

        private void SpinTick()
        {
            RandomizeReels();
            ticks++;

            if (ticks >= maxTicks)
            {
                spinTimer.Stop();
                var spin = new[] { reel1.Text, reel2.Text, reel3.Text };
                int payout = CalculatePayout(spin, bet);

                if (payout > 0)
                {
                    balance += payout;
                    lblMsg.Text = $"Win! +{payout} credits   Result: {spin[0]} {spin[1]} {spin[2]}";
                }
                else
                {
                    lblMsg.Text = $"No win. Result: {spin[0]} {spin[1]} {spin[2]}";
                }

                btnBetDown.Enabled = true;
                btnBetUp.Enabled = true;
                UpdateHUD();
                btnSpin.Enabled = (balance >= MinBet);
                if (balance < MinBet) lblMsg.Text += "   (Not enough balance to continue.)";
            }
        }

        private int CalculatePayout(string[] spin, int stake)
        {
            var counts = new Dictionary<string, int>();
            foreach (var e in spin)
            {
                if (!counts.ContainsKey(e)) counts[e] = 0;
                counts[e]++;
            }

            bool hasDiamond = counts.ContainsKey("💎");
            bool threeOfKind = false;
            bool twoOfKind = false;
            string threeSym = "";

            foreach (var kv in counts)
            {
                if (kv.Value == 3) { threeOfKind = true; threeSym = kv.Key; }
                if (kv.Value == 2) twoOfKind = true;
            }

            if (threeOfKind && threeSym == "💎") return stake * 20;
            if (threeOfKind) return stake * 10;
            if (twoOfKind) return stake * 3;
            if (hasDiamond) return stake * 2;
            return 0;
        }
    }
}
