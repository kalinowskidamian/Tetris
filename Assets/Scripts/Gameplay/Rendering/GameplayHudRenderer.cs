using System.Collections.Generic;
using Tetris.Gameplay.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace Tetris.Gameplay.Rendering
{
    public sealed class GameplayHudRenderer
    {
        private readonly RectTransform hudRoot;
        private readonly RectTransform scoreAnchor;
        private readonly RectTransform nextPreviewAnchor;

        private readonly Font defaultFont;
        private readonly List<Image> previewCells = new();

        private Text scoreValueText;
        private Text linesValueText;
        private Text levelValueText;
        private Text gameOverText;
        private RectTransform previewPanel;

        public GameplayHudRenderer(RectTransform hudRoot, RectTransform scoreAnchor, RectTransform nextPreviewAnchor)
        {
            this.hudRoot = hudRoot;
            this.scoreAnchor = scoreAnchor;
            this.nextPreviewAnchor = nextPreviewAnchor;
            defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        public void EnsureBuilt()
        {
            EnsureHudBackdrop();
            EnsureScorePanel();
            EnsurePreviewPanel();
            EnsureGameOverText();
        }

        public void Render(GameplayRuntime runtime)
        {
            if (runtime == null)
            {
                return;
            }

            EnsureBuilt();

            if (scoreValueText != null)
            {
                scoreValueText.text = $"Score  {runtime.Score}";
            }

            if (linesValueText != null)
            {
                linesValueText.text = $"Lines   {runtime.LinesCleared}";
            }

            if (levelValueText != null)
            {
                levelValueText.text = $"Level   {runtime.Level}";
            }

            if (gameOverText != null)
            {
                gameOverText.gameObject.SetActive(runtime.IsGameOver);
            }

            RenderNextPreview(runtime);
        }

        private void EnsureHudBackdrop()
        {
            if (hudRoot == null)
            {
                return;
            }

            EnsurePanelImage(hudRoot, "HudBackdrop", new Color(0.02f, 0.03f, 0.08f, 0.92f), 0);
            EnsurePanelImage(hudRoot, "HudAccent", new Color(0.09f, 0.52f, 0.85f, 0.20f), 1);
        }

        private void EnsureScorePanel()
        {
            if (scoreAnchor == null)
            {
                return;
            }

            EnsurePanelImage(scoreAnchor, "ScorePanel", new Color(0.04f, 0.06f, 0.12f, 0.94f), 0);
            scoreValueText = EnsureLabel(scoreAnchor, "ScoreValue", new Vector2(0.04f, 0.58f), new Vector2(0.96f, 0.93f), 32, TextAnchor.MiddleLeft);
            linesValueText = EnsureLabel(scoreAnchor, "LinesValue", new Vector2(0.04f, 0.29f), new Vector2(0.96f, 0.56f), 24, TextAnchor.MiddleLeft);
            levelValueText = EnsureLabel(scoreAnchor, "LevelValue", new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.27f), 24, TextAnchor.MiddleLeft);

            scoreValueText.text = "Score 0";
            linesValueText.text = "Lines 0";
            levelValueText.text = "Level 1";
        }

        private void EnsurePreviewPanel()
        {
            if (nextPreviewAnchor == null)
            {
                return;
            }

            previewPanel = nextPreviewAnchor;
            EnsurePanelImage(previewPanel, "NextPanel", new Color(0.04f, 0.06f, 0.12f, 0.94f), 0);
            EnsureLabel(previewPanel, "NextTitle", new Vector2(0.1f, 0.72f), new Vector2(0.9f, 0.95f), 22, TextAnchor.MiddleCenter).text = "NEXT";
            EnsurePreviewPool(4);
        }

        private void EnsureGameOverText()
        {
            if (hudRoot == null)
            {
                return;
            }

            gameOverText = EnsureLabel(hudRoot, "GameOverText", new Vector2(0.1f, -0.3f), new Vector2(0.9f, 0.1f), 30, TextAnchor.MiddleCenter, useStretch: false);
            gameOverText.text = "GAME OVER\nTap to Restart";
            gameOverText.color = new Color(1f, 0.3f, 0.64f, 1f);
            gameOverText.gameObject.SetActive(false);
        }

        private void RenderNextPreview(GameplayRuntime runtime)
        {
            if (previewPanel == null)
            {
                return;
            }

            var shape = runtime.NextPiece.GetRotationState(runtime.NextPiece.SpawnRotationIndex);
            EnsurePreviewPool(shape.Length);

            var minX = int.MaxValue;
            var maxX = int.MinValue;
            var minY = int.MaxValue;
            var maxY = int.MinValue;

            for (var i = 0; i < shape.Length; i++)
            {
                var cell = shape[i];
                minX = Mathf.Min(minX, cell.X);
                maxX = Mathf.Max(maxX, cell.X);
                minY = Mathf.Min(minY, cell.Y);
                maxY = Mathf.Max(maxY, cell.Y);
            }

            var width = maxX - minX + 1;
            var height = maxY - minY + 1;
            var gridSize = Mathf.Min(previewPanel.rect.width * 0.7f, previewPanel.rect.height * 0.55f);
            var step = gridSize / Mathf.Max(width, height);
            var startX = -((width - 1) * step) * 0.5f;
            var startY = ((height - 1) * step) * 0.5f;
            var centerOffset = new Vector2(previewPanel.rect.width * 0.5f, previewPanel.rect.height * 0.32f);

            for (var i = 0; i < previewCells.Count; i++)
            {
                previewCells[i].gameObject.SetActive(false);
            }

            for (var i = 0; i < shape.Length; i++)
            {
                var cell = shape[i];
                var image = previewCells[i];
                image.gameObject.SetActive(true);
                image.color = runtime.NextPiece.TokenColor;

                var rect = (RectTransform)image.transform;
                rect.sizeDelta = new Vector2(step * 0.84f, step * 0.84f);
                rect.anchoredPosition = new Vector2(
                    centerOffset.x + startX + (cell.X - minX) * step,
                    centerOffset.y + startY - (cell.Y - minY) * step);
            }
        }

        private void EnsurePreviewPool(int count)
        {
            while (previewCells.Count < count)
            {
                var cell = new GameObject($"PreviewCell_{previewCells.Count}", typeof(RectTransform), typeof(Image));
                cell.transform.SetParent(previewPanel, false);
                var image = cell.GetComponent<Image>();
                image.raycastTarget = false;
                previewCells.Add(image);
            }
        }

        private Image EnsurePanelImage(RectTransform parent, string name, Color color, int siblingIndex)
        {
            var panel = parent.Find(name) as RectTransform;
            if (panel == null)
            {
                panel = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
                panel.SetParent(parent, false);
                panel.anchorMin = Vector2.zero;
                panel.anchorMax = Vector2.one;
                panel.offsetMin = Vector2.zero;
                panel.offsetMax = Vector2.zero;
            }

            panel.SetSiblingIndex(siblingIndex);
            var image = panel.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private Text EnsureLabel(RectTransform parent, string name, Vector2 minAnchor, Vector2 maxAnchor, int fontSize, TextAnchor anchor, bool useStretch = true)
        {
            var existing = parent.Find(name);
            RectTransform textRect;
            Text text;

            if (existing == null)
            {
                textRect = new GameObject(name, typeof(RectTransform), typeof(Text)).GetComponent<RectTransform>();
                textRect.SetParent(parent, false);
                text = textRect.GetComponent<Text>();
            }
            else
            {
                textRect = existing.GetComponent<RectTransform>();
                text = existing.GetComponent<Text>();
            }

            if (useStretch)
            {
                textRect.anchorMin = minAnchor;
                textRect.anchorMax = maxAnchor;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
            }
            else
            {
                textRect.anchorMin = new Vector2(0.5f, 0.5f);
                textRect.anchorMax = new Vector2(0.5f, 0.5f);
                textRect.pivot = new Vector2(0.5f, 0.5f);
                textRect.anchoredPosition = Vector2.zero;
                textRect.sizeDelta = new Vector2(680f, 180f);
            }

            text.font = defaultFont;
            text.alignment = anchor;
            text.fontSize = fontSize;
            text.color = new Color(0.76f, 0.9f, 1f, 1f);
            text.raycastTarget = false;
            return text;
        }
    }
}
