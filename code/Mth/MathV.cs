

namespace Sandcube.Mth;

public static class MathV
{
    public const float InchesInMeter = 39.3700787402f;

    //        6_______5
    //       /|      /|
    //     7/_|____4/ |
    //      | |     | | 
    //      | |2____|_|1         z  x
    //      | /     | /          | /
    //      |/______|/        y__|/
    //      3 front 0 
    public static readonly Vector3[] InchCubeCorners = GetCubeCorners(1);
    public static readonly Vector3[] MeterCubeCorners = GetCubeCorners(InchesInMeter);

    public static readonly Vector3[] MeterCubeTopDownCenters = new Vector3[4]
    {
        (MeterCubeCorners[0] + MeterCubeCorners[4]) / 2f,
        (MeterCubeCorners[1] + MeterCubeCorners[5]) / 2f,
        (MeterCubeCorners[2] + MeterCubeCorners[6]) / 2f,
        (MeterCubeCorners[3] + MeterCubeCorners[7]) / 2f,
    };

    public static Vector3[] GetCubeCorners(Vector3 size)
    {
        return new Vector3[8]
        {
            Vector3.Zero,
            new Vector3(size.x, 0f, 0f),
            new Vector3(size.x, size.y, 0f),
            new Vector3(0f, size.y, 0f),
            new Vector3(0f, 0f, size.z),
            new Vector3(size.x, 0f, size.z),
            new Vector3(size.x, size.y, size.z),
            new Vector3(0f, size.y, size.z)
        };
    }
}
