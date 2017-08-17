using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotVisualizer.Transformations
{
    public class ZoomTransformation : ITransformation
    {
        public const double ZoomFactor = 1.1;

        private MandelbrotPosition startPosition;
        private MandelbrotPosition targetPosition;
        private MandelbrotPosition currentPosition;
        private TransformationState state;
        private double zoom;
        private bool zoomIn;

        public ZoomTransformation(MandelbrotPosition start, MandelbrotPosition target, bool zoomIn)
        {
            startPosition = start;
            currentPosition = new MandelbrotPosition { CenterPoint = new OpenTK.Vector2d(start.CenterPoint.X, start.CenterPoint.Y), Scale = start.Scale };
            targetPosition = target;
            zoom = zoomIn ? 1 / ZoomFactor : ZoomFactor;
            this.zoomIn = zoomIn;
            state = TransformationState.Active;
        }

        public MandelbrotPosition CurrentPosition { get => currentPosition; set => currentPosition = value; }
        public MandelbrotPosition TargetPosition { get => targetPosition; set => targetPosition = value; }
        public MandelbrotPosition StartPosition { get => startPosition; set => startPosition = value; }
        public TransformationState State { get => state; set => state = value; }

        public MandelbrotPosition Tick()
        {
            currentPosition.Scale *= zoom;
            if ((zoomIn && currentPosition.Scale < targetPosition.Scale) ||
                (!zoomIn && currentPosition.Scale > targetPosition.Scale))
                currentPosition.Scale = targetPosition.Scale;
            if (currentPosition.Scale == targetPosition.Scale)
                state = TransformationState.Finished;
            return currentPosition;
        }
    }
}
