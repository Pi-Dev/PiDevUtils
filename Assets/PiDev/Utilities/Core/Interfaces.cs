using UnityEngine;
using System.Collections.Generic;

namespace PiDev.Utilities
{
    public interface IPointsProvider
    {
        List<Vector3> GetPoints();
    }
}