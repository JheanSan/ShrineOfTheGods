using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Shrine.Core;
using Shrine.Data;
using Shrine.Gameplay;
using Shrine.UI;
using UnityEngine;

namespace Shrine.Utils
{
    /// <summary>
    /// Development harness: plays a whole battle through the normal battle API and
    /// saves screenshots, so the game can be inspected without touching the desktop
    /// cursor or keyboard. Activated only by the <c>-autobattle</c> command line flag.
    /// </summary>
    public sealed class BattleRecorder : MonoBehaviour
    {
        private const int MaxActions = 220;

        private ShrineApp _app;
        private string _directory;
        private int _shot;

        public static bool Requested
        {
            get
            {
                string[] args = Environment.GetCommandLineArgs();
                foreach (string arg in args)
                    if (string.Equals(arg, "-autobattle", StringComparison.OrdinalIgnoreCase))
                        return true;
                return false;
            }
        }

        public void Begin(ShrineApp app)
        {
            _app = app;
            _directory = Path.Combine(Application.persistentDataPath, "autobattle");
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (string.Equals(args[i], "-shotdir", StringComparison.OrdinalIgnoreCase))
                    _directory = args[i + 1];
            try { Directory.CreateDirectory(_directory); }
            catch (Exception exception) { Debug.LogWarning("[AutoBattle] " + exception.Message); }
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            yield return new WaitForSecondsRealtime(1.2f);
            Capture("title");

            _app.StartBattle();
            yield return new WaitForSecondsRealtime(3f);
            Capture("battle_open");
            yield return new WaitForSecondsRealtime(0.35f);
            Debug.Log($"[AutoBattle] open hand={Describe(_app.Battle.Model.Player.Hand)} "
                + $"field={_app.Battle.Model.Player.Field.Count}");

            int actions = 0;
            int turns = 0;
            while (actions < MaxActions)
            {
                BattleScreen screen = _app.Battle;
                if (screen == null || screen.Model == null) break;
                BattleModel model = screen.Model;
                if (model.IsOver) break;

                if (!screen.IsPlayerTurn)
                {
                    yield return new WaitForSecondsRealtime(0.35f);
                    continue;
                }

                if (TryAction(screen, model))
                {
                    actions++;
                    // Catch the swing itself: trails, callouts and damage numerals live
                    // for a fraction of a second, so a late frame would miss them.
                    yield return new WaitForSecondsRealtime(0.16f);
                    Capture($"strike_{actions:00}");
                    yield return new WaitForSecondsRealtime(0.79f);
                    Capture($"action_{actions:00}");
                    yield return new WaitForSecondsRealtime(0.25f);
                    continue;
                }

                turns++;
                Capture($"turn_{turns:00}_before_end");
                screen.AutoEndTurn();
                float waited = 0f;
                while (waited < 14f && !model.IsOver && model.CurrentSide != Side.Player)
                {
                    yield return new WaitForSecondsRealtime(0.25f);
                    waited += 0.25f;
                }
                yield return new WaitForSecondsRealtime(0.6f);
                Capture($"turn_{turns:00}_after_rival");
                Debug.Log($"[AutoBattle] rival hand={model.Opponent.Hand.Count} "
                    + $"field={model.Opponent.Field.Count} mana={model.Opponent.Mana}");
            }

            yield return new WaitForSecondsRealtime(2.2f);
            Capture("battle_end");
            BattleModel final = _app.Battle != null ? _app.Battle.Model : null;
            if (final != null)
                Debug.Log($"[AutoBattle] over={final.IsOver} playerHealth={final.Player.Health} "
                    + $"rivalHealth={final.Opponent.Health} round={final.Round} actions={actions} turns={turns}");
            yield return new WaitForSecondsRealtime(0.8f);
            Application.Quit();
        }

        private bool TryAction(BattleScreen screen, BattleModel model)
        {
            Participant player = model.Player;
            Participant opponent = model.Opponent;

            CardInstance playable = null;
            foreach (CardInstance card in player.Hand)
            {
                if (!model.CanPlay(Side.Player, card.Uid)) continue;
                if (playable == null) { playable = card; continue; }
                int better = card.Data.Attack * 2 + card.CurrentDefense - card.Data.ManaCost;
                int current = playable.Data.Attack * 2 + playable.CurrentDefense - playable.Data.ManaCost;
                if (better > current) playable = card;
            }
            if (playable != null) return screen.AutoPlay(playable.Uid);

            foreach (CardInstance attacker in player.Field)
            {
                if (!model.CanAttack(Side.Player, attacker.Uid)) continue;
                CardInstance best = null;
                foreach (CardInstance target in opponent.Field)
                {
                    if (best == null) { best = target; continue; }
                    bool kills = target.CurrentDefense <= attacker.Data.Attack;
                    bool bestKills = best.CurrentDefense <= attacker.Data.Attack;
                    if (kills != bestKills) { if (kills) best = target; continue; }
                    if (target.CurrentDefense < best.CurrentDefense) best = target;
                }
                if (best != null) return screen.AutoAttack(attacker.Uid, best.Uid);
                if (opponent.Field.Count == 0) return screen.AutoAttackHero(attacker.Uid);
            }
            return false;
        }

        private static string Describe(IReadOnlyList<CardInstance> cards)
        {
            var builder = new System.Text.StringBuilder();
            foreach (CardInstance card in cards)
                builder.Append(card.Data.Id).Append('(').Append(card.Data.ManaCost).Append(") ");
            return builder.ToString().Trim();
        }

        private void Capture(string label)
        {
            _shot++;
            string path = Path.Combine(_directory, $"{_shot:00}_{label}.png");
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log("[AutoBattle] shot " + path);
        }
    }
}
