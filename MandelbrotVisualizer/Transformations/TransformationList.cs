using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotVisualizer.Transformations
{
    public class TransformationList
    {
        public readonly List<ITransformation> TList = new List<ITransformation>();

        public MandelbrotPosition Tick()
        {
            if (TList.Count > 0)
            {
                ITransformation currentTransformation = TList.First();
                MandelbrotPosition transfromResult = currentTransformation.Tick();
                if (currentTransformation.State == TransformationState.Finished)
                    TList.Remove(currentTransformation);
                return transfromResult;
            }
            else
                return null;
        }

        public void AddTransformations(MandelbrotPosition start, MandelbrotPosition stop)
        {
            //we zoom out untill both points are on visible on the screen (2 units apart maximum - after scale)
            double deltaPosition = (start.CenterPoint - stop.CenterPoint).Length;
            double screenSpan = 2 * start.Scale;
            if(deltaPosition > screenSpan)
            {
                MandelbrotPosition zoomStop = new MandelbrotPosition { CenterPoint = start.CenterPoint, Scale = deltaPosition / 2 };
                ZoomTransformation zoomOut = new ZoomTransformation(start, zoomStop, false);
                TList.Add(zoomOut);
                start = zoomStop;
            }
            //move center pozition untill it becomes stop's center
            if (!stop.CenterPoint.Equals(start.CenterPoint))
            {
                MandelbrotPosition moveStop = new MandelbrotPosition { CenterPoint = stop.CenterPoint, Scale = start.Scale};
                MoveTransformation move = new MoveTransformation(start, moveStop);
                TList.Add(move);
                start = moveStop;
            }
            //zoom in or our if we need to
            if(stop.Scale != start.Scale)
            {
                ZoomTransformation zoom = new ZoomTransformation(start, stop, stop.Scale < start.Scale);
                TList.Add(zoom);
            }
        }
    }
}
