using Microsoft.Xna.Framework;

namespace Tactile
{
    class Camera
    {
        protected Matrix transformation_matrix;
        protected int width, height;
        public Vector2 pos;
        protected Vector2 scale;
        public float angle;
        public Vector2 offset;

        public Camera(int width, int height, Vector2 pos)
        {
            this.width = width;
            this.height = height;
            this.pos = pos;
            angle = 0;
            scale = Vector2.One;
            offset = new Vector2(width / 2, height / 2);
        }

        public Vector2 zoom
        {
            set
            {
                scale.X = (value.X <= 0.001f ? 0.001f : value.X);
                scale.Y = (value.Y <= 0.001f ? 0.001f : value.Y);
            }
            get { return scale; }
        }

        public Matrix matrix
        {
            get
            {
                transformation_matrix = Matrix.Identity *
                        Matrix.CreateTranslation(new Vector3(-offset.X, -offset.Y, 0)) *
                        Matrix.CreateRotationZ(angle) *
                        Matrix.CreateScale(scale.X, scale.Y, 1) *
                        Matrix.CreateTranslation(new Vector3(pos.X, pos.Y, 0));
                return transformation_matrix;
            }
        }

        public Vector2 ToWorldLocation(Vector2 position)
        {
            return Vector2.Transform(position, Matrix.Invert(transformation_matrix));
        }
        public Vector2 ToLocalLocation(Vector2 position)
        {
            return Vector2.Transform(position, transformation_matrix);
        }
    }
}
