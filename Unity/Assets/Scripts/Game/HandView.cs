using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AetherFlow.Core;
using AetherFlow.Core.Enums;
using AetherFlow.Unity;

namespace AetherFlow.Unity
{
    // Procedurally creates and refreshes the card hand panel at the bottom of the screen.
    public class HandView : MonoBehaviour
    {
        private GameObject   _panel;
        private Transform    _cardRow;
        private Button       _skipBtn;
        private readonly List<GameObject> _cards = new();

        public void Build(Canvas canvas)
        {
            BuildPanel(canvas);
            GameController.Instance.OnStateChanged += Refresh;
        }

        private void BuildPanel(Canvas canvas)
        {
            // Semi-transparent dark panel anchored to the bottom.
            _panel = new GameObject("HandPanel");
            _panel.transform.SetParent(canvas.transform, false);

            var rect = _panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 0);
            rect.pivot     = new Vector2(0.5f, 0);
            rect.sizeDelta = new Vector2(0, 200);
            rect.anchoredPosition = Vector2.zero;

            var bg = _panel.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.10f, 0.88f);

            // Horizontal layout for cards.
            var rowGO = new GameObject("CardRow");
            rowGO.transform.SetParent(_panel.transform, false);
            _cardRow = rowGO.transform;

            var rowRect = rowGO.AddComponent<RectTransform>();
            rowRect.anchorMin        = new Vector2(0, 0);
            rowRect.anchorMax        = new Vector2(1, 1);
            rowRect.offsetMin        = new Vector2(10, 10);
            rowRect.offsetMax        = new Vector2(-130, -10);

            var layout = rowGO.AddComponent<HorizontalLayoutGroup>();
            layout.spacing           = 10;
            layout.childForceExpandWidth  = false;
            layout.childForceExpandHeight = false;
            layout.childAlignment    = TextAnchor.MiddleLeft;

            // Skip button on the right.
            _skipBtn = MakeButton(_panel.transform, "Skip", new Vector2(110, 180),
                new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Color(0.25f, 0.25f, 0.30f));
            _skipBtn.GetComponentInChildren<Text>().text = "Skip";
            _skipBtn.onClick.AddListener(() => GameController.Instance.PlayerSkip());
        }

        private void Refresh()
        {
            var gc = GameController.Instance;

            // Clear existing card buttons.
            foreach (var c in _cards) Destroy(c);
            _cards.Clear();

            bool show = gc.Input == InputMode.SelectCard && gc.ActiveAgent != null;
            _panel.SetActive(show || gc.Input == InputMode.SelectMoveZone);
            _skipBtn.gameObject.SetActive(show || gc.Input == InputMode.SelectMoveZone);

            if (!show) return;

            var agent = gc.ActiveAgent!;
            foreach (var card in agent.AgentDeck)
            {
                bool locked = card.IsUltimate && agent.UltimateCharge < 100;
                var  btn    = BuildCardButton(card, locked);
                _cards.Add(btn);
            }
        }

        private GameObject BuildCardButton(Card card, bool locked)
        {
            var go = new GameObject($"Card_{card.Name}");
            go.transform.SetParent(_cardRow, false);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(145, 170);

            // Background colour by effect type.
            var bg = go.AddComponent<Image>();
            bg.color = locked ? new Color(0.2f, 0.2f, 0.2f) : CardColour(card.EffectType);

            // Card name.
            AddLabel(go.transform, card.Name, 18, new Vector2(0, 0.75f), bold: true);

            // Effect type.
            AddLabel(go.transform, card.EffectType.ToString(), 14, new Vector2(0, 0.52f));

            // Power value.
            if (card.Power > 0)
                AddLabel(go.transform, $"PWR {card.Power}", 14, new Vector2(0, 0.35f));

            // Range.
            if (card.Range > 0 && card.Range < 99)
                AddLabel(go.transform, $"RNG {card.Range}", 13, new Vector2(0, 0.22f));

            // Ultimate charge indicator.
            if (card.IsUltimate)
            {
                var charge = GameController.Instance.ActiveAgent?.UltimateCharge ?? 0;
                AddLabel(go.transform, locked ? $"ULT {charge}%" : "ULT READY", 13,
                    new Vector2(0, 0.10f), locked ? Color.gray : Color.yellow);
            }

            var btn = go.AddComponent<Button>();
            btn.interactable = !locked;
            var captured = card;
            btn.onClick.AddListener(() => GameController.Instance.PlayerSelectCard(captured));

            return go;
        }

        // ── Tiny helpers ──────────────────────────────────────────────────────

        private static void AddLabel(Transform parent, string text, int fontSize,
            Vector2 anchorCentre, bool bold = false)
            => AddLabel(parent, text, fontSize, anchorCentre, Color.white, bold);

        private static void AddLabel(Transform parent, string text, int fontSize,
            Vector2 anchorCentre, Color color, bool bold = false)
        {
            var go   = new GameObject("Lbl");
            go.transform.SetParent(parent, false);

            var r = go.AddComponent<RectTransform>();
            r.anchorMin = r.anchorMax = anchorCentre;
            r.sizeDelta = new Vector2(135, 26);
            r.anchoredPosition = Vector2.zero;

            var t = go.AddComponent<Text>();
            t.text      = text;
            t.fontSize  = fontSize;
            t.color     = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            t.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static Button MakeButton(Transform parent, string label, Vector2 size,
            Vector2 anchorMin, Vector2 anchorMax, Color bgColor)
        {
            var go   = new GameObject($"Btn_{label}");
            go.transform.SetParent(parent, false);

            var r = go.AddComponent<RectTransform>();
            r.anchorMin        = anchorMin;
            r.anchorMax        = anchorMax;
            r.sizeDelta        = size;
            r.anchoredPosition = new Vector2(-10, 0);

            go.AddComponent<Image>().color = bgColor;
            var btn = go.AddComponent<Button>();

            var txt = new GameObject("Text");
            txt.transform.SetParent(go.transform, false);
            var tr = txt.AddComponent<RectTransform>();
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = tr.offsetMax = Vector2.zero;

            var t = txt.AddComponent<Text>();
            t.text      = label;
            t.fontSize  = 16;
            t.color     = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            return btn;
        }

        private static Color CardColour(EffectType e) => e switch
        {
            EffectType.Damage => new Color(0.75f, 0.20f, 0.20f),
            EffectType.Heal   => new Color(0.15f, 0.60f, 0.25f),
            EffectType.Smoke  => new Color(0.40f, 0.40f, 0.45f),
            EffectType.Stun   => new Color(0.70f, 0.60f, 0.05f),
            EffectType.Buff   => new Color(0.15f, 0.35f, 0.75f),
            _                 => new Color(0.30f, 0.30f, 0.30f),
        };
    }
}
