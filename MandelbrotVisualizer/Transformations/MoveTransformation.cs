using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotVisualizer.Transformations
{
    public class MoveTransformation : ITransformation
    {
        const int NumerOfSteps = 10;

        private MandelbrotPosition startPosition;
        private MandelbrotPosition targetPosition;
        private MandelbrotPosition currentPosition;
        private OpenTK.Vector2d stepVector;
        private TransformationState state;

        public MoveTransformation(MandelbrotPosition start, MandelbrotPosition target)
        {
            startPosition = start;
            currentPosition = new MandelbrotPosition { CenterPoint = new OpenTK.Vector2d(start.CenterPoint.X, start.CenterPoint.Y), Scale = start.Scale };
            targetPosition = target;
            stepVector = (targetPosition.CenterPoint - startPosition.CenterPoint) / NumerOfSteps;
            state = TransformationState.Active;
        }

        public MandelbrotPosition CurrentPosition { get => currentPosition; set => currentPosition = value; }
        public MandelbrotPosition TargetPosition { get => targetPosition; set => targetPosition = value; }
        public MandelbrotPosition StartPosition { get => startPosition; set => startPosition = value; }
        public TransformationState State { get => state; set => state = value; }

        public MandelbrotPosition Tick()
        {
            currentPosition.CenterPoint += stepVector;
            double currentDelta = (targetPosition.CenterPoint - currentPosition.CenterPoint).LengthSquared;
            OpenTK.Vector2d nextPosition = currentPosition.CenterPoint + stepVector;
            double nextDelta = (targetPosition.CenterPoint - nextPosition).LengthSquared;
            if (nextDelta > currentDelta)
                currentPosition.CenterPoint = targetPosition.CenterPoint;
            if (currentPosition.CenterPoint.Equals(targetPosition.CenterPoint))
                state = TransformationState.Finished;
            return currentPosition;
        }
    }
}
