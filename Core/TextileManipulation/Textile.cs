using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TextileManipulation
{
    [Flags]
    public enum RenderStyles
    {
        Wire = 0x01,
        Texture = 0x02,
    }

    public sealed class Textile : IDisposable
    {
        private readonly IList<Stitch> stitches = new List<Stitch>();

        private readonly ManipulationAdapter manipulation;
        private readonly Dictionary<int, List<Stitch>> capturingStitches = new Dictionary<int, List<Stitch>>();

        private static readonly Random random = new Random();

        private static int nextID;

        private Texture2D texture;
        private VertexPositionColor[] linesVertices;
        private VertexPositionTexture[] textureVertices;
        private short[] textureIndices;

        private readonly int columnCount;
        private readonly int rowCount;

        private VertexDeclaration linesVertexDeclaration;
        private VertexDeclaration textureVertexDeclaration;

        public int Id { get; private set; }

        public Textile(int cols,
                       int rows,
                       Vector2 center,
                       float scale,
                       float orientation,
                       Vector3 startColor,
                       Vector3 endColor,
                       Vector2 screenSize)
        {
            Id = nextID++;
            columnCount = cols;
            rowCount = rows;

            Vector2 clothSize = new Vector2(
                cols * TextileConstants.LengthPerStitch,
                rows * TextileConstants.LengthPerStitch);

            Vector2 centerOffset = clothSize / 2;

            manipulation = new ManipulationAdapter(clothSize, new BoundingRect(screenSize / 2f, screenSize));
            manipulation.Scale = scale;
            manipulation.Center = center;
            manipulation.Orientation = orientation;

            int jointCount = 0;
            for (int col = 0; col < cols; col++)
            {
                for (int row = 0; row < rows; row++)
                {
                    Color color = GetColor(col, row, cols, rows, startColor, endColor);
                    Stitch stitch = new Stitch(
                        this,
                        new Vector2(
                            (col * TextileConstants.LengthPerStitch) - centerOffset.X,
                            (row * TextileConstants.LengthPerStitch) - centerOffset.Y),
                            new Vector2((float)col / cols, (float)row /rows),
                            color);

                    if (row > 0)
                    {
                        Stitch upperStitch = stitches[stitches.Count - 1];
                        upperStitch.ReferencedStitches.Add(stitch);
                        stitch.ReferencingStitches.Add(upperStitch);
                        jointCount++;
                    }
                    if (col > 0)
                    {
                        Stitch leftStitch = stitches[stitches.Count - rows];
                        leftStitch.ReferencedStitches.Add(stitch);
                        stitch.ReferencingStitches.Add(leftStitch);
                        jointCount++;
                   }
                    stitches.Add(stitch);
                }
            }

            linesVertices = new VertexPositionColor[jointCount * 2];
            textureVertices = new VertexPositionTexture[stitches.Count];

            RenderStyle = RenderStyles.Wire | RenderStyles.Texture;

            Ripple(manipulation.Transform(Vector2.Zero), clothSize.Length() * (float)random.NextDouble(), -0.2f * (float)random.NextDouble());
        }

        public void Dispose()
        {
            if (linesVertexDeclaration != null)
            {
                linesVertexDeclaration.Dispose();
                linesVertexDeclaration = null;
            }

            if (textureVertexDeclaration != null)
            {
                textureVertexDeclaration.Dispose();
                textureVertexDeclaration = null;
            }
        }

        private static Color GetColor(int col, int row, int columns, int rows, Vector3 startColor, Vector3 endColor)
        {
            float colFactor = (float)col / columns;
            float rowFactor = (float)row / rows;

            Vector3 diff = endColor - startColor;

            return new Color(new Vector3(
                startColor.X + (diff.X * rowFactor),
                startColor.Y + (diff.Y * colFactor),
                startColor.Z + (diff.Z * (colFactor + rowFactor) / 2.0f)));
        }

        public RenderStyles RenderStyle { get; set; }

        public Texture2D Texture
        {
            get { return texture; }
            set { texture = value; }
        }

        public float Scale
        {
            get { return manipulation.Scale; }
        }

        public float Orientation
        {
            get { return manipulation.Orientation; }
        }

        public Vector2 Center
        {
            get { return manipulation.Center; }
        }

        internal Vector2 Transform(Vector2 position)
        {
            return manipulation.Transform(position);
        }

        /// <summary>
        /// Returns true if contact position hits close to this textile.
        /// </summary>
        /// <param name="position">Contact position in screen coordinates.</param>
        /// <returns></returns>
        internal bool HitTest(Vector2 position)
        {
            float closestDistance = TextileConstants.LengthPerStitch * 0.65f;

            foreach (Stitch stitch in stitches)
            {
                Vector2 distance = stitch.Position - position;
                if (distance.Length() < closestDistance)
                {
                    return true;
                }
            }

            return false;
        }

        internal bool TryAddContact(int contactId, Vector2 position)
        {
            float closestDistance = TextileConstants.LengthPerStitch * 0.65f;
            Stitch closestStitch = null;

            foreach (Stitch stitch in stitches)
            {
                Vector2 distance = stitch.Position - position;
                if (distance.Length() < closestDistance)
                {
                    closestDistance = distance.Length();
                    closestStitch = stitch;
                }
            }
            if (closestStitch != null)
            {
                if (capturingStitches.ContainsKey(contactId) == false)
                {
                    capturingStitches.Add(contactId, new List<Stitch>());
                }
                capturingStitches[contactId].Add(closestStitch);
                manipulation.AddContact(contactId, position);

                return true;
            }

            return false;
        }

        internal bool IsCapturing(int contactId)
        {
            return capturingStitches.ContainsKey(contactId);
        }

        internal void RemoveContact(int contactId, Vector2 position)
        {
            if (capturingStitches.ContainsKey(contactId))
            {
                foreach (Stitch stitch in capturingStitches[contactId])
                {
                    if (stitch.Owner == this)
                    {
                        manipulation.RemoveContact(contactId, position);
                    }
                }
                capturingStitches[contactId].Clear();
                capturingStitches.Remove(contactId);
            }
        }

        public void Ripple(Vector2 epicenter, float diameter, float update)
        {
            foreach (Stitch stitch in stitches)
            {
                stitch.Ripple(epicenter, diameter, update);
            }
        }

        internal int CapturedContactCount
        {
            get
            {
                return capturingStitches.Keys.Count;
            }
        }

        internal void Update(Dictionary<int, Vector2> contactItems)
        {
            foreach (Stitch stitch in stitches)
            {
                stitch.UpdateAcceleration();
            }

            foreach (int contactId in contactItems.Keys)
            {
                if (capturingStitches.ContainsKey(contactId))
                {
                    foreach (Stitch stitch in capturingStitches[contactId])
                    {
                        if (stitch.Owner == this)
                        {
                            Vector2 contactPosition = contactItems[contactId];
                            manipulation.UpdateContact(contactId, contactPosition);
                            stitch.Position = contactPosition;
                            stitch.Move(stitch, 1.0f);
                        }
                    }
                }
            }

            foreach (Stitch stitch in stitches)
            {
                stitch.Update();
            }

            foreach (Stitch stitch in stitches)
            {
                stitch.UpdateRelationalVelocity();
            }

            manipulation.ProcessContacts();
        }

        internal void Draw(GraphicsDevice device, Effect effectLines, Effect effectTexture)
        {
            device.RenderState.MultiSampleAntiAlias = true;

            if ((RenderStyle & RenderStyles.Texture) == RenderStyles.Texture && texture != null)
            {
                effectTexture.Parameters["UserTexture"].SetValue(texture);
                DrawTexture(device, effectTexture);
            }

            if ((RenderStyle & RenderStyles.Wire) == RenderStyles.Wire || texture == null)
            {
                DrawLines(device, effectLines);
            }
        }

        private VertexDeclaration GetLinesVertexDeclaration(GraphicsDevice device)
        {
            if (linesVertexDeclaration == null)
            {
                linesVertexDeclaration = new VertexDeclaration(device, VertexPositionColor.VertexElements);
            }
            return linesVertexDeclaration;
        }

        private VertexDeclaration GetTextureVertexDeclaration(GraphicsDevice device)
        {
            if (textureVertexDeclaration == null)
            {
                textureVertexDeclaration = new VertexDeclaration(device, VertexPositionTexture.VertexElements);
            }
            return textureVertexDeclaration;
        }

        private void DrawLines(GraphicsDevice device, Effect effect)
        {
            UpdateColoredLinesVertices();

            device.RenderState.AlphaBlendEnable = true;
            device.RenderState.SourceBlend = Blend.SourceAlpha;
            device.RenderState.DestinationBlend = Blend.DestinationAlpha;
            device.RenderState.CullMode = CullMode.CullClockwiseFace;
            device.VertexDeclaration = GetLinesVertexDeclaration(device);

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, linesVertices, 0, linesVertices.Length / 2);
                pass.End();
            }
            effect.End();
        }

        private void DrawTexture(GraphicsDevice device, Effect effect)
        {
            UpdateTextureVertices();
            short[] indices = GetTextureIndices(columnCount, rowCount);

            device.RenderState.AlphaBlendEnable = true;
            device.RenderState.AlphaBlendOperation = BlendFunction.Add;
            device.RenderState.SourceBlend = Blend.SourceColor;
            device.RenderState.DestinationBlend = Blend.InverseDestinationAlpha;
            device.RenderState.CullMode = CullMode.None;
            device.VertexDeclaration = GetTextureVertexDeclaration(device);

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.DrawUserIndexedPrimitives<VertexPositionTexture>(
                        PrimitiveType.TriangleList, textureVertices, 0, textureVertices.Length,
                        indices, 0, indices.Length / 3);
                pass.End();
            }
            effect.End();
        }

        private void UpdateColoredLinesVertices()
        {
            int index = 0;
            foreach (Stitch stitch1 in stitches)
            {
                foreach (Stitch stitch2 in stitch1.ReferencedStitches)
                {
                    linesVertices[index].Color = stitch1.Color;
                    linesVertices[index].Position = new Vector3(stitch1.Position, 0);
                    index++;
                    linesVertices[index].Color = stitch2.Color;
                    linesVertices[index].Position = new Vector3(stitch2.Position, 0);
                    index++;
                }
            }
        }

        private void UpdateTextureVertices()
        {
            int index = 0;
            foreach (Stitch stitch in stitches)
            {
                textureVertices[index].Position = new Vector3(stitch.Position, 0f);
                textureVertices[index++].TextureCoordinate = stitch.Coordinate;
            }
        }

        private short[] GetTextureIndices(int cols, int rows)
        {
            if (textureIndices == null)
            {
                int indicesCount = (cols - 1) * (rows - 1) * 2 * 3;
                textureIndices = new short[indicesCount];
                int index = 0;
                for (int col = 0; col < cols - 1; col++)
                {
                    for (int row = 0; row < rows - 1; row++)
                    {
                        int basePosition = (col * rows);
                        textureIndices[index++] = (short)(basePosition + row);
                        textureIndices[index++] = (short)(basePosition + row + rows);
                        textureIndices[index++] = (short)(basePosition + row + 1);

                        textureIndices[index++] = (short)(basePosition + row + rows);
                        textureIndices[index++] = (short)(basePosition + row + rows + 1);
                        textureIndices[index++] = (short)(basePosition + row + 1);
                    }
                }
            }
            return textureIndices;
        }
    }
}
