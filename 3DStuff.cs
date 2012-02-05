using OpenTK;

namespace SM64DSe
{
    public enum RenderMode
    {
        Opaque = 1,
        Translucent,
        Picking
    }

    public struct BoundingBox
    {
        public Vector3 m_Min, m_Max;

        /*public BoundingBox()
        {
            m_Min = m_Max = Vector3.Zero;
        }*/
    }
}
