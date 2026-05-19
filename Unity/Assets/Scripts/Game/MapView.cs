using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AetherFlow.Core;
using AetherFlow.Core.Enums;
using AetherFlow.Unity;

namespace AetherFlow.Unity
{
    // Procedurally builds and updates the visual zone graph in world space.
    // Zones are circles; connections are LineRenderers; agent tokens are smaller circles.
    public class MapView : MonoBehaviour
    {
        // World-space positions for each Vienna zone.
        private static readonly Dictionary<string, Vector2> Layout = new()
        {
            ["Attackers Spawn"] = new Vector2(-7.5f,  0.0f),
            ["A Long"]          = new Vector2(-3.5f,  3.2f),
            ["A Short"]         = new Vector2(-3.5f,  1.1f),
            ["Mid"]             = new Vector2(-2.0f,  0.0f),
            ["Mid To A"]        = new Vector2( 1.0f,  1.8f),
            ["Mid To B"]        = new Vector2( 1.0f, -1.8f),
            ["A Site"]          = new Vector2( 4.5f,  3.2f),
            ["B Short"]         = new Vector2(-3.5f, -1.1f),
            ["B Long"]          = new Vector2(-3.5f, -3.2f),
            ["B Site"]          = new Vector2( 4.5f, -3.2f),
            ["Defenders Spawn"] = new Vector2( 7.5f,  0.0f),
        };

        private readonly Dictionary<Zone, ZoneNode>   _nodes  = new();
        private readonly Dictionary<Agent, GameObject> _tokens = new();

        // ── Build ────────────────────────────────────────────────────────────

        public void Build(ZoneGraph graph, List<Agent> teamA, List<Agent> teamB)
        {
            CreateZoneNodes(graph);
            DrawEdges(graph);
            CreateAgentTokens(teamA, isPlayerTeam: true);
            CreateAgentTokens(teamB, isPlayerTeam: false);

            GameController.Instance.OnStateChanged += Refresh;
        }

        private void CreateZoneNodes(ZoneGraph graph)
        {
            foreach (var zone in graph.Zones)
            {
                if (!Layout.TryGetValue(zone.ZoneName, out var pos)) continue;

                var go = new GameObject($"Zone_{zone.ZoneName}");
                go.transform.SetParent(transform, false);
                go.transform.position = pos;

                // Clickable circle.
                var circle = CreateCircle(go, 0.9f, Color.gray);

                // Label.
                var label = CreateWorldLabel(go, zone.ZoneName, 0.22f, Vector3.up * 1.1f);

                // Click detection via 2D collider.
                var col = go.AddComponent<CircleCollider2D>();
                col.radius = 0.9f;

                var node = go.AddComponent<ZoneNode>();
                node.Init(zone, circle, label);
                _nodes[zone] = node;
            }
        }

        private void DrawEdges(ZoneGraph graph)
        {
            var drawn = new HashSet<(string, string)>();
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = new Color(0.55f, 0.55f, 0.55f);

            foreach (var zone in graph.Zones)
            {
                foreach (var neighbour in zone.Neighbours)
                {
                    var key = string.CompareOrdinal(zone.ZoneName, neighbour.ZoneName) < 0
                        ? (zone.ZoneName, neighbour.ZoneName)
                        : (neighbour.ZoneName, zone.ZoneName);

                    if (drawn.Contains(key)) continue;
                    drawn.Add(key);

                    if (!Layout.TryGetValue(zone.ZoneName, out var a)) continue;
                    if (!Layout.TryGetValue(neighbour.ZoneName, out var b)) continue;

                    var go = new GameObject($"Edge_{key}");
                    go.transform.SetParent(transform, false);

                    var lr = go.AddComponent<LineRenderer>();
                    lr.material           = mat;
                    lr.startWidth         = lr.endWidth = 0.08f;
                    lr.sortingOrder       = -1;
                    lr.useWorldSpace      = true;
                    lr.SetPositions(new Vector3[] { a, b });
                }
            }
        }

        private void CreateAgentTokens(List<Agent> agents, bool isPlayerTeam)
        {
            var teamColor = isPlayerTeam
                ? new Color(0.25f, 0.55f, 1.00f)
                : new Color(1.00f, 0.35f, 0.35f);

            foreach (var agent in agents)
            {
                var go = new GameObject($"Token_{agent.Name}");
                go.transform.SetParent(transform, false);
                CreateCircle(go, 0.32f, teamColor);
                CreateWorldLabel(go, agent.Name[0].ToString(), 0.28f, Vector3.zero);

                _tokens[agent] = go;
            }
        }

