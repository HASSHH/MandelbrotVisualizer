using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotVisualizer.Transformations
{
    public interface ITransformation
    {
        MandelbrotPosition CurrentPosition { get; set; }
        MandelbrotPosition TargetPosition { get; set; }
        MandelbrotPosition StartPosition { get; set; }
        TransformationState State { get; set; }

        MandelbrotPosition Tick();
    }

    public enum TransformationState
    {
        Active,
        Finished
    }
}
