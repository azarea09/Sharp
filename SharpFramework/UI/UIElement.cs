using System;
using System.Drawing;
using System.Numerics;
using Raylib_cs;
using SharpFramework.Graphics;
using SharpFramework.Utils;

namespace SharpFramework.UI
{
    public abstract class UIElement
    {
        public Vector2 Position;
        public Vector2 Size;

        public bool IsHovered { get; private set; }
        public bool IsPressed { get; private set; }
        public bool WasClicked { get; private set; }

        protected virtual void OnClick() { }

        public void Update()
        {
            var mouse = Raylib.GetMousePosition();
            RectangleF bounds = new(Position.X, Position.Y, Size.X, Size.Y);

            IsHovered = bounds.Contains(mouse.X, mouse.Y);
            WasClicked = false;

            if (IsHovered)
            {
                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    IsPressed = true;
                }

                if (Raylib.IsMouseButtonReleased(MouseButton.Left) && IsPressed)
                {
                    IsPressed = false;
                    WasClicked = true;
                    OnClick();
                }
            }
            else
            {
                IsPressed = false;
            }
        }

        public abstract void Draw();
    }
}