        // ── Refresh ──────────────────────────────────────────────────────────

        private void Refresh()
        {
            var gc    = GameController.Instance;
            var state = gc.State;

            // Update zone node colours.
            foreach (var (zone, node) in _nodes)
            {
                bool smoked = state.ActiveZoneEffects
                    .Any(e => e.Zone == zone && e.EffectType == EffectType.Smoke);

                bool isTarget    = gc.HighlightedZones.Contains(zone) &&
                                   gc.Input == InputMode.SelectTargetZone;
                bool isMove      = gc.HighlightedZones.Contains(zone) &&
                                   gc.Input == InputMode.SelectMoveZone;
                bool isSpikeZone = state.SpikeZone == zone;

                Color fill;
                if (smoked)         fill = new Color(0.30f, 0.30f, 0.30f);
                else if (isSpikeZone) fill = new Color(1.00f, 0.65f, 0.00f);
                else if (isTarget)  fill = new Color(0.20f, 0.85f, 0.35f);
                else if (isMove)    fill = new Color(0.20f, 0.80f, 0.90f);
                else                fill = ControlColour(zone.ControlStatus);

                node.SetColor(fill);
            }

            // Update agent token positions (stack agents per zone with small offsets).
            var slotIndex = new Dictionary<Zone, int>();

            foreach (var (agent, go) in _tokens)
            {
                if (!agent.IsAlive) { go.SetActive(false); continue; }
                go.SetActive(true);

                if (!Layout.TryGetValue(agent.CurrentZone.ZoneName, out var zonePos)) continue;

                slotIndex.TryGetValue(agent.CurrentZone, out int slot);
                slotIndex[agent.CurrentZone] = slot + 1;

                // Arrange tokens in a small arc around the zone centre.
                float angle   = slot * 72f * Mathf.Deg2Rad;
                var   offset  = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * 0.48f;
                go.transform.position = (Vector3)zonePos + offset;
            }
        }

        private static Color ControlColour(ZoneType status) => status switch
        {
            ZoneType.ControlledByTeamA => new Color(0.25f, 0.50f, 0.95f, 0.8f),
            ZoneType.ControlledByTeamB => new Color(0.95f, 0.30f, 0.30f, 0.8f),
            ZoneType.Contested         => new Color(0.95f, 0.80f, 0.15f, 0.8f),
            _                          => new Color(0.42f, 0.42f, 0.42f, 0.8f),
        };

        // ── Helpers ──────────────────────────────────────────────────────────

        private static SpriteRenderer CreateCircle(GameObject parent, float radius, Color colour)
        {
            var go = new GameObject("Circle");
            go.transform.SetParent(parent.transform, false);
            go.transform.localScale = Vector3.one * radius * 2f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite(32);
            sr.color  = colour;
            return sr;
        }

        private static TextMesh CreateWorldLabel(GameObject parent, string text, float size, Vector3 localOffset)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = localOffset;

            var tm = go.AddComponent<TextMesh>();
            tm.text          = text;
            tm.characterSize = size;
            tm.anchor        = TextAnchor.MiddleCenter;
            tm.alignment     = TextAlignment.Center;
            tm.color         = Color.white;
            tm.fontSize      = 24;
            return tm;
        }

        // Generates a circle sprite procedurally using a texture.
        private static Sprite CreateCircleSprite(int segments)
        {
            int   size    = 128;
            var   tex     = new Texture2D(size, size);
            float centre  = size / 2f;
            float radius  = size / 2f - 1f;

            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dist = Mathf.Sqrt((x - centre) * (x - centre) + (y - centre) * (y - centre));
                tex.SetPixel(x, y, dist <= radius ? Color.white : Color.clear);
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
        }
    }

    // Component attached to each zone GameObject.
    // Forwards clicks to GameController.
    public class ZoneNode : MonoBehaviour
    {
        public Zone Zone { get; private set; }
        private SpriteRenderer _fill;
        private TextMesh       _label;

        public void Init(Zone zone, SpriteRenderer fill, TextMesh label)
        {
            Zone   = zone;
            _fill  = fill;
            _label = label;
        }

        public void SetColor(Color c) => _fill.color = c;

        private void OnMouseDown()
        {
            GameController.Instance?.PlayerSelectZone(Zone);
        }
    }
}
