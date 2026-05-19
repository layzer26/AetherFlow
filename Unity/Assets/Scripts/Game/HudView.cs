using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AetherFlow.Unity;

namespace AetherFlow.Unity
{
    // Builds and refreshes the top HUD (round, phase, spike timer) and the side log panel.
    public class HudView : MonoBehaviour
    {
        private Text _roundText;
        private Text _phaseText;
        private Text _spikeText;
        private Text _logText;
        private ScrollRect _logScroll;
        private readonly List<string> _logLines = new();

        public void Build(Canvas canvas)
        {
            BuildTopBar(canvas);
            BuildLogPanel(canvas);

            var gc = GameController.Instance;
            gc.OnStateChanged += Refresh;
            gc.OnLog          += AppendLog;
            gc.OnMatchOver    += winner => AppendLog($"\n★ MATCH OVER — {winner.ToUpper()} WIN ★");
        }

        // ── Top bar ──────────────────────────────────────────────────────────

        private void BuildTopBar(Canvas canvas)
        {
            var bar = MakePanel(canvas.transform, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -50), new Vector2(0, 0), new Color(0.05f, 0.05f, 0.12f, 0.92f));

            // Round counter (left).
            _roundText = MakeText(bar.transform, "Round 0",
                new Vector2(0, 0), new Vector2(0.25f, 1), 20, TextAnchor.MiddleCenter);

            // Phase label (centre).
            _phaseText = MakeText(bar.transform, "",
                new Vector2(0.25f, 0), new Vector2(0.70f, 1), 18, TextAnchor.MiddleCenter);
            _phaseText.color = new Color(0.90f, 0.85f, 0.30f);

            // Spike status (right).
            _spikeText = MakeText(bar.transform, "Spike: safe",
                new Vector2(0.70f, 0), new Vector2(1.00f, 1), 16, TextAnchor.MiddleCenter);
            _spikeText.color = new Color(0.80f, 0.80f, 0.80f);
        }

        // ── Log panel ────────────────────────────────────────────────────────

        private void BuildLogPanel(Canvas canvas)
        {
            // Narrow panel on the right side, above the hand.
            var panel = MakePanel(canvas.transform,
                new Vector2(0.72f, 0), new Vector2(1, 1),
                new Vector2(0, 200), new Vector2(0, -50),
                new Color(0.04f, 0.04f, 0.08f, 0.85f));

            var scrollGO = new GameObject("Scroll");
            scrollGO.transform.SetParent(panel.transform, false);
            var sr = scrollGO.AddComponent<RectTransform>();
            sr.anchorMin = Vector2.zero;
            sr.anchorMax = Vector2.one;
            sr.offsetMin = new Vector2(4, 4);
            sr.offsetMax = new Vector2(-4, -4);

            _logScroll = scrollGO.AddComponent<ScrollRect>();
            _logScroll.horizontal = false;

            // Viewport mask.
            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollGO.transform, false);
            var vr = viewport.AddComponent<RectTransform>();
            vr.anchorMin = Vector2.zero;
            vr.anchorMax = Vector2.one;
            vr.offsetMin = vr.offsetMax = Vector2.zero;
            viewport.AddComponent<Image>().color = Color.clear;
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            _logScroll.viewport = vr;

            // Content container.
            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            var cr = content.AddComponent<RectTransform>();
            cr.anchorMin = new Vector2(0, 0);
            cr.anchorMax = new Vector2(1, 1);
            cr.pivot     = new Vector2(0.5f, 0);
            cr.offsetMin = cr.offsetMax = Vector2.zero;

            var csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _logScroll.content = cr;

            _logText = MakeTextInParent(content.transform, "", 13, TextAnchor.LowerLeft);
            _logText.color = new Color(0.85f, 0.85f, 0.85f);
            _logText.horizontalOverflow = HorizontalWrapMode.Wrap;
        }

        // ── Refresh ──────────────────────────────────────────────────────────

        private void Refresh()
        {
            var gc = GameController.Instance;
            var s  = gc.State;

            _roundText.text = $"Round {s.TurnNumber}";

            _phaseText.text = gc.Input switch
            {
                InputMode.SelectCard       => $"▶ {gc.ActiveAgent?.Name}: Select Card",
                InputMode.SelectTargetZone => "▶ Tap a zone to target",
                InputMode.SelectMoveZone   => $"▶ {gc.ActiveAgent?.Name}: Tap to move (or Skip)",
                _                          => "AI is thinking…",
            };

            if (s.IsSpikePlanted)
            {
                _spikeText.text  = $"💣 SPIKE — {s.SpikeTimer} rnd";
                _spikeText.color = new Color(1f, 0.4f, 0.1f);
            }
            else
            {
                _spikeText.text  = "Spike: safe";
                _spikeText.color = new Color(0.7f, 0.7f, 0.7f);
            }
        }

        private void AppendLog(string line)
        {
            _logLines.Add(line);
            if (_logLines.Count > 80) _logLines.RemoveAt(0);
            _logText.text = string.Join("\n", _logLines);
            Canvas.ForceUpdateCanvases();
            _logScroll.verticalNormalizedPosition = 0f;
        }

        // ── Builder helpers ──────────────────────────────────────────────────

        private static RectTransform MakePanel(Transform parent,
            Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax, Color bg)
        {
            var go = new GameObject("Panel");
            go.transform.SetParent(parent, false);
            var r = go.AddComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.offsetMin = offsetMin;
            r.offsetMax = offsetMax;
            go.AddComponent<Image>().color = bg;
            return r;
        }

        private static Text MakeText(Transform parent, string text,
            Vector2 anchorMin, Vector2 anchorMax, int fontSize, TextAnchor align)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var r = go.AddComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.offsetMin = r.offsetMax = Vector2.zero;
            var t = go.AddComponent<Text>();
            t.text      = text;
            t.fontSize  = fontSize;
            t.color     = Color.white;
            t.alignment = align;
            t.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return t;
        }

        private static Text MakeTextInParent(Transform parent, string text, int fontSize, TextAnchor align)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var r = go.AddComponent<RectTransform>();
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = r.offsetMax = Vector2.zero;
            var t = go.AddComponent<Text>();
            t.text      = text;
            t.fontSize  = fontSize;
            t.color     = Color.white;
            t.alignment = align;
            t.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return t;
        }
    }
}
