using System.Collections.Generic;
using System;
using Tetris.Gameplay.Domain;
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
        private readonly RectTransform holdPreviewAnchor;
        private readonly Action pauseRequested;
        private readonly Action resumeRequested;
        private readonly Action restartRequested;
        private readonly Action toggleEffectsRequested;
        private readonly Action menuRequested;

        private readonly Font defaultFont;
        private readonly List<Image> nextPreviewCells = new();
        private readonly List<Image> holdPreviewCells = new();

        private Text scoreValueText;
        private Text linesValueText;
        private Text levelValueText;
        private Text gameOverText;
        private RectTransform pauseOverlay;
        private Text effectsToggleText;
        private RectTransform nextPreviewContentArea;
        private RectTransform holdPreviewContentArea;

        public GameplayHudRenderer(
            RectTransform hudRoot,
            RectTransform scoreAnchor,
            RectTransform nextPreviewAnchor,
            RectTransform holdPreviewAnchor,
            Action pauseRequested,
            Action resumeRequested,
            Action restartRequested,
            Action toggleEffectsRequested,
            Action menuRequested)
        {
            this.hudRoot = hudRoot;
            this.scoreAnchor = scoreAnchor;
            this.nextPreviewAnchor = nextPreviewAnchor;
            this.holdPreviewAnchor = holdPreviewAnchor;
            this.pauseRequested = pauseRequested;
            this.resumeRequested = resumeRequested;
            this.restartRequested = restartRequested;
            this.toggleEffectsRequested = toggleEffectsRequested;
            this.menuRequested = menuRequested;
            defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        public void EnsureBuilt()
        {
            EnsureHudBackdrop();
            EnsureScorePanel();
            nextPreviewContentArea = EnsurePreviewPanel(nextPreviewAnchor, "NextPanel", "NEXT");
            holdPreviewContentArea = EnsurePreviewPanel(holdPreviewAnchor, "HoldPanel", "HOLD");
            EnsureGameOverText();
            EnsurePauseButton();
            EnsurePauseOverlay();
        }

        public void Render(GameplayRuntime runtime, bool isPaused, bool reducedEffects)
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

            if (pauseOverlay != null)
            {
                pauseOverlay.gameObject.SetActive(isPaused && !runtime.IsGameOver);
            }

            if (effectsToggleText != null)
            {
                effectsToggleText.text = reducedEffects ? "Effects: Reduced" : "Effects: Full";
            }

            RenderPreview(runtime.NextPiece, nextPreviewContentArea, nextPreviewCells);
            RenderPreview(runtime.HeldPiece, holdPreviewContentArea, holdPreviewCells);
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
            scoreValueText = EnsureLabel(scoreAnchor, "ScoreValue", new Vector2(0.04f, 0.62f), new Vector2(0.96f, 0.98f), 48, TextAnchor.MiddleLeft);
            linesValueText = EnsureLabel(scoreAnchor, "LinesValue", new Vector2(0.04f, 0.30f), new Vector2(0.96f, 0.62f), 42, TextAnchor.MiddleLeft);
            levelValueText = EnsureLabel(scoreAnchor, "LevelValue", new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.30f), 42, TextAnchor.MiddleLeft);

            scoreValueText.text = "Score 0";
            linesValueText.text = "Lines 0";
            levelValueText.text = "Level 1";
        }

        private RectTransform EnsurePreviewPanel(RectTransform anchor, string panelName, string title)
        {
            if (anchor == null)
            {
                return null;
            }

            EnsurePanelImage(anchor, panelName, new Color(0.04f, 0.06f, 0.12f, 0.94f), 0);
            EnsureLabel(anchor, $"{title}Title", new Vector2(0.05f, 0.60f), new Vector2(0.95f, 0.96f), 36, TextAnchor.MiddleCenter).text = title;
            return EnsurePreviewContentArea(anchor, $"{title}PreviewContentArea");
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

        private void RenderPreview(PieceDefinition piece, RectTransform contentArea, List<Image> cellPool)
        {
            if (contentArea == null)
            {
                return;
            }

            for (var i = 0; i < cellPool.Count; i++)
            {
                cellPool[i].gameObject.SetActive(false);
            }

            if (piece == null)
            {
                return;
            }

            var shape = piece.GetRotationState(piece.SpawnRotationIndex);
            EnsurePreviewPool(cellPool, contentArea, shape.Length);

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
            var gridSize = Mathf.Min(contentArea.rect.width, contentArea.rect.height) * 0.82f;
            var step = gridSize / Mathf.Max(width, height);
            var centerX = (minX + maxX) * 0.5f;
            var centerY = (minY + maxY) * 0.5f;

            for (var i = 0; i < shape.Length; i++)
            {
                var cell = shape[i];
                var image = cellPool[i];
                image.gameObject.SetActive(true);
                image.color = piece.TokenColor;

                var rect = (RectTransform)image.transform;
                rect.sizeDelta = new Vector2(step * 0.82f, step * 0.82f);
                rect.anchoredPosition = new Vector2(
                    (cell.X - centerX) * step,
                    (cell.Y - centerY) * step);
            }
        }

        private void EnsurePauseButton()
        {
            if (hudRoot == null)
            {
                return;
            }

            var button = EnsureButton(hudRoot, "PauseButton", "II", new Vector2(0.915f, 0.13f), new Vector2(0.985f, 0.52f), pauseRequested);
            var buttonImage = button.GetComponent<Image>();
            buttonImage.color = new Color(0.09f, 0.17f, 0.3f, 0.88f);
        }

        private void EnsurePauseOverlay()
        {
            if (hudRoot == null)
            {
                return;
            }

            pauseOverlay = EnsureContainer(hudRoot, "PauseOverlay", new Vector2(0f, 0f), new Vector2(1f, 1f));
            pauseOverlay.GetComponent<Image>().color = new Color(0.01f, 0.02f, 0.05f, 0.82f);
            pauseOverlay.gameObject.SetActive(false);

            EnsureLabel(pauseOverlay, "PauseTitle", new Vector2(0.2f, 0.68f), new Vector2(0.8f, 0.86f), 30, TextAnchor.MiddleCenter).text = "PAUSED";
            EnsureButton(pauseOverlay, "ResumeButton", "Resume", new Vector2(0.26f, 0.52f), new Vector2(0.74f, 0.64f), resumeRequested);
            EnsureButton(pauseOverlay, "RestartButton", "Restart Run", new Vector2(0.26f, 0.38f), new Vector2(0.74f, 0.5f), restartRequested);
            var effectsButton = EnsureButton(pauseOverlay, "EffectsButton", "Effects", new Vector2(0.26f, 0.24f), new Vector2(0.74f, 0.36f), toggleEffectsRequested);
            effectsToggleText = effectsButton.GetComponentInChildren<Text>();

            if (menuRequested != null)
            {
                EnsureButton(pauseOverlay, "MenuButton", "Menu (placeholder)", new Vector2(0.26f, 0.10f), new Vector2(0.74f, 0.22f), menuRequested);
            }
        }

        private Button EnsureButton(RectTransform parent, string name, string label, Vector2 minAnchor, Vector2 maxAnchor, Action callback)
        {
            var rect = EnsureContainer(parent, name, minAnchor, maxAnchor);
            var button = rect.GetComponent<Button>() ?? rect.gameObject.AddComponent<Button>();
            button.onClick.RemoveAllListeners();
            if (callback != null)
            {
                button.onClick.AddListener(() => callback.Invoke());
            }

            var image = rect.GetComponent<Image>();
            image.color = new Color(0.08f, 0.12f, 0.22f, 0.92f);
            var text = EnsureLabel(rect, $"{name}Label", new Vector2(0.06f, 0.1f), new Vector2(0.94f, 0.9f), 16, TextAnchor.MiddleCenter);
            text.text = label;
            return button;
        }

        private static RectTransform EnsureContainer(RectTransform parent, string name, Vector2 minAnchor, Vector2 maxAnchor)
        {
            var child = parent.Find(name) as RectTransform;
            if (child == null)
            {
                child = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
                child.SetParent(parent, false);
            }

            child.anchorMin = minAnchor;
            child.anchorMax = maxAnchor;
            child.offsetMin = Vector2.zero;
            child.offsetMax = Vector2.zero;
            return child;
        }

        private static void EnsurePreviewPool(List<Image> cellPool, RectTransform parent, int count)
        {
            while (cellPool.Count < count)
            {
                var cell = new GameObject($"PreviewCell_{cellPool.Count}", typeof(RectTransform), typeof(Image));
                cell.transform.SetParent(parent, false);
                var image = cell.GetComponent<Image>();
                image.raycastTarget = false;
                var rect = (RectTransform)cell.transform;
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                cellPool.Add(image);
            }
        }

        private static RectTransform EnsurePreviewContentArea(RectTransform parent, string areaName)
        {
            var area = parent.Find(areaName) as RectTransform;
            if (area == null)
            {
                area = new GameObject(areaName, typeof(RectTransform)).GetComponent<RectTransform>();
                area.SetParent(parent, false);
            }

            area.anchorMin = new Vector2(0.08f, 0.07f);
            area.anchorMax = new Vector2(0.92f, 0.68f);
            area.offsetMin = Vector2.zero;
            area.offsetMax = Vector2.zero;
            area.pivot = new Vector2(0.5f, 0.5f);
            return area;
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
