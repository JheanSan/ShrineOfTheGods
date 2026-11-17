using System;
using System.Collections.Generic;
using Shrine.Audio;
using Shrine.Data;
using Shrine.Gameplay;
using Shrine.UI;
using Shrine.VFX;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Shrine.Core
{
    /// <summary>Composition root. Owns services, screens and the single active battle.</summary>
    public sealed class ShrineApp : MonoBehaviour
    {
        private RectTransform _screensRoot;
        private RectTransform _fxRoot;
        private BattleModel _battle;
        private OpponentAI _ai;
        private BattleScreen _battleScreen;
        private TooltipOverlay _tooltip;
        private PauseOverlay _pause;
        private SettingsOverlay _settings;
        private GameOverOverlay _gameOver;
        private ConfirmOverlay _confirm;
        private bool _busy;

        public SettingsStore Settings { get; private set; }
        public ShrineProfile Profile { get; private set; }
        public ScreenRouter Router { get; private set; } = new ScreenRouter();
        public ShrineVfx Vfx { get; private set; }
        public AudioService Audio { get; private set; }
        public TooltipOverlay Tooltip => _tooltip;

        private void Awake()
        {
            Settings = new SettingsStore();
            Profile = ProfileStore.Load();

            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler),
                    typeof(GraphicRaycaster));
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
            }

            _screensRoot = UiKit.Stretch(canvas.transform, "Screens");
            _fxRoot = UiKit.Stretch(canvas.transform, "Fx");

            gameObject.AddComponent<Shrine.Utils.PerformanceProbe>();
            Audio = gameObject.AddComponent<AudioService>();
            Audio.Initialize(Settings);
            Vfx = _fxRoot.gameObject.AddComponent<ShrineVfx>();
            Vfx.Initialize(_fxRoot, Settings);

            CreateScreen<TitleScreen>("Title");
            CreateScreen<CovenantScreen>("Covenant");
            _battleScreen = CreateScreen<BattleScreen>("Battle");
            CreateScreen<ChronicleScreen>("Chronicle");
            _tooltip = CreateScreen<TooltipOverlay>("Tooltip");
            _pause = CreateScreen<PauseOverlay>("Pause");
            _settings = CreateScreen<SettingsOverlay>("Settings");
            _gameOver = CreateScreen<GameOverOverlay>("GameOver");
            _confirm = CreateScreen<ConfirmOverlay>("Confirm");

            Router.Register(_tooltip);
            Router.Register(_pause);
            Router.Register(_settings);
            Router.Register(_gameOver);
            Router.Register(_confirm);

            Router.ShowImmediate("Title");
        }

        private T CreateScreen<T>(string name) where T : ShrineScreen
        {
            RectTransform rect = UiKit.Stretch(_screensRoot, name + "Root");
            T screen = rect.gameObject.AddComponent<T>();
            screen.Initialize(name, this);
            Router.Register(screen);
            return screen;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_confirm.gameObject.activeSelf) _confirm.Close();
                else if (_settings.gameObject.activeSelf) _settings.Close();
                else if (_pause.gameObject.activeSelf) _pause.Close();
                else if (!Router.Back() && Router.Current != "Title") Router.Show("Title");
            }
            if (Input.GetKeyDown(KeyCode.F1)) ToggleSettings();
        }

        public void SaveProfile() => ProfileStore.Save(Profile);

        public void StartBattle()
        {
            if (_busy) return;
            if (Profile.Deck.Count < 8)
            {
                Notify("Your covenant needs eight bound cards.");
                UiAudioBus.PlayDeny();
                return;
            }
            BalanceConfig balance = ShrineDatabase.Balance;
            if (balance == null) { Notify("Shrine data is unavailable."); return; }

            PatronData playerPatron = FindPatron(Profile.Patron);
            PatronData rivalPatron = FindPatron(PatronKind.Stone);
            var catalog = new List<CardData>();
            foreach (CardDefinition definition in ShrineDatabase.Cards) catalog.Add(definition.Data);
            var deck = new List<CardData>();
            foreach (CardDefinition definition in ShrineDatabase.Cards)
                if (Profile.Deck.Contains(definition.Data.Id)) deck.Add(definition.Data);

            var random = new SeededRandom(Environment.TickCount);
            _battle = new BattleModel(balance, catalog, Omens(), deck, playerPatron, rivalPatron, random,
                Profile.Name, "The Rival Shrine");
            _ai = new OpponentAI(_battle, random);
            _battleScreen.Bind(_battle, _ai);
            _gameOver.Close();
            _pause.Close();
            Router.Show("Battle");
            _battle.Start();
        }

        private static PatronData FindPatron(PatronKind kind)
        {
            foreach (PatronDefinition definition in ShrineDatabase.Patrons)
                if (definition.Data.Kind == kind) return definition.Data;
            foreach (PatronData data in ContentFactory.Patrons)
                if (data.Kind == kind) return data;
            return ContentFactory.Patrons[0];
        }

        private static IReadOnlyList<OmenData> Omens()
        {
            var list = new List<OmenData>();
            foreach (OmenDefinition definition in ShrineDatabase.Omens) list.Add(definition.Data);
            if (list.Count == 0)
                foreach (OmenData data in ContentFactory.Omens) list.Add(data);
            return list;
        }

        public void RestartBattle()
        {
            _gameOver.Close();
            _battleScreen.Hide(true);
            StartBattle();
        }

        public void LeaveBattle()
        {
            _gameOver.Close();
            _pause.Close();
            Router.ClearStack();
            Router.Show("Title");
        }

        public void ShowGameOver(bool won)
        {
            int xp = won ? ShrineDatabase.Balance.VictoryXp : ShrineDatabase.Balance.DefeatXp;
            Profile.Xp += xp;
            Profile.Games++;
            if (won) Profile.Wins++; else Profile.Losses++;
            SaveProfile();
            _gameOver.ShowResult(won, xp, Profile.Games, Profile.Wins);
        }

        public void TogglePause()
        {
            if (_pause.gameObject.activeSelf) _pause.Close();
            else if (Router.Current == "Battle") _pause.Open();
        }

        public void ToggleSettings()
        {
            if (_settings.gameObject.activeSelf) _settings.Close();
            else _settings.Open();
        }

        public void ConfirmQuit() => _confirm.Open("LEAVE THE SHRINE?",
            "Any unfinished battle will be lost.", Application.Quit);

        public void ConfirmConcede() => _confirm.Open("CONCEDE?",
            "The rival shrine will claim this battle.", () =>
            {
                _pause.Close();
                if (_battle != null && !_battle.IsOver) ShowGameOver(false);
            });

        public void ConfirmReset() => _confirm.Open("FORGET THIS PILGRIM?",
            "Your deck, rank and record will be erased.", () =>
            {
                ProfileStore.Reset();
                Profile = ShrineProfile.CreateDefault();
                SaveProfile();
                Router.Show("Title");
            });

        private void Notify(string message)
        {
            _confirm.Open("THE SHRINE WAITS", message, null);
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
        }
    }
}
